using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using dashboard.Data;

namespace dashboard.Components.Account;

// Envía correo real como notificaciones@sosgroup.cl vía Microsoft Graph (app-only), en el tenant
// propio de SOS Group — reemplaza al envío por Azure Communication Services.
public sealed class CorreoSistemaService : IEmailSender<ApplicationUser>
{
    private static readonly HttpClient Http = new();

    private readonly ClientSecretCredential credential;
    private readonly string buzonRemitente;
    private readonly ILogger<CorreoSistemaService> logger;

    public CorreoSistemaService(IConfiguration configuration, ILogger<CorreoSistemaService> logger)
    {
        var tenantId = configuration["Graph:TenantId"] ?? throw new InvalidOperationException("Falta 'Graph:TenantId'.");
        var clientId = configuration["Graph:ClientId"] ?? throw new InvalidOperationException("Falta 'Graph:ClientId'.");
        var clientSecret = configuration["Graph:ClientSecret"] ?? throw new InvalidOperationException("Falta 'Graph:ClientSecret'.");
        buzonRemitente = configuration["Graph:BuzonRemitente"] ?? throw new InvalidOperationException("Falta 'Graph:BuzonRemitente'.");

        credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        this.logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        EnviarAsync(email, "Confirma tu cuenta - SOS Group", $"<p>Confirma tu cuenta haciendo <a href='{confirmationLink}'>clic aquí</a>.</p>");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        EnviarAsync(email, "Recuperar contraseña - SOS Group", $"<p>Hola {user.Nombre},</p><p>Para restablecer tu contraseña haz <a href='{resetLink}'>clic aquí</a>. Si no solicitaste este cambio, ignora este correo.</p>");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        EnviarAsync(email, "Código de recuperación - SOS Group", $"<p>Hola {user.Nombre},</p><p>Tu código de recuperación es: <b>{resetCode}</b></p>");

    // Reusa el mismo envío por Graph para destinatarios que no son ApplicationUser (ej. candidatos
    // del portal público de postulación, que no tienen cuenta de Identity).
    public Task EnviarCorreoGenericoAsync(string destinatario, string asunto, string cuerpoHtml) =>
        EnviarAsync(destinatario, asunto, cuerpoHtml);

    // Task.Run: una llamada HTTP saliente disparada directamente desde un circuito interactivo de
    // Blazor Server puede colgarse por conflicto entre el SynchronizationContext del circuito y el
    // cliente HTTP interno de Azure.Identity — mismo síntoma ya documentado en GestionO365.
    private Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml) => Task.Run(async () =>
    {
        try
        {
            var token = await credential.GetTokenAsync(new TokenRequestContext(["https://graph.microsoft.com/.default"]));

            var mensaje = new
            {
                message = new
                {
                    subject = asunto,
                    body = new { contentType = "HTML", content = cuerpoHtml },
                    toRecipients = new[] { new { emailAddress = new { address = destinatario } } },
                },
                saveToSentItems = false,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{buzonRemitente}/sendMail")
            {
                Content = new StringContent(JsonSerializer.Serialize(mensaje), System.Text.Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

            var response = await Http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogError("Graph sendMail falló ({Status}): {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando correo a {Destinatario}", destinatario);
            throw;
        }
    });
}
