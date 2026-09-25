using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Extrae datos de un CV (texto obtenido vía PdfTextExtractor, sin depender del formato de ningún
// portal externo) para precargar la carga masiva de candidatos. Mismo patrón que
// PerfilCargoIAService — el resultado se devuelve para revisión humana, nunca se persiste directo.
public class PostulanteIAService(IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbFactory)
{
    // Precios Azure OpenAI gpt-5.1 (setiembre 2026): USD 1.38 / millón tokens entrada, USD 11.00 / millón salida.
    private const decimal CostoPorMillonTokensEntradaUsd = 1.38m;
    private const decimal CostoPorMillonTokensSalidaUsd = 11.00m;

    public record CvExtraido(
        string? Nombre,
        string? RUT,
        string? Correo,
        string? Telefono,
        string? Sexo,
        int? Edad,
        string? Comuna,
        int? AniosExperiencia,
        string? RubroExperiencia);

    public async Task<CvExtraido> ExtraerCvAsync(string textoDocumento, int clienteId, int solicitudId, string usuarioId)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Falta la configuración 'AzureOpenAI:Endpoint'.");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Falta la configuración 'AzureOpenAI:ApiKey'.");
        var deployment = configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Falta la configuración 'AzureOpenAI:DeploymentName'.");

        var clienteOpenAI = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
        var chatClient = clienteOpenAI.GetChatClient(deployment);

        List<ChatMessage> mensajes =
        [
            new SystemChatMessage(PromptSistema),
            new UserChatMessage(textoDocumento),
        ];

        var opciones = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
        };

        var respuesta = await CompletarConReintentoAsync(chatClient, mensajes, opciones);
        var json = respuesta.Value.Content[0].Text;

        var resultado = JsonSerializer.Deserialize<CvExtraido>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("La IA no devolvió un resultado interpretable.");

        var tokensEntrada = respuesta.Value.Usage.InputTokenCount;
        var tokensSalida = respuesta.Value.Usage.OutputTokenCount;
        var costo = (tokensEntrada * CostoPorMillonTokensEntradaUsd + tokensSalida * CostoPorMillonTokensSalidaUsd) / 1_000_000m;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.InteraccionesIA.Add(new InteraccionIA
        {
            ClienteId = clienteId,
            SolicitudId = solicitudId,
            ReclutadorId = usuarioId,
            Proveedor = "Azure OpenAI",
            Proposito = "ExtraccionCV",
            Modelo = deployment,
            TokensEntrada = tokensEntrada,
            TokensSalida = tokensSalida,
            CostoUsd = costo,
        });
        await db.SaveChangesAsync();

        return resultado;
    }

    // El límite de tokens por minuto del despliegue se recupera solo: ante un 429 se espera y se
    // reintenta en vez de descartar el CV (además del reintento corto que ya trae el cliente de Azure).
    private static async Task<ClientResult<ChatCompletion>> CompletarConReintentoAsync(ChatClient chatClient, List<ChatMessage> mensajes, ChatCompletionOptions opciones)
    {
        var esperas = new[] { 15, 30, 60 };
        for (var intento = 0; ; intento++)
        {
            try
            {
                return await chatClient.CompleteChatAsync(mensajes, opciones);
            }
            catch (ClientResultException ex) when (ex.Status == 429 && intento < esperas.Length)
            {
                await Task.Delay(TimeSpan.FromSeconds(esperas[intento]));
            }
        }
    }

    private const string PromptSistema = """
        Eres un asistente que ayuda a un reclutador de SOS Group (empresa chilena de externalización
        de personal) a extraer datos de un currículum (CV). Se te entrega el texto extraído de un CV
        en PDF. Léelo y devuelve un JSON con exactamente este formato. Si un dato no aparece
        explícitamente en el texto, usa null — no inventes ni infieras información que no esté escrita.

        {
          "nombre": string o null,
          "rut": string o null (formato chileno, ej. "12.345.678-9"),
          "correo": string o null,
          "telefono": string o null,
          "sexo": "Femenino" | "Masculino" | null,
          "edad": number o null,
          "comuna": string o null (comuna de residencia en Chile),
          "aniosExperiencia": number o null (años totales de experiencia laboral),
          "rubroExperiencia": string o null (rubro/área principal de experiencia, ej. "Ventas", "Remuneraciones y RRHH")
        }
        """;
}
