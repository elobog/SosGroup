using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Reclutamiento");

            migrationBuilder.EnsureSchema(
                name: "Admin");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    UltimaVerificacion2FA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                schema: "Admin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContactoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactoCorreo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactoTelefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogActividad",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogActividad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Postulante",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Edad = table.Column<int>(type: "int", nullable: true),
                    Comuna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AniosExperiencia = table.Column<int>(type: "int", nullable: true),
                    RubroExperiencia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Nacionalidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Discapacidad = table.Column<bool>(type: "bit", nullable: true),
                    RentaPretendida = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    TituloCV = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DescripcionProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Habilidades = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postulante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilAplicacion",
                schema: "Admin",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    App = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilAplicacion", x => new { x.RoleId, x.App });
                    table.ForeignKey(
                        name: "FK_PerfilAplicacion_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilModulo",
                schema: "Admin",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    App = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilModulo", x => new { x.RoleId, x.App, x.Modulo });
                    table.ForeignKey(
                        name: "FK_PerfilModulo_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "varbinary(1024)", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CredencialPlataforma",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Plataforma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Conectado = table.Column<bool>(type: "bit", nullable: false),
                    NombreCuenta = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialPlataforma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredencialPlataforma_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteReclutador",
                schema: "Reclutamiento",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteReclutador", x => new { x.ClienteId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_ClienteReclutador_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteReclutador_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteSucursal",
                schema: "Admin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactoResponsableNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactoResponsableCargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactoResponsableCorreo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactoResponsableTelefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteSucursal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClienteSucursal_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteSupervisor",
                schema: "Reclutamiento",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteSupervisor", x => new { x.ClienteId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_ClienteSupervisor_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteSupervisor_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilCargo",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Requisitos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargo_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulanteDocumento",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoProcesamiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulanteDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulanteDocumento_Postulante_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Postulante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulanteHistorial",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaDesde = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulanteHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulanteHistorial_Postulante_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Postulante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilCargoVersion",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoId = table.Column<int>(type: "int", nullable: false),
                    NumeroVersion = table.Column<int>(type: "int", nullable: false),
                    RentaFija = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RentaVariable = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Beneficios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VersionBaseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargoVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargoVersion_PerfilCargoVersion_VersionBaseId",
                        column: x => x.VersionBaseId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilCargoVersion_PerfilCargo_PerfilCargoId",
                        column: x => x.PerfilCargoId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoExtraccionIA",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteDocumentoId = table.Column<int>(type: "int", nullable: false),
                    TextoExtraido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatosEstructurados = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaProcesamiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoExtraccionIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoExtraccionIA_PostulanteDocumento_PostulanteDocumentoId",
                        column: x => x.PostulanteDocumentoId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilCargoVersionAprobacion",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoVersionId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AprobadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargoVersionAprobacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargoVersionAprobacion_AspNetUsers_AprobadoPorUsuarioId",
                        column: x => x.AprobadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilCargoVersionAprobacion_PerfilCargoVersion_PerfilCargoVersionId",
                        column: x => x.PerfilCargoVersionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitud",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoSolicitud = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    PerfilCargoVersionId = table.Column<int>(type: "int", nullable: false),
                    FechaInicioServicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    DireccionServicio = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SupervisorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitud_AspNetUsers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_ClienteSucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "Admin",
                        principalTable: "ClienteSucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_PerfilCargoVersion_PerfilCargoVersionId",
                        column: x => x.PerfilCargoVersionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Apertura",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IniciadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apertura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apertura_AspNetUsers_IniciadoPorUsuarioId",
                        column: x => x.IniciadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Apertura_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteraccionIA",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    ReclutadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Proposito = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokensEntrada = table.Column<int>(type: "int", nullable: false),
                    TokensSalida = table.Column<int>(type: "int", nullable: false),
                    CostoUsd = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteraccionIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteraccionIA_AspNetUsers_ReclutadorId",
                        column: x => x.ReclutadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteraccionIA_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteraccionIA_Postulante_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Postulante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteraccionIA_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudDetalle",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    ReclutadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CantidadPersonalSolicitado = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudDetalle_AspNetUsers_ReclutadorId",
                        column: x => x.ReclutadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudDetalle_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aviso",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AperturaId = table.Column<int>(type: "int", nullable: false),
                    NumeroVersion = table.Column<int>(type: "int", nullable: false),
                    TituloCargo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Modalidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CierrePostulaciones = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CuerpoHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aviso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aviso_Apertura_AperturaId",
                        column: x => x.AperturaId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Apertura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aviso_AspNetUsers_CreadoPorUsuarioId",
                        column: x => x.CreadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publicacion",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvisoId = table.Column<int>(type: "int", nullable: false),
                    Plataforma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CantidadPostulantes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publicacion_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publicacion_Aviso_AvisoId",
                        column: x => x.AvisoId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Aviso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulacionCandidato",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicacionId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulacionCandidato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulacionCandidato_Publicacion_PublicacionId",
                        column: x => x.PublicacionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Publicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostulanteSolicitud",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: true),
                    SeleccionadoPreseleccion = table.Column<bool>(type: "bit", nullable: false),
                    EtapaPreseleccion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PuntajeInicial = table.Column<int>(type: "int", nullable: true),
                    PuntajeFinal = table.Column<int>(type: "int", nullable: true),
                    PublicacionId = table.Column<int>(type: "int", nullable: true),
                    CanalOrigen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SituacionActual = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UltimaActualizacionCV = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimoLoginPortal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdecuacionPortal = table.Column<int>(type: "int", nullable: true),
                    RespuestasPreguntasPortal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EtapaEvaluacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AprobadoPorSupervisorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComentarioEvaluacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulanteSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulanteSolicitud_AspNetUsers_AprobadoPorSupervisorId",
                        column: x => x.AprobadoPorSupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostulanteSolicitud_Postulante_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Postulante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostulanteSolicitud_Publicacion_PublicacionId",
                        column: x => x.PublicacionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Publicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostulanteSolicitud_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificadoWHO",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteSolicitudId = table.Column<int>(type: "int", nullable: false),
                    ReclutadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CostoUsd = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificadoWHO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificadoWHO_AspNetUsers_ReclutadorId",
                        column: x => x.ReclutadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CertificadoWHO_PostulanteSolicitud_PostulanteSolicitudId",
                        column: x => x.PostulanteSolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoLegal",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteSolicitudId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoArchivo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoLegal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoLegal_PostulanteSolicitud_PostulanteSolicitudId",
                        column: x => x.PostulanteSolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntrevistaPreseleccion",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteSolicitudId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Checklist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Preguntas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cierre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cerrada = table.Column<bool>(type: "bit", nullable: false),
                    ReclutadorId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrevistaPreseleccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrevistaPreseleccion_AspNetUsers_ReclutadorId",
                        column: x => x.ReclutadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntrevistaPreseleccion_PostulanteSolicitud_PostulanteSolicitudId",
                        column: x => x.PostulanteSolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreseleccionTarea",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteSolicitudId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Detalle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreseleccionTarea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreseleccionTarea_PostulanteSolicitud_PostulanteSolicitudId",
                        column: x => x.PostulanteSolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestPiamentor",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteSolicitudId = table.Column<int>(type: "int", nullable: false),
                    ReclutadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CostoUsd = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPiamentor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestPiamentor_AspNetUsers_ReclutadorId",
                        column: x => x.ReclutadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestPiamentor_PostulanteSolicitud_PostulanteSolicitudId",
                        column: x => x.PostulanteSolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PostulanteSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apertura_IniciadoPorUsuarioId",
                schema: "Reclutamiento",
                table: "Apertura",
                column: "IniciadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Apertura_SolicitudId",
                schema: "Reclutamiento",
                table: "Apertura",
                column: "SolicitudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Aviso_AperturaId",
                schema: "Reclutamiento",
                table: "Aviso",
                column: "AperturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Aviso_AperturaId_NumeroVersion",
                schema: "Reclutamiento",
                table: "Aviso",
                columns: new[] { "AperturaId", "NumeroVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aviso_CreadoPorUsuarioId",
                schema: "Reclutamiento",
                table: "Aviso",
                column: "CreadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoWHO_PostulanteSolicitudId",
                schema: "Reclutamiento",
                table: "CertificadoWHO",
                column: "PostulanteSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificadoWHO_ReclutadorId",
                schema: "Reclutamiento",
                table: "CertificadoWHO",
                column: "ReclutadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Estado",
                schema: "Admin",
                table: "Cliente",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_RUT",
                schema: "Admin",
                table: "Cliente",
                column: "RUT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClienteReclutador_UsuarioId",
                schema: "Reclutamiento",
                table: "ClienteReclutador",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteSucursal_ClienteId",
                schema: "Admin",
                table: "ClienteSucursal",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteSupervisor_UsuarioId",
                schema: "Reclutamiento",
                table: "ClienteSupervisor",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialPlataforma_Plataforma",
                schema: "Reclutamiento",
                table: "CredencialPlataforma",
                column: "Plataforma",
                unique: true,
                filter: "[UsuarioId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialPlataforma_Plataforma_UsuarioId",
                schema: "Reclutamiento",
                table: "CredencialPlataforma",
                columns: new[] { "Plataforma", "UsuarioId" },
                unique: true,
                filter: "[UsuarioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CredencialPlataforma_UsuarioId",
                schema: "Reclutamiento",
                table: "CredencialPlataforma",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoExtraccionIA_PostulanteDocumentoId",
                schema: "Reclutamiento",
                table: "DocumentoExtraccionIA",
                column: "PostulanteDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoLegal_PostulanteSolicitudId",
                schema: "Reclutamiento",
                table: "DocumentoLegal",
                column: "PostulanteSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrevistaPreseleccion_PostulanteSolicitudId",
                schema: "Reclutamiento",
                table: "EntrevistaPreseleccion",
                column: "PostulanteSolicitudId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntrevistaPreseleccion_ReclutadorId",
                schema: "Reclutamiento",
                table: "EntrevistaPreseleccion",
                column: "ReclutadorId");

            migrationBuilder.CreateIndex(
                name: "IX_InteraccionIA_ClienteId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_InteraccionIA_PostulanteId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "PostulanteId");

            migrationBuilder.CreateIndex(
                name: "IX_InteraccionIA_ReclutadorId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "ReclutadorId");

            migrationBuilder.CreateIndex(
                name: "IX_InteraccionIA_SolicitudId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActividad_Entidad_EntidadId",
                schema: "dbo",
                table: "LogActividad",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_LogActividad_FechaHora",
                schema: "dbo",
                table: "LogActividad",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargo_ClienteId",
                schema: "Reclutamiento",
                table: "PerfilCargo",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoVersion_PerfilCargoId",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                column: "PerfilCargoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoVersion_PerfilCargoId_NumeroVersion",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                columns: new[] { "PerfilCargoId", "NumeroVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoVersion_VersionBaseId",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                column: "VersionBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoVersionAprobacion_AprobadoPorUsuarioId",
                schema: "Reclutamiento",
                table: "PerfilCargoVersionAprobacion",
                column: "AprobadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoVersionAprobacion_PerfilCargoVersionId_Rol",
                schema: "Reclutamiento",
                table: "PerfilCargoVersionAprobacion",
                columns: new[] { "PerfilCargoVersionId", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilModulo_RoleId",
                schema: "Admin",
                table: "PerfilModulo",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulacionCandidato_PublicacionId",
                schema: "Reclutamiento",
                table: "PostulacionCandidato",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Postulante_RUT",
                schema: "Reclutamiento",
                table: "Postulante",
                column: "RUT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteDocumento_PostulanteId",
                schema: "Reclutamiento",
                table: "PostulanteDocumento",
                column: "PostulanteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteHistorial_PostulanteId",
                schema: "Reclutamiento",
                table: "PostulanteHistorial",
                column: "PostulanteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteSolicitud_AprobadoPorSupervisorId",
                schema: "Reclutamiento",
                table: "PostulanteSolicitud",
                column: "AprobadoPorSupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteSolicitud_PostulanteId_SolicitudId",
                schema: "Reclutamiento",
                table: "PostulanteSolicitud",
                columns: new[] { "PostulanteId", "SolicitudId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteSolicitud_PublicacionId",
                schema: "Reclutamiento",
                table: "PostulanteSolicitud",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteSolicitud_SolicitudId",
                schema: "Reclutamiento",
                table: "PostulanteSolicitud",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_PreseleccionTarea_PostulanteSolicitudId",
                schema: "Reclutamiento",
                table: "PreseleccionTarea",
                column: "PostulanteSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_PreseleccionTarea_PostulanteSolicitudId_Tipo",
                schema: "Reclutamiento",
                table: "PreseleccionTarea",
                columns: new[] { "PostulanteSolicitudId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publicacion_AvisoId",
                schema: "Reclutamiento",
                table: "Publicacion",
                column: "AvisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Publicacion_UsuarioId",
                schema: "Reclutamiento",
                table: "Publicacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_ClienteId",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_CodigoSolicitud",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "CodigoSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_Estado",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "PerfilCargoVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_SucursalId",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_SupervisorId",
                schema: "Reclutamiento",
                table: "Solicitud",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudDetalle_Estado",
                schema: "Reclutamiento",
                table: "SolicitudDetalle",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudDetalle_ReclutadorId",
                schema: "Reclutamiento",
                table: "SolicitudDetalle",
                column: "ReclutadorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudDetalle_SolicitudId",
                schema: "Reclutamiento",
                table: "SolicitudDetalle",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPiamentor_PostulanteSolicitudId",
                schema: "Reclutamiento",
                table: "TestPiamentor",
                column: "PostulanteSolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPiamentor_ReclutadorId",
                schema: "Reclutamiento",
                table: "TestPiamentor",
                column: "ReclutadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CertificadoWHO",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "ClienteReclutador",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "ClienteSupervisor",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "CredencialPlataforma",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "DocumentoExtraccionIA",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "DocumentoLegal",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "EntrevistaPreseleccion",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "InteraccionIA",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "LogActividad",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PerfilAplicacion",
                schema: "Admin");

            migrationBuilder.DropTable(
                name: "PerfilCargoVersionAprobacion",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PerfilModulo",
                schema: "Admin");

            migrationBuilder.DropTable(
                name: "PostulacionCandidato",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PostulanteHistorial",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PreseleccionTarea",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "SolicitudDetalle",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "TestPiamentor",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PostulanteDocumento",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PostulanteSolicitud",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Postulante",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Publicacion",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Aviso",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Apertura",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Solicitud",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ClienteSucursal",
                schema: "Admin");

            migrationBuilder.DropTable(
                name: "PerfilCargoVersion",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PerfilCargo",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "Cliente",
                schema: "Admin");
        }
    }
}
