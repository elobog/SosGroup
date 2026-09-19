using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRemuneracionFase0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Remuneracion");

            migrationBuilder.CreateTable(
                name: "AuditoriaEjecucion",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MesAuditado = table.Column<DateOnly>(type: "date", nullable: false),
                    ContratosRevisados = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaEjecucion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContratoServicio",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaTermino = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratoServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoServicio_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calculo",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContratoServicioId = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calculo_ContratoServicio_ContratoServicioId",
                        column: x => x.ContratoServicioId,
                        principalSchema: "Remuneracion",
                        principalTable: "ContratoServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpleadoTalana",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RUT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ContratoServicioId = table.Column<int>(type: "int", nullable: true),
                    Cargo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EstadoTalana = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaUltimaSincronizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadoTalana", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpleadoTalana_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpleadoTalana_ContratoServicio_ContratoServicioId",
                        column: x => x.ContratoServicioId,
                        principalSchema: "Remuneracion",
                        principalTable: "ContratoServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalculoEmpleado",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculoId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoTalanaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculoEmpleado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleado_Calculo_CalculoId",
                        column: x => x.CalculoId,
                        principalSchema: "Remuneracion",
                        principalTable: "Calculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleado_EmpleadoTalana_EmpleadoTalanaId",
                        column: x => x.EmpleadoTalanaId,
                        principalSchema: "Remuneracion",
                        principalTable: "EmpleadoTalana",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaInconsistencia",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditoriaEjecucionId = table.Column<int>(type: "int", nullable: false),
                    CalculoEmpleadoId = table.Column<int>(type: "int", nullable: false),
                    TipoInconsistencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaInconsistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaInconsistencia_AuditoriaEjecucion_AuditoriaEjecucionId",
                        column: x => x.AuditoriaEjecucionId,
                        principalSchema: "Remuneracion",
                        principalTable: "AuditoriaEjecucion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditoriaInconsistencia_CalculoEmpleado_CalculoEmpleadoId",
                        column: x => x.CalculoEmpleadoId,
                        principalSchema: "Remuneracion",
                        principalTable: "CalculoEmpleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalculoEmpleadoDia",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculoEmpleadoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    TipoJornada = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HorasExtra = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculoEmpleadoDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleadoDia_CalculoEmpleado_CalculoEmpleadoId",
                        column: x => x.CalculoEmpleadoId,
                        principalSchema: "Remuneracion",
                        principalTable: "CalculoEmpleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaInconsistencia_AuditoriaEjecucionId",
                schema: "Remuneracion",
                table: "AuditoriaInconsistencia",
                column: "AuditoriaEjecucionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaInconsistencia_CalculoEmpleadoId",
                schema: "Remuneracion",
                table: "AuditoriaInconsistencia",
                column: "CalculoEmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Calculo_ContratoServicioId_Periodo",
                schema: "Remuneracion",
                table: "Calculo",
                columns: new[] { "ContratoServicioId", "Periodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleado_CalculoId_EmpleadoTalanaId",
                schema: "Remuneracion",
                table: "CalculoEmpleado",
                columns: new[] { "CalculoId", "EmpleadoTalanaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleado_EmpleadoTalanaId",
                schema: "Remuneracion",
                table: "CalculoEmpleado",
                column: "EmpleadoTalanaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleadoDia_CalculoEmpleadoId_Fecha",
                schema: "Remuneracion",
                table: "CalculoEmpleadoDia",
                columns: new[] { "CalculoEmpleadoId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratoServicio_ClienteId",
                schema: "Remuneracion",
                table: "ContratoServicio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoServicio_Estado",
                schema: "Remuneracion",
                table: "ContratoServicio",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoTalana_ClienteId",
                schema: "Remuneracion",
                table: "EmpleadoTalana",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoTalana_ContratoServicioId",
                schema: "Remuneracion",
                table: "EmpleadoTalana",
                column: "ContratoServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoTalana_RUT",
                schema: "Remuneracion",
                table: "EmpleadoTalana",
                column: "RUT",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaInconsistencia",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "CalculoEmpleadoDia",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "AuditoriaEjecucion",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "CalculoEmpleado",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "Calculo",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "EmpleadoTalana",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "ContratoServicio",
                schema: "Remuneracion");
        }
    }
}
