-- ============================================================================
-- SOS Group — Portal interno — Estructura inicial de base de datos
-- Servidor: sql-sosgroup-app.database.windows.net
-- Base de datos: sqldb-sosgroup-app
-- Fuente de verdad de campos/tipos/índices: dashboard/BD_Dashboard.md
--
-- Convivencia de las 3 aplicaciones (Admin, Reclutamiento, Remuneración) en
-- una sola base de datos, separadas por SQL SCHEMA (no por base de datos):
--   dbo           -> Identity (AspNetUsers/Roles/etc., estándar) + LogActividad
--                    (genérica, no pertenece a una sola app)
--   Admin         -> Cliente, ClienteSucursal, PerfilAplicacion, PerfilModulo
--   Reclutamiento -> todas las tablas de ese módulo (Solicitud, Apertura,
--                    Atracción, Preselección)
--   Remuneracion  -> reservado, sin tablas todavía (se agregan cuando se
--                    detalle esa app con el mismo método)
--
-- El control de acceso real (qué app/módulo ve cada perfil) vive en la capa
-- de aplicación vía PerfilAplicacion/PerfilModulo — los schemas de acá son
-- organización física, no un mecanismo de seguridad a nivel de SQL.
--
-- Orden de ejecución: de arriba hacia abajo (respeta dependencias de FK).
-- Ejecutar completo en una sola pasada contra sqldb-sosgroup-app.
-- ============================================================================

SET NOCOUNT ON;
GO

-- ============================================================================
-- 0. SCHEMAS
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Admin')
    EXEC('CREATE SCHEMA Admin');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Reclutamiento')
    EXEC('CREATE SCHEMA Reclutamiento');
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Remuneracion')
    EXEC('CREATE SCHEMA Remuneracion'); -- reservado, sin tablas todavía
GO

-- ============================================================================
-- 1. IDENTITY (dbo) — esquema estándar de ASP.NET Core Identity
--    AspNetUsers extendida con Nombre/Estado/UltimaVerificacion2FA y con
--    Email/PhoneNumber marcados NOT NULL (requeridos en este sistema, ver
--    BD_Dashboard.md > Admin > ApplicationUser).
-- ============================================================================

