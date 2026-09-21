using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using dashboard.Models.Reclutamiento;

namespace dashboard.Data;

// Analiza el texto de un perfil de cargo entregado por un cliente (extraído del PDF vía
// PdfTextExtractor) y lo estructura en el formato estándar SOS Group. El resultado se devuelve
// para que el usuario lo revise antes de guardarlo — nunca se persiste directo en PerfilCargoVersion.
public class PerfilCargoIAService(IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbFactory)
{
    // Precios Azure OpenAI gpt-5.1 (setiembre 2026): USD 1.38 / millón tokens entrada, USD 11.00 / millón salida.
    private const decimal CostoPorMillonTokensEntradaUsd = 1.38m;
    private const decimal CostoPorMillonTokensSalidaUsd = 11.00m;

    public record FuncionResultado(string Nombre, List<string> Tareas);

    public record AnalisisResultado(
        string? Area,
        string? ReportaA,
        string? ObjetivoCargo,
        string? EducacionMinima,
        int? AniosExperienciaMinimo,
        string? ConocimientosTecnicos,
        string? Habilidades,
        string? CondicionesEspeciales,
        string? RentaFija,
        string? RentaVariable,
        string? Beneficios,
        List<FuncionResultado> Funciones);

    public async Task<AnalisisResultado> AnalizarAsync(string textoDocumento, int clienteId, int? perfilCargoVersionId, string usuarioId)
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

        var respuesta = await chatClient.CompleteChatAsync(mensajes, opciones);
        var json = respuesta.Value.Content[0].Text;

        var resultadoCrudo = JsonSerializer.Deserialize<AnalisisResultado>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("La IA no devolvió un resultado interpretable.");
        var resultado = resultadoCrudo with { Funciones = resultadoCrudo.Funciones ?? [] };

        var tokensEntrada = respuesta.Value.Usage.InputTokenCount;
        var tokensSalida = respuesta.Value.Usage.OutputTokenCount;
        var costo = (tokensEntrada * CostoPorMillonTokensEntradaUsd + tokensSalida * CostoPorMillonTokensSalidaUsd) / 1_000_000m;

        await using var db = await dbFactory.CreateDbContextAsync();
        db.InteraccionesIA.Add(new InteraccionIA
        {
            ClienteId = clienteId,
            PerfilCargoVersionId = perfilCargoVersionId,
            ReclutadorId = usuarioId,
            Proveedor = "Azure OpenAI",
            Proposito = "AnalisisPerfilCargo",
            Modelo = deployment,
            TokensEntrada = tokensEntrada,
            TokensSalida = tokensSalida,
            CostoUsd = costo,
        });
        await db.SaveChangesAsync();

        return resultado;
    }

    private const string PromptSistema = """
        Eres un asistente que ayuda a SOS Group (empresa chilena de externalización de personal) a
        estandarizar perfiles de cargo. Se te entrega el texto extraído de un documento de perfil de
        cargo entregado por un cliente. Léelo y devuelve un JSON con exactamente este formato,
        completando cada campo con la información presente en el texto. Si un dato no aparece, usa
        null (o lista vacía en "funciones"). No inventes información que no esté en el texto.

        {
          "area": string o null,
          "reportaA": string o null,
          "objetivoCargo": string o null,
          "educacionMinima": string o null,
          "aniosExperienciaMinimo": number o null,
          "conocimientosTecnicos": string o null,
          "habilidades": string o null,
          "condicionesEspeciales": string o null,
          "rentaFija": string o null,
          "rentaVariable": string o null,
          "beneficios": string o null,
          "funciones": [
            { "nombre": "nombre de la categoría de función", "tareas": ["tarea 1", "tarea 2"] }
          ]
        }
        """;
}
