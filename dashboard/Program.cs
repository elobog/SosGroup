using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dashboard.Components;
using dashboard.Components.Account;
using dashboard.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// Blazor Server comparte el DbContext scoped por circuito (sesión), no por request: componentes
// interactivos que quedan montados todo el tiempo (topbar/sidebar) pueden correr consultas al
// mismo tiempo que una página recién cargada y EF Core no soporta eso. AccesoAppService pide un
// DbContext nuevo por consulta vía este factory en vez del scoped de abajo, para no competir con
// Identity (UserManager/SignInManager, que sí necesitan el scoped de siempre).
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<CorreoSistemaService>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>>(sp => sp.GetRequiredService<CorreoSistemaService>());
builder.Services.AddScoped<AccesoAppService>();
builder.Services.AddScoped<ClientesService>();
builder.Services.AddScoped<CostosIAService>();
builder.Services.AddScoped<PerfilCargoService>();
builder.Services.AddScoped<BlobStorageService>();
builder.Services.AddScoped<PerfilCargoIAService>();
builder.Services.AddScoped<PostulanteIAService>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<ContratosService>();
builder.Services.AddScoped<PostulacionPublicaService>();
builder.Services.AddScoped<AppSeleccionState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Descarga de documentos de un Postulante (CV/Certificados) desde la ficha de Solicitud. Los mismos
// roles que ven Reclutamiento pueden descargar — el acceso por cliente asignado no se acota acá,
// igual que SolicitudService.ObtenerAsync (ficha por Id, sin filtro de cliente).
app.MapGet("/reclutamiento/postulantes/documentos/{documentoId:int}", async (int documentoId, IDbContextFactory<ApplicationDbContext> dbFactory, BlobStorageService blobStorage) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var documento = await db.PostulanteDocumentos.FindAsync(documentoId);
    if (documento is null) return Results.NotFound();

    var contenido = await blobStorage.DescargarDocumentoPostulanteAsync(documento.RutaArchivo);
    // RutaArchivo = "{postulanteId}/{guid}-{nombreOriginal}" (ver BlobStorageService.SubirDocumentoPostulanteAsync).
    var nombreConGuid = documento.RutaArchivo[(documento.RutaArchivo.LastIndexOf('/') + 1)..];
    var nombreArchivo = nombreConGuid.Length > 37 ? nombreConGuid[37..] : nombreConGuid;
    return Results.File(contenido, "application/pdf", nombreArchivo);
})
.RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Admin", "Supervisor Operaciones", "Supervisor Administrativo", "Reclutador"));

app.Run();