CREATE TABLE dbo.AspNetRoles (
    Id               nvarchar(450)   NOT NULL,
    Name             nvarchar(256)   NULL,
    NormalizedName   nvarchar(256)   NULL,
    ConcurrencyStamp nvarchar(max)   NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_AspNetRoles_NormalizedName ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
GO

CREATE TABLE dbo.AspNetUsers (
    Id                   nvarchar(450)     NOT NULL,
    UserName             nvarchar(256)     NULL,
    NormalizedUserName   nvarchar(256)     NULL,
    Email                nvarchar(256)     NOT NULL, -- requerido en este sistema (recuperación de password + 2FA)
    NormalizedEmail      nvarchar(256)     NOT NULL,
    EmailConfirmed       bit               NOT NULL DEFAULT 0,
    PasswordHash         nvarchar(max)     NULL,
    SecurityStamp        nvarchar(max)     NULL,
    ConcurrencyStamp     nvarchar(max)     NULL,
    PhoneNumber          nvarchar(30)      NOT NULL, -- requerido en este sistema
    PhoneNumberConfirmed bit               NOT NULL DEFAULT 0,
    TwoFactorEnabled     bit               NOT NULL DEFAULT 0,
    LockoutEnd           datetimeoffset    NULL,
    LockoutEnabled       bit               NOT NULL DEFAULT 1,
    AccessFailedCount    int               NOT NULL DEFAULT 0,
    -- Campos propios (ver BD_Dashboard.md > Admin > ApplicationUser)
    Nombre               nvarchar(200)     NOT NULL,
    Estado               nvarchar(20)      NOT NULL DEFAULT 'Activo', -- Activo / Bloqueado
    UltimaVerificacion2FA datetime2        NULL,
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_AspNetUsers_NormalizedEmail ON dbo.AspNetUsers(NormalizedEmail);
CREATE UNIQUE INDEX IX_AspNetUsers_NormalizedUserName ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
GO

CREATE TABLE dbo.AspNetUserRoles (
    UserId nvarchar(450) NOT NULL,
    RoleId nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserClaims (
    Id         int IDENTITY(1,1) NOT NULL,
    UserId     nvarchar(450)     NOT NULL,
    ClaimType  nvarchar(max)     NULL,
    ClaimValue nvarchar(max)     NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetRoleClaims (
    Id         int IDENTITY(1,1) NOT NULL,
    RoleId     nvarchar(450)     NOT NULL,
    ClaimType  nvarchar(max)     NULL,
    ClaimValue nvarchar(max)     NULL,
    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserLogins (
    LoginProvider       nvarchar(450) NOT NULL,
    ProviderKey          nvarchar(450) NOT NULL,
    ProviderDisplayName  nvarchar(max) NULL,
    UserId               nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetUserTokens (
    UserId        nvarchar(450) NOT NULL,
    LoginProvider nvarchar(450) NOT NULL,
    Name          nvarchar(450) NOT NULL,
    Value         nvarchar(max) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 2. ADMIN — Perfiles dinámicos (referencian AspNetRoles, no duplican el
--    concepto de rol/perfil) y datos maestros de Cliente/Sucursal
-- ============================================================================

CREATE TABLE Admin.PerfilAplicacion (
    RoleId nvarchar(450) NOT NULL,
    App    nvarchar(30)  NOT NULL, -- "Reclutamiento" / "Remuneracion" / "Admin"
    CONSTRAINT PK_PerfilAplicacion PRIMARY KEY (RoleId, App),
    CONSTRAINT FK_PerfilAplicacion_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO

CREATE TABLE Admin.PerfilModulo (
    RoleId nvarchar(450) NOT NULL,
    App    nvarchar(30)  NOT NULL,
    Modulo nvarchar(50)  NOT NULL,
    CONSTRAINT PK_PerfilModulo PRIMARY KEY (RoleId, App, Modulo),
    CONSTRAINT FK_PerfilModulo_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_PerfilModulo_RoleId ON Admin.PerfilModulo(RoleId);
GO

CREATE TABLE Admin.Cliente (
    Id              int IDENTITY(1,1) NOT NULL,
    RazonSocial     nvarchar(200)     NOT NULL,
    RUT             nvarchar(20)      NOT NULL,
    ContactoNombre  nvarchar(200)     NULL,
    ContactoCorreo  nvarchar(200)     NULL,
    ContactoTelefono nvarchar(30)     NULL,
    Estado          nvarchar(20)      NOT NULL DEFAULT 'Activo', -- Activo / Inactivo (bloqueado)
    FechaCreacion   datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Cliente PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Cliente_RUT ON Admin.Cliente(RUT);
CREATE INDEX IX_Cliente_Estado ON Admin.Cliente(Estado);
GO

CREATE TABLE Admin.ClienteSucursal (
    Id                            int IDENTITY(1,1) NOT NULL,
    ClienteId                     int               NOT NULL,
    Nombre                        nvarchar(150)     NOT NULL,
    Direccion                     nvarchar(300)     NOT NULL,
    ContactoResponsableNombre     nvarchar(200)     NULL,
    ContactoResponsableCargo      nvarchar(100)     NULL,
    ContactoResponsableCorreo     nvarchar(200)     NULL,
    ContactoResponsableTelefono   nvarchar(30)      NULL,
    Estado                        nvarchar(20)      NOT NULL DEFAULT 'Activo',
    FechaCreacion                 datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ClienteSucursal PRIMARY KEY (Id),
    CONSTRAINT FK_ClienteSucursal_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id)
);
GO
CREATE INDEX IX_ClienteSucursal_ClienteId ON Admin.ClienteSucursal(ClienteId);
GO

-- ============================================================================
-- 3. RECLUTAMIENTO — Módulo 1 · Solicitud
-- ============================================================================

CREATE TABLE Reclutamiento.PerfilCargo (
    Id            int IDENTITY(1,1) NOT NULL,
    ClienteId     int               NOT NULL,
    Nombre        nvarchar(150)     NOT NULL,
    Descripcion   nvarchar(max)     NULL,
    Requisitos    nvarchar(max)     NULL,
    Estado        nvarchar(20)      NOT NULL DEFAULT 'Activo',
    FechaCreacion datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PerfilCargo PRIMARY KEY (Id),
    CONSTRAINT FK_PerfilCargo_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id)
);
GO
CREATE INDEX IX_PerfilCargo_ClienteId ON Reclutamiento.PerfilCargo(ClienteId);
GO

CREATE TABLE Reclutamiento.PerfilCargoVersion (
    Id             int IDENTITY(1,1) NOT NULL,
    PerfilCargoId  int               NOT NULL,
    NumeroVersion  int               NOT NULL,
    RentaFija      nvarchar(100)     NULL,
    RentaVariable  nvarchar(200)     NULL,
    Beneficios     nvarchar(max)     NULL,
    FechaVigencia  datetime2         NOT NULL,
    Estado         nvarchar(20)      NOT NULL, -- Pendiente / Vigente / Historica
    VersionBaseId  int               NULL,
    CONSTRAINT PK_PerfilCargoVersion PRIMARY KEY (Id),
    CONSTRAINT FK_PerfilCargoVersion_PerfilCargo FOREIGN KEY (PerfilCargoId) REFERENCES Reclutamiento.PerfilCargo(Id),
    CONSTRAINT FK_PerfilCargoVersion_VersionBase FOREIGN KEY (VersionBaseId) REFERENCES Reclutamiento.PerfilCargoVersion(Id)
);
GO
CREATE INDEX IX_PerfilCargoVersion_PerfilCargoId ON Reclutamiento.PerfilCargoVersion(PerfilCargoId);
CREATE UNIQUE INDEX IX_PerfilCargoVersion_PerfilCargoId_NumeroVersion ON Reclutamiento.PerfilCargoVersion(PerfilCargoId, NumeroVersion);
GO

CREATE TABLE Reclutamiento.PerfilCargoVersionAprobacion (
    Id                    int IDENTITY(1,1) NOT NULL,
    PerfilCargoVersionId  int               NOT NULL,
    Rol                   nvarchar(30)      NOT NULL, -- 'Supervisor Operaciones' / 'Supervisor Administrativo'
    AprobadoPorUsuarioId  nvarchar(450)     NULL,
    FechaAprobacion       datetime2         NULL,
    CONSTRAINT PK_PerfilCargoVersionAprobacion PRIMARY KEY (Id),
    CONSTRAINT FK_PCVAprobacion_PerfilCargoVersion FOREIGN KEY (PerfilCargoVersionId) REFERENCES Reclutamiento.PerfilCargoVersion(Id),
    CONSTRAINT FK_PCVAprobacion_Usuario FOREIGN KEY (AprobadoPorUsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE UNIQUE INDEX IX_PCVAprobacion_Version_Rol ON Reclutamiento.PerfilCargoVersionAprobacion(PerfilCargoVersionId, Rol);
GO

CREATE TABLE Reclutamiento.Solicitud (
    Id                   int IDENTITY(1,1) NOT NULL,
    CodigoSolicitud      nvarchar(30)      NOT NULL,
    ClienteId            int               NOT NULL,
    PerfilCargoVersionId int               NOT NULL,
    FechaInicioServicio  datetime2         NOT NULL,
    SucursalId           int               NULL,
    DireccionServicio    nvarchar(300)     NULL, -- override cuando no hay SucursalId
    SupervisorId         nvarchar(450)     NOT NULL, -- usuario rol Supervisor Operaciones
    Estado               nvarchar(20)      NOT NULL, -- Activa / Cerrada — derivado
    FechaCreacion        datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Solicitud PRIMARY KEY (Id),
    CONSTRAINT FK_Solicitud_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id),
    CONSTRAINT FK_Solicitud_PerfilCargoVersion FOREIGN KEY (PerfilCargoVersionId) REFERENCES Reclutamiento.PerfilCargoVersion(Id),
    CONSTRAINT FK_Solicitud_Sucursal FOREIGN KEY (SucursalId) REFERENCES Admin.ClienteSucursal(Id),
    CONSTRAINT FK_Solicitud_Supervisor FOREIGN KEY (SupervisorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_Solicitud_ClienteId ON Reclutamiento.Solicitud(ClienteId);
CREATE UNIQUE INDEX IX_Solicitud_CodigoSolicitud ON Reclutamiento.Solicitud(CodigoSolicitud);
CREATE INDEX IX_Solicitud_Estado ON Reclutamiento.Solicitud(Estado);
GO

CREATE TABLE Reclutamiento.SolicitudDetalle (
    Id                          int IDENTITY(1,1) NOT NULL,
    SolicitudId                 int               NOT NULL,
    ReclutadorId                nvarchar(450)     NOT NULL,
    CantidadPersonalSolicitado  int               NOT NULL,
    FechaSolicitud              datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    Estado                      nvarchar(20)      NOT NULL, -- Pendiente / EnProceso / Cerrada
    Observaciones               nvarchar(max)     NULL,
    CONSTRAINT PK_SolicitudDetalle PRIMARY KEY (Id),
    CONSTRAINT FK_SolicitudDetalle_Solicitud FOREIGN KEY (SolicitudId) REFERENCES Reclutamiento.Solicitud(Id),
    CONSTRAINT FK_SolicitudDetalle_Reclutador FOREIGN KEY (ReclutadorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_SolicitudDetalle_SolicitudId ON Reclutamiento.SolicitudDetalle(SolicitudId);
CREATE INDEX IX_SolicitudDetalle_Estado ON Reclutamiento.SolicitudDetalle(Estado);
GO

CREATE TABLE Reclutamiento.ClienteReclutador (
    ClienteId       int           NOT NULL,
    UsuarioId       nvarchar(450) NOT NULL,
    FechaAsignacion datetime2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ClienteReclutador PRIMARY KEY (ClienteId, UsuarioId),
    CONSTRAINT FK_ClienteReclutador_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id),
    CONSTRAINT FK_ClienteReclutador_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO

CREATE TABLE Reclutamiento.ClienteSupervisor (
    ClienteId       int           NOT NULL,
    UsuarioId       nvarchar(450) NOT NULL,
    FechaAsignacion datetime2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ClienteSupervisor PRIMARY KEY (ClienteId, UsuarioId),
    CONSTRAINT FK_ClienteSupervisor_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id),
    CONSTRAINT FK_ClienteSupervisor_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO

-- ============================================================================
-- 4. RECLUTAMIENTO — Módulo 1 · Apertura
-- ============================================================================

CREATE TABLE Reclutamiento.Apertura (
    Id                   int IDENTITY(1,1) NOT NULL,
    SolicitudId          int               NOT NULL,
    FechaInicio          datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    IniciadoPorUsuarioId nvarchar(450)     NOT NULL,
    Estado               nvarchar(20)      NOT NULL, -- Iniciada
    CONSTRAINT PK_Apertura PRIMARY KEY (Id),
    CONSTRAINT FK_Apertura_Solicitud FOREIGN KEY (SolicitudId) REFERENCES Reclutamiento.Solicitud(Id),
    CONSTRAINT FK_Apertura_Usuario FOREIGN KEY (IniciadoPorUsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE UNIQUE INDEX IX_Apertura_SolicitudId ON Reclutamiento.Apertura(SolicitudId);
GO

CREATE TABLE Reclutamiento.Aviso (
    Id                  int IDENTITY(1,1) NOT NULL,
    AperturaId          int               NOT NULL,
    NumeroVersion       int               NOT NULL,
    TituloCargo         nvarchar(150)     NOT NULL,
    Modalidad           nvarchar(20)      NOT NULL, -- Presencial / Híbrido / Remoto
    Ubicacion           nvarchar(300)     NULL,
    CierrePostulaciones datetime2         NULL,
    CuerpoHtml          nvarchar(max)     NOT NULL,
    CreadoPorUsuarioId  nvarchar(450)     NOT NULL,
    FechaCreacion       datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Aviso PRIMARY KEY (Id),
    CONSTRAINT FK_Aviso_Apertura FOREIGN KEY (AperturaId) REFERENCES Reclutamiento.Apertura(Id),
    CONSTRAINT FK_Aviso_Usuario FOREIGN KEY (CreadoPorUsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_Aviso_AperturaId ON Reclutamiento.Aviso(AperturaId);
CREATE UNIQUE INDEX IX_Aviso_AperturaId_NumeroVersion ON Reclutamiento.Aviso(AperturaId, NumeroVersion);
GO

CREATE TABLE Reclutamiento.Publicacion (
    Id                  int IDENTITY(1,1) NOT NULL,
    AvisoId             int               NOT NULL,
    Plataforma          nvarchar(30)      NOT NULL, -- Computrabajo / Trabajando.com / LinkedIn / Facebook / Instagram / TikTok
    UsuarioId           nvarchar(450)     NOT NULL,
    FechaHora           datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    Metodo              nvarchar(20)      NOT NULL, -- API / Manual
    Estado              nvarchar(30)      NOT NULL, -- Publicado / Pendiente de carga manual / Error
    CantidadPostulantes int               NOT NULL DEFAULT 0,
    CONSTRAINT PK_Publicacion PRIMARY KEY (Id),
    CONSTRAINT FK_Publicacion_Aviso FOREIGN KEY (AvisoId) REFERENCES Reclutamiento.Aviso(Id),
    CONSTRAINT FK_Publicacion_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_Publicacion_AvisoId ON Reclutamiento.Publicacion(AvisoId);
GO

CREATE TABLE Reclutamiento.PostulacionCandidato (
    Id            int IDENTITY(1,1) NOT NULL,
    PublicacionId int               NOT NULL,
    Nombre        nvarchar(200)     NOT NULL,
    RUT           nvarchar(20)      NOT NULL,
    Correo        nvarchar(200)     NULL,
    Telefono      nvarchar(30)      NULL,
    FechaHora     datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PostulacionCandidato PRIMARY KEY (Id),
    CONSTRAINT FK_PostulacionCandidato_Publicacion FOREIGN KEY (PublicacionId) REFERENCES Reclutamiento.Publicacion(Id)
);
GO
CREATE INDEX IX_PostulacionCandidato_PublicacionId ON Reclutamiento.PostulacionCandidato(PublicacionId);
GO

CREATE TABLE Reclutamiento.CredencialPlataforma (
    Id            int IDENTITY(1,1) NOT NULL,
    UsuarioId     nvarchar(450)     NULL, -- null = credencial de empresa
    Plataforma    nvarchar(30)      NOT NULL,
    Conectado     bit               NOT NULL DEFAULT 0,
    NombreCuenta  nvarchar(150)     NULL,
    CONSTRAINT PK_CredencialPlataforma PRIMARY KEY (Id),
    CONSTRAINT FK_CredencialPlataforma_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
-- Único (UsuarioId, Plataforma) tratando NULL como un solo valor de "empresa" por plataforma:
CREATE UNIQUE INDEX IX_CredencialPlataforma_Usuario_Plataforma
    ON Reclutamiento.CredencialPlataforma(Plataforma, UsuarioId)
    WHERE UsuarioId IS NOT NULL;
CREATE UNIQUE INDEX IX_CredencialPlataforma_Empresa_Plataforma
    ON Reclutamiento.CredencialPlataforma(Plataforma)
    WHERE UsuarioId IS NULL;
GO

-- ============================================================================
-- 5. RECLUTAMIENTO — Módulo 2 · Preselección · Atracción
-- ============================================================================

CREATE TABLE Reclutamiento.Postulante (
    Id                     int IDENTITY(1,1) NOT NULL,
    Nombre                 nvarchar(200)     NOT NULL,
    RUT                    nvarchar(20)      NOT NULL,
    Correo                 nvarchar(200)     NULL,
    Telefono               nvarchar(30)      NULL,
    Sexo                   nvarchar(20)      NULL,
    Edad                   int               NULL,
    Comuna                 nvarchar(100)     NULL,
    AniosExperiencia       int               NULL,
    RubroExperiencia       nvarchar(150)     NULL,
    EstadoCivil            nvarchar(30)      NULL,
    Nacionalidad           nvarchar(50)      NULL,
    Discapacidad           bit               NULL,
    RentaPretendida        decimal(12,2)     NULL,
    TituloCV               nvarchar(200)     NULL,
    DescripcionProfesional nvarchar(max)     NULL,
    Habilidades            nvarchar(max)     NULL,
    FechaCreacion          datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Postulante PRIMARY KEY (Id)
);
GO
CREATE UNIQUE INDEX IX_Postulante_RUT ON Reclutamiento.Postulante(RUT);
GO

CREATE TABLE Reclutamiento.PostulanteSolicitud (
    Id                         int IDENTITY(1,1) NOT NULL,
    PostulanteId               int               NOT NULL,
    SolicitudId                int               NOT NULL,
    Origen                     nvarchar(20)      NOT NULL, -- Directa / CargaMasiva
    FechaIngreso               datetime2         NOT NULL,
    Puntaje                    int               NULL,
    SeleccionadoPreseleccion   bit               NOT NULL DEFAULT 0,
    EtapaPreseleccion          nvarchar(20)      NULL, -- Preseleccionado / Seleccionado
    PuntajeInicial             int               NULL,
    PuntajeFinal               int               NULL,
    PublicacionId              int               NULL,
    CanalOrigen                nvarchar(50)      NULL,
    SituacionActual            nvarchar(100)     NULL,
    UltimaActualizacionCV      datetime2         NULL,
    UltimoLoginPortal          datetime2         NULL,
    AdecuacionPortal           int               NULL,
    RespuestasPreguntasPortal  nvarchar(max)     NULL,
    EtapaEvaluacion            nvarchar(20)      NULL, -- Validado / Rechazado
    AprobadoPorSupervisorId    nvarchar(450)     NULL,
    FechaEvaluacion            datetime2         NULL,
    ComentarioEvaluacion       nvarchar(500)     NULL,
    CONSTRAINT PK_PostulanteSolicitud PRIMARY KEY (Id),
    CONSTRAINT FK_PostulanteSolicitud_Postulante FOREIGN KEY (PostulanteId) REFERENCES Reclutamiento.Postulante(Id),
    CONSTRAINT FK_PostulanteSolicitud_Solicitud FOREIGN KEY (SolicitudId) REFERENCES Reclutamiento.Solicitud(Id),
    CONSTRAINT FK_PostulanteSolicitud_Publicacion FOREIGN KEY (PublicacionId) REFERENCES Reclutamiento.Publicacion(Id),
    CONSTRAINT FK_PostulanteSolicitud_Supervisor FOREIGN KEY (AprobadoPorSupervisorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_PostulanteSolicitud_SolicitudId ON Reclutamiento.PostulanteSolicitud(SolicitudId);
CREATE INDEX IX_PostulanteSolicitud_PublicacionId ON Reclutamiento.PostulanteSolicitud(PublicacionId);
CREATE UNIQUE INDEX IX_PostulanteSolicitud_Postulante_Solicitud ON Reclutamiento.PostulanteSolicitud(PostulanteId, SolicitudId);
GO

CREATE TABLE Reclutamiento.InteraccionIA (
    Id            int IDENTITY(1,1) NOT NULL,
    ClienteId     int               NOT NULL,
    SolicitudId   int               NOT NULL,
    PostulanteId  int               NOT NULL,
    ReclutadorId  nvarchar(450)     NOT NULL,
    Proveedor     nvarchar(50)      NOT NULL,
    Proposito     nvarchar(100)     NOT NULL,
    Modelo        nvarchar(50)      NOT NULL,
    TokensEntrada int               NOT NULL,
    TokensSalida  int               NOT NULL,
    CostoUsd      decimal(10,4)     NOT NULL,
    FechaHora     datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_InteraccionIA PRIMARY KEY (Id),
    CONSTRAINT FK_InteraccionIA_Cliente FOREIGN KEY (ClienteId) REFERENCES Admin.Cliente(Id),
    CONSTRAINT FK_InteraccionIA_Solicitud FOREIGN KEY (SolicitudId) REFERENCES Reclutamiento.Solicitud(Id),
    CONSTRAINT FK_InteraccionIA_Postulante FOREIGN KEY (PostulanteId) REFERENCES Reclutamiento.Postulante(Id),
    CONSTRAINT FK_InteraccionIA_Reclutador FOREIGN KEY (ReclutadorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_InteraccionIA_SolicitudId ON Reclutamiento.InteraccionIA(SolicitudId);
CREATE INDEX IX_InteraccionIA_ClienteId ON Reclutamiento.InteraccionIA(ClienteId);
GO

-- ============================================================================
-- 6. RECLUTAMIENTO — Módulo 2 · Preselección · Preselección
-- ============================================================================

CREATE TABLE Reclutamiento.PreseleccionTarea (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteSolicitudId  int               NOT NULL,
    Tipo                   nvarchar(30)      NOT NULL, -- Inicio/EnvioCorreo/UploadDocumentos/TestPiamentor/ValidacionIA1/Entrevista/CertificadoWHO/ValidacionIA2/Validacion
    Estado                 nvarchar(20)      NOT NULL, -- Pendiente / Completado
    Fecha                  datetime2         NULL,
    Detalle                nvarchar(300)     NULL,
    CONSTRAINT PK_PreseleccionTarea PRIMARY KEY (Id),
    CONSTRAINT FK_PreseleccionTarea_PS FOREIGN KEY (PostulanteSolicitudId) REFERENCES Reclutamiento.PostulanteSolicitud(Id)
);
GO
CREATE INDEX IX_PreseleccionTarea_PSId ON Reclutamiento.PreseleccionTarea(PostulanteSolicitudId);
CREATE UNIQUE INDEX IX_PreseleccionTarea_PS_Tipo ON Reclutamiento.PreseleccionTarea(PostulanteSolicitudId, Tipo);
GO

CREATE TABLE Reclutamiento.DocumentoLegal (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteSolicitudId  int               NOT NULL,
    NombreArchivo          nvarchar(200)     NOT NULL,
    TipoArchivo            nvarchar(10)      NOT NULL, -- PDF / Imagen
    Origen                 nvarchar(20)      NOT NULL, -- Reclutador / Postulante
    RutaArchivo            nvarchar(500)     NOT NULL,
    FechaCarga             datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_DocumentoLegal PRIMARY KEY (Id),
    CONSTRAINT FK_DocumentoLegal_PS FOREIGN KEY (PostulanteSolicitudId) REFERENCES Reclutamiento.PostulanteSolicitud(Id)
);
GO
CREATE INDEX IX_DocumentoLegal_PSId ON Reclutamiento.DocumentoLegal(PostulanteSolicitudId);
GO

CREATE TABLE Reclutamiento.TestPiamentor (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteSolicitudId  int               NOT NULL,
    ReclutadorId           nvarchar(450)     NOT NULL,
    Proveedor              nvarchar(50)      NOT NULL, -- "Piamentor.cl"
    CostoUsd               decimal(10,4)     NOT NULL,
    Puntaje                int               NOT NULL,
    Fecha                  datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TestPiamentor PRIMARY KEY (Id),
    CONSTRAINT FK_TestPiamentor_PS FOREIGN KEY (PostulanteSolicitudId) REFERENCES Reclutamiento.PostulanteSolicitud(Id),
    CONSTRAINT FK_TestPiamentor_Reclutador FOREIGN KEY (ReclutadorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_TestPiamentor_PSId ON Reclutamiento.TestPiamentor(PostulanteSolicitudId);
GO

CREATE TABLE Reclutamiento.CertificadoWHO (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteSolicitudId  int               NOT NULL,
    ReclutadorId           nvarchar(450)     NOT NULL,
    Proveedor              nvarchar(50)      NOT NULL, -- "WHO.cl"
    CostoUsd               decimal(10,4)     NOT NULL,
    Resultado              nvarchar(200)     NOT NULL,
    Fecha                  datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CertificadoWHO PRIMARY KEY (Id),
    CONSTRAINT FK_CertificadoWHO_PS FOREIGN KEY (PostulanteSolicitudId) REFERENCES Reclutamiento.PostulanteSolicitud(Id),
    CONSTRAINT FK_CertificadoWHO_Reclutador FOREIGN KEY (ReclutadorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_CertificadoWHO_PSId ON Reclutamiento.CertificadoWHO(PostulanteSolicitudId);
GO

CREATE TABLE Reclutamiento.EntrevistaPreseleccion (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteSolicitudId  int               NOT NULL,
    FechaHora              datetime2         NOT NULL,
    Checklist              nvarchar(max)     NULL, -- JSON
    Preguntas              nvarchar(max)     NULL, -- JSON
    Cierre                 nvarchar(max)     NULL,
    Cerrada                bit               NOT NULL DEFAULT 0,
    ReclutadorId           nvarchar(450)     NULL,
    CONSTRAINT PK_EntrevistaPreseleccion PRIMARY KEY (Id),
    CONSTRAINT FK_EntrevistaPreseleccion_PS FOREIGN KEY (PostulanteSolicitudId) REFERENCES Reclutamiento.PostulanteSolicitud(Id),
    CONSTRAINT FK_EntrevistaPreseleccion_Reclutador FOREIGN KEY (ReclutadorId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE UNIQUE INDEX IX_EntrevistaPreseleccion_PSId ON Reclutamiento.EntrevistaPreseleccion(PostulanteSolicitudId);
GO

-- ============================================================================
-- 7. RECLUTAMIENTO — v1 aún vigentes (hoja de vida / documentos, pendiente de
--    integrar al flujo activo de Preselección — ver BD_Dashboard.md)
-- ============================================================================

CREATE TABLE Reclutamiento.PostulanteHistorial (
    Id           int IDENTITY(1,1) NOT NULL,
    PostulanteId int               NOT NULL,
    Tipo         nvarchar(50)      NOT NULL, -- Experiencia / Educacion / Otro
    Descripcion  nvarchar(max)     NULL,
    FechaDesde   datetime2         NULL,
    FechaHasta   datetime2         NULL,
    Origen       nvarchar(20)      NOT NULL, -- Manual / IA
    CONSTRAINT PK_PostulanteHistorial PRIMARY KEY (Id),
    CONSTRAINT FK_PostulanteHistorial_Postulante FOREIGN KEY (PostulanteId) REFERENCES Reclutamiento.Postulante(Id)
);
GO

CREATE TABLE Reclutamiento.PostulanteDocumento (
    Id                  int IDENTITY(1,1) NOT NULL,
    PostulanteId        int               NOT NULL,
    Tipo                nvarchar(50)      NOT NULL, -- CV / CertificadoAntecedentes / Otro
    RutaArchivo         nvarchar(500)     NOT NULL,
    FechaCarga          datetime2         NOT NULL,
    EstadoProcesamiento nvarchar(20)      NOT NULL, -- Pendiente/Procesando/Listo/Error
    CONSTRAINT PK_PostulanteDocumento PRIMARY KEY (Id),
    CONSTRAINT FK_PostulanteDocumento_Postulante FOREIGN KEY (PostulanteId) REFERENCES Reclutamiento.Postulante(Id)
);
GO

CREATE TABLE Reclutamiento.DocumentoExtraccionIA (
    Id                     int IDENTITY(1,1) NOT NULL,
    PostulanteDocumentoId  int               NOT NULL,
    TextoExtraido          nvarchar(max)     NULL,
    DatosEstructurados     nvarchar(max)     NULL, -- JSON
    FechaProcesamiento     datetime2         NULL,
    Estado                 nvarchar(20)      NOT NULL, -- Pendiente/Procesando/Listo/Error
    CONSTRAINT PK_DocumentoExtraccionIA PRIMARY KEY (Id),
    CONSTRAINT FK_DocumentoExtraccionIA_Documento FOREIGN KEY (PostulanteDocumentoId) REFERENCES Reclutamiento.PostulanteDocumento(Id)
);
GO

-- ============================================================================
-- 8. GENÉRICO (dbo) — LogActividad: no pertenece a una sola app, se referencia
--    por Entidad+EntidadId en vez de FK real (puede apuntar a filas de
--    cualquier schema).
-- ============================================================================

CREATE TABLE dbo.LogActividad (
    Id        int IDENTITY(1,1) NOT NULL,
    UsuarioId nvarchar(450)     NOT NULL,
    Accion    nvarchar(200)     NOT NULL,
    Entidad   nvarchar(50)      NOT NULL,
    EntidadId int               NOT NULL,
    FechaHora datetime2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_LogActividad PRIMARY KEY (Id),
    CONSTRAINT FK_LogActividad_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.AspNetUsers(Id)
);
GO
CREATE INDEX IX_LogActividad_Entidad_EntidadId ON dbo.LogActividad(Entidad, EntidadId);
CREATE INDEX IX_LogActividad_FechaHora ON dbo.LogActividad(FechaHora);
GO

-- ============================================================================
-- 9. SEED — Roles de fábrica y su acceso a nivel de app (PerfilAplicacion)
--    No se siembra PerfilModulo todavía: el listado de módulos por app se
--    termina de fijar recién cuando se detalla cada componente de UI; se
--    configura desde Admin > Accesos por perfil una vez que la app exista.
-- ============================================================================

DECLARE @SuperAdmin nvarchar(450) = NEWID();
DECLARE @Admin nvarchar(450) = NEWID();
DECLARE @SupAdministrativo nvarchar(450) = NEWID();
DECLARE @SupOperaciones nvarchar(450) = NEWID();
DECLARE @Reclutador nvarchar(450) = NEWID();

INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
    (@SuperAdmin,        'SuperAdmin',                 'SUPERADMIN',                 NEWID()),
    (@Admin,             'Admin',                      'ADMIN',                      NEWID()),
    (@SupAdministrativo, 'Supervisor Administrativo',  'SUPERVISOR ADMINISTRATIVO',  NEWID()),
    (@SupOperaciones,    'Supervisor Operaciones',      'SUPERVISOR OPERACIONES',     NEWID()),
    (@Reclutador,        'Reclutador',                  'RECLUTADOR',                 NEWID());

-- Acceso por app, según admin.md (tabla "Modelo de permisos de Admin") y
-- Reclutamiento2.md (sección "Roles y accesos"):
INSERT INTO Admin.PerfilAplicacion (RoleId, App) VALUES
    (@SuperAdmin,        'Admin'),
    (@SuperAdmin,        'Reclutamiento'),
    (@SuperAdmin,        'Remuneracion'),
    (@Admin,             'Admin'),
    (@Admin,             'Reclutamiento'),
    (@Admin,             'Remuneracion'),
    (@SupAdministrativo, 'Admin'),          -- solo Clientes/Sucursales dentro de Admin
    (@SupAdministrativo, 'Reclutamiento'),
    (@SupAdministrativo, 'Remuneracion'),
    (@SupOperaciones,    'Reclutamiento'),
    (@Reclutador,        'Reclutamiento');
GO

-- ============================================================================
-- Fin de la instalación inicial.
-- Próximos pasos cuando corresponda: detallar Módulo 3 (Selección), Módulo 4
-- (Ingreso) y la app Remuneración con este mismo método antes de agregar sus
-- tablas a Remuneracion.*.
-- ============================================================================
