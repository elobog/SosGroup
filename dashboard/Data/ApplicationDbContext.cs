using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using dashboard.Models;
using dashboard.Models.Admin;
using dashboard.Models.Reclutamiento;
using dashboard.Models.Remuneracion;

namespace dashboard.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // ---- Admin ----
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClienteSucursal> ClienteSucursales => Set<ClienteSucursal>();
    public DbSet<PerfilAplicacion> PerfilesAplicacion => Set<PerfilAplicacion>();
    public DbSet<PerfilModulo> PerfilesModulo => Set<PerfilModulo>();

    // ---- Genérico (dbo) ----
    public DbSet<LogActividad> LogsActividad => Set<LogActividad>();

    // ---- Reclutamiento ----
    public DbSet<PerfilCargo> PerfilesCargo => Set<PerfilCargo>();
    public DbSet<PerfilCargoVersion> PerfilCargoVersiones => Set<PerfilCargoVersion>();
    public DbSet<PerfilCargoVersionAprobacion> PerfilCargoVersionAprobaciones => Set<PerfilCargoVersionAprobacion>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
    public DbSet<SolicitudDetalle> SolicitudDetalles => Set<SolicitudDetalle>();
    public DbSet<ClienteReclutador> ClienteReclutadores => Set<ClienteReclutador>();
    public DbSet<ClienteSupervisor> ClienteSupervisores => Set<ClienteSupervisor>();
    public DbSet<Apertura> Aperturas => Set<Apertura>();
    public DbSet<Aviso> Avisos => Set<Aviso>();
    public DbSet<Publicacion> Publicaciones => Set<Publicacion>();
    public DbSet<PostulacionCandidato> PostulacionesCandidato => Set<PostulacionCandidato>();
    public DbSet<CredencialPlataforma> CredencialesPlataforma => Set<CredencialPlataforma>();
    public DbSet<Postulante> Postulantes => Set<Postulante>();
    public DbSet<PostulanteSolicitud> PostulanteSolicitudes => Set<PostulanteSolicitud>();
    public DbSet<InteraccionIA> InteraccionesIA => Set<InteraccionIA>();
    public DbSet<PreseleccionTarea> PreseleccionTareas => Set<PreseleccionTarea>();
    public DbSet<DocumentoLegal> DocumentosLegales => Set<DocumentoLegal>();
    public DbSet<TestPiamentor> TestsPiamentor => Set<TestPiamentor>();
    public DbSet<CertificadoWHO> CertificadosWHO => Set<CertificadoWHO>();
    public DbSet<EntrevistaPreseleccion> EntrevistasPreseleccion => Set<EntrevistaPreseleccion>();
    public DbSet<PostulanteHistorial> PostulanteHistoriales => Set<PostulanteHistorial>();
    public DbSet<PostulanteDocumento> PostulanteDocumentos => Set<PostulanteDocumento>();
    public DbSet<DocumentoExtraccionIA> DocumentosExtraccionIA => Set<DocumentoExtraccionIA>();

    // ---- Remuneración ----
    public DbSet<ContratoServicio> ContratosServicio => Set<ContratoServicio>();
    public DbSet<EmpleadoTalana> EmpleadosTalana => Set<EmpleadoTalana>();
    public DbSet<Calculo> Calculos => Set<Calculo>();
    public DbSet<CalculoEmpleado> CalculosEmpleado => Set<CalculoEmpleado>();
    public DbSet<CalculoEmpleadoDia> CalculosEmpleadoDia => Set<CalculoEmpleadoDia>();
    public DbSet<AuditoriaEjecucion> AuditoriasEjecucion => Set<AuditoriaEjecucion>();
    public DbSet<AuditoriaInconsistencia> AuditoriasInconsistencia => Set<AuditoriaInconsistencia>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===================== Identity (dbo) — campos propios de ApplicationUser =====================
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Nombre).HasMaxLength(200).IsRequired();
            e.Property(u => u.Estado).HasMaxLength(20).IsRequired().HasDefaultValue("Activo");
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(30).IsRequired();
        });

        // ===================== Admin =====================
        builder.Entity<Cliente>(e =>
        {
            e.ToTable("Cliente", schema: "Admin");
            e.HasKey(x => x.Id);
            e.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
            e.Property(x => x.RUT).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.RUT).IsUnique();
            e.HasIndex(x => x.Estado);
            e.Property(x => x.ContactoNombre).HasMaxLength(200);
            e.Property(x => x.ContactoCorreo).HasMaxLength(200);
            e.Property(x => x.ContactoTelefono).HasMaxLength(30);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired().HasDefaultValue("Activo");
        });

        builder.Entity<ClienteSucursal>(e =>
        {
            e.ToTable("ClienteSucursal", schema: "Admin");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClienteId);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Direccion).HasMaxLength(300).IsRequired();
            e.Property(x => x.ContactoResponsableNombre).HasMaxLength(200);
            e.Property(x => x.ContactoResponsableCargo).HasMaxLength(100);
            e.Property(x => x.ContactoResponsableCorreo).HasMaxLength(200);
            e.Property(x => x.ContactoResponsableTelefono).HasMaxLength(30);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired().HasDefaultValue("Activo");
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PerfilAplicacion>(e =>
        {
            e.ToTable("PerfilAplicacion", schema: "Admin");
            e.HasKey(x => new { x.RoleId, x.App });
            e.Property(x => x.App).HasMaxLength(30).IsRequired();
            e.HasOne<Microsoft.AspNetCore.Identity.IdentityRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PerfilModulo>(e =>
        {
            e.ToTable("PerfilModulo", schema: "Admin");
            e.HasKey(x => new { x.RoleId, x.App, x.Modulo });
            e.Property(x => x.App).HasMaxLength(30).IsRequired();
            e.Property(x => x.Modulo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoleId);
            e.HasOne<Microsoft.AspNetCore.Identity.IdentityRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== Genérico (dbo) =====================
        builder.Entity<LogActividad>(e =>
        {
            e.ToTable("LogActividad", schema: "dbo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Accion).HasMaxLength(200).IsRequired();
            e.Property(x => x.Entidad).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.Entidad, x.EntidadId });
            e.HasIndex(x => x.FechaHora);
        });

        // ===================== Reclutamiento =====================
        builder.Entity<PerfilCargo>(e =>
        {
            e.ToTable("PerfilCargo", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClienteId);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PerfilCargoVersion>(e =>
        {
            e.ToTable("PerfilCargoVersion", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PerfilCargoId);
            e.HasIndex(x => new { x.PerfilCargoId, x.NumeroVersion }).IsUnique();
            e.Property(x => x.RentaFija).HasMaxLength(100);
            e.Property(x => x.RentaVariable).HasMaxLength(200);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<PerfilCargo>().WithMany().HasForeignKey(x => x.PerfilCargoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PerfilCargoVersion>().WithMany().HasForeignKey(x => x.VersionBaseId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PerfilCargoVersionAprobacion>(e =>
        {
            e.ToTable("PerfilCargoVersionAprobacion", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.PerfilCargoVersionId, x.Rol }).IsUnique();
            e.Property(x => x.Rol).HasMaxLength(30).IsRequired();
            e.HasOne<PerfilCargoVersion>().WithMany().HasForeignKey(x => x.PerfilCargoVersionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AprobadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Solicitud>(e =>
        {
            e.ToTable("Solicitud", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.CodigoSolicitud).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.CodigoSolicitud).IsUnique();
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.Estado);
            e.Property(x => x.DireccionServicio).HasMaxLength(300);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PerfilCargoVersion>().WithMany().HasForeignKey(x => x.PerfilCargoVersionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ClienteSucursal>().WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SolicitudDetalle>(e =>
        {
            e.ToTable("SolicitudDetalle", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SolicitudId);
            e.HasIndex(x => x.Estado);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<Solicitud>().WithMany().HasForeignKey(x => x.SolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReclutadorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ClienteReclutador>(e =>
        {
            e.ToTable("ClienteReclutador", schema: "Reclutamiento");
            e.HasKey(x => new { x.ClienteId, x.UsuarioId });
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ClienteSupervisor>(e =>
        {
            e.ToTable("ClienteSupervisor", schema: "Reclutamiento");
            e.HasKey(x => new { x.ClienteId, x.UsuarioId });
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Apertura>(e =>
        {
            e.ToTable("Apertura", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SolicitudId).IsUnique();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<Solicitud>().WithMany().HasForeignKey(x => x.SolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.IniciadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Aviso>(e =>
        {
            e.ToTable("Aviso", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AperturaId);
            e.HasIndex(x => new { x.AperturaId, x.NumeroVersion }).IsUnique();
            e.Property(x => x.TituloCargo).HasMaxLength(150).IsRequired();
            e.Property(x => x.Modalidad).HasMaxLength(20).IsRequired();
            e.Property(x => x.Region).HasMaxLength(100);
            e.Property(x => x.Comuna).HasMaxLength(100);
            e.Property(x => x.NivelCargo).HasMaxLength(50);
            e.Property(x => x.TipoContrato).HasMaxLength(50);
            e.Property(x => x.JornadaTipo).HasMaxLength(20);
            e.Property(x => x.JornadaTexto).HasMaxLength(100);
            e.Property(x => x.TurnoTexto).HasMaxLength(100);
            e.Property(x => x.SalarioPublicado).HasPrecision(12, 2);
            e.Property(x => x.SalarioPeriodicidad).HasMaxLength(20);
            e.Property(x => x.EducacionMinima).HasMaxLength(100);
            e.Property(x => x.EducacionEstado).HasMaxLength(20);
            e.Property(x => x.CursoDeseable).HasMaxLength(200);
            e.Property(x => x.PalabrasClave).HasMaxLength(300);
            e.HasOne<Apertura>().WithMany().HasForeignKey(x => x.AperturaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CreadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Publicacion>(e =>
        {
            e.ToTable("Publicacion", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AvisoId);
            e.Property(x => x.Plataforma).HasMaxLength(30).IsRequired();
            e.Property(x => x.Metodo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(30).IsRequired();
            e.HasOne<Aviso>().WithMany().HasForeignKey(x => x.AvisoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostulacionCandidato>(e =>
        {
            e.ToTable("PostulacionCandidato", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PublicacionId);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.RUT).HasMaxLength(20).IsRequired();
            e.Property(x => x.Correo).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.HasOne<Publicacion>().WithMany().HasForeignKey(x => x.PublicacionId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CredencialPlataforma>(e =>
        {
            e.ToTable("CredencialPlataforma", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Plataforma).HasMaxLength(30).IsRequired();
            e.Property(x => x.NombreCuenta).HasMaxLength(150);
            e.HasIndex(x => new { x.Plataforma, x.UsuarioId }).IsUnique().HasFilter("[UsuarioId] IS NOT NULL");
            e.HasIndex(x => x.Plataforma).IsUnique().HasFilter("[UsuarioId] IS NULL");
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Postulante>(e =>
        {
            e.ToTable("Postulante", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.RUT).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.RUT).IsUnique();
            e.Property(x => x.Correo).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Sexo).HasMaxLength(20);
            e.Property(x => x.Comuna).HasMaxLength(100);
            e.Property(x => x.RubroExperiencia).HasMaxLength(150);
            e.Property(x => x.EstadoCivil).HasMaxLength(30);
            e.Property(x => x.Nacionalidad).HasMaxLength(50);
            e.Property(x => x.RentaPretendida).HasPrecision(12, 2);
            e.Property(x => x.TituloCV).HasMaxLength(200);
        });

        builder.Entity<PostulanteSolicitud>(e =>
        {
            e.ToTable("PostulanteSolicitud", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SolicitudId);
            e.HasIndex(x => x.PublicacionId);
            e.HasIndex(x => new { x.PostulanteId, x.SolicitudId }).IsUnique();
            e.Property(x => x.Origen).HasMaxLength(20).IsRequired();
            e.Property(x => x.EtapaPreseleccion).HasMaxLength(20);
            e.Property(x => x.CanalOrigen).HasMaxLength(50);
            e.Property(x => x.SituacionActual).HasMaxLength(100);
            e.Property(x => x.EtapaEvaluacion).HasMaxLength(20);
            e.Property(x => x.ComentarioEvaluacion).HasMaxLength(500);
            e.HasOne<Postulante>().WithMany().HasForeignKey(x => x.PostulanteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Solicitud>().WithMany().HasForeignKey(x => x.SolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Publicacion>().WithMany().HasForeignKey(x => x.PublicacionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AprobadoPorSupervisorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InteraccionIA>(e =>
        {
            e.ToTable("InteraccionIA", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SolicitudId);
            e.HasIndex(x => x.ClienteId);
            e.Property(x => x.Proveedor).HasMaxLength(50).IsRequired();
            e.Property(x => x.Proposito).HasMaxLength(100).IsRequired();
            e.Property(x => x.Modelo).HasMaxLength(50).IsRequired();
            e.Property(x => x.CostoUsd).HasPrecision(10, 4);
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Solicitud>().WithMany().HasForeignKey(x => x.SolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Postulante>().WithMany().HasForeignKey(x => x.PostulanteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReclutadorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PreseleccionTarea>(e =>
        {
            e.ToTable("PreseleccionTarea", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PostulanteSolicitudId);
            e.HasIndex(x => new { x.PostulanteSolicitudId, x.Tipo }).IsUnique();
            e.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.Property(x => x.Detalle).HasMaxLength(300);
            e.HasOne<PostulanteSolicitud>().WithMany().HasForeignKey(x => x.PostulanteSolicitudId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DocumentoLegal>(e =>
        {
            e.ToTable("DocumentoLegal", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PostulanteSolicitudId);
            e.Property(x => x.NombreArchivo).HasMaxLength(200).IsRequired();
            e.Property(x => x.TipoArchivo).HasMaxLength(10).IsRequired();
            e.Property(x => x.Origen).HasMaxLength(20).IsRequired();
            e.Property(x => x.RutaArchivo).HasMaxLength(500).IsRequired();
            e.HasOne<PostulanteSolicitud>().WithMany().HasForeignKey(x => x.PostulanteSolicitudId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TestPiamentor>(e =>
        {
            e.ToTable("TestPiamentor", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PostulanteSolicitudId);
            e.Property(x => x.Proveedor).HasMaxLength(50).IsRequired();
            e.Property(x => x.CostoUsd).HasPrecision(10, 4);
            e.HasOne<PostulanteSolicitud>().WithMany().HasForeignKey(x => x.PostulanteSolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReclutadorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CertificadoWHO>(e =>
        {
            e.ToTable("CertificadoWHO", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PostulanteSolicitudId);
            e.Property(x => x.Proveedor).HasMaxLength(50).IsRequired();
            e.Property(x => x.CostoUsd).HasPrecision(10, 4);
            e.Property(x => x.Resultado).HasMaxLength(200).IsRequired();
            e.HasOne<PostulanteSolicitud>().WithMany().HasForeignKey(x => x.PostulanteSolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReclutadorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EntrevistaPreseleccion>(e =>
        {
            e.ToTable("EntrevistaPreseleccion", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PostulanteSolicitudId).IsUnique();
            e.HasOne<PostulanteSolicitud>().WithMany().HasForeignKey(x => x.PostulanteSolicitudId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReclutadorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostulanteHistorial>(e =>
        {
            e.ToTable("PostulanteHistorial", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            e.Property(x => x.Origen).HasMaxLength(20).IsRequired();
            e.HasOne<Postulante>().WithMany().HasForeignKey(x => x.PostulanteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PostulanteDocumento>(e =>
        {
            e.ToTable("PostulanteDocumento", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            e.Property(x => x.RutaArchivo).HasMaxLength(500).IsRequired();
            e.Property(x => x.EstadoProcesamiento).HasMaxLength(20).IsRequired();
            e.HasOne<Postulante>().WithMany().HasForeignKey(x => x.PostulanteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DocumentoExtraccionIA>(e =>
        {
            e.ToTable("DocumentoExtraccionIA", schema: "Reclutamiento");
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<PostulanteDocumento>().WithMany().HasForeignKey(x => x.PostulanteDocumentoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== Remuneración =====================
        builder.Entity<ContratoServicio>(e =>
        {
            e.ToTable("ContratoServicio", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.Estado);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EmpleadoTalana>(e =>
        {
            e.ToTable("EmpleadoTalana", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RUT).IsUnique();
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.ContratoServicioId);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.RUT).HasMaxLength(20).IsRequired();
            e.Property(x => x.Cargo).HasMaxLength(150);
            e.Property(x => x.EstadoTalana).HasMaxLength(20).IsRequired();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ContratoServicio>().WithMany().HasForeignKey(x => x.ContratoServicioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Calculo>(e =>
        {
            e.ToTable("Calculo", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ContratoServicioId, x.Periodo }).IsUnique();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<ContratoServicio>().WithMany().HasForeignKey(x => x.ContratoServicioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CalculoEmpleado>(e =>
        {
            e.ToTable("CalculoEmpleado", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CalculoId, x.EmpleadoTalanaId }).IsUnique();
            e.HasOne<Calculo>().WithMany().HasForeignKey(x => x.CalculoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<EmpleadoTalana>().WithMany().HasForeignKey(x => x.EmpleadoTalanaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CalculoEmpleadoDia>(e =>
        {
            e.ToTable("CalculoEmpleadoDia", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.CalculoEmpleadoId, x.Fecha }).IsUnique();
            e.Property(x => x.TipoJornada).HasMaxLength(20).IsRequired();
            e.Property(x => x.HorasExtra).HasPrecision(5, 2);
            e.HasOne<CalculoEmpleado>().WithMany().HasForeignKey(x => x.CalculoEmpleadoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditoriaEjecucion>(e =>
        {
            e.ToTable("AuditoriaEjecucion", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
        });

        builder.Entity<AuditoriaInconsistencia>(e =>
        {
            e.ToTable("AuditoriaInconsistencia", schema: "Remuneracion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AuditoriaEjecucionId);
            e.HasIndex(x => x.CalculoEmpleadoId);
            e.Property(x => x.TipoInconsistencia).HasMaxLength(100).IsRequired();
            e.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasOne<AuditoriaEjecucion>().WithMany().HasForeignKey(x => x.AuditoriaEjecucionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<CalculoEmpleado>().WithMany().HasForeignKey(x => x.CalculoEmpleadoId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
