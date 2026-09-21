using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class EvolucionaFlujoCalculoRemuneracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaEnvio",
                schema: "Remuneracion",
                table: "Calculo",
                newName: "FechaValidacion");

            migrationBuilder.RenameColumn(
                name: "FechaCierre",
                schema: "Remuneracion",
                table: "Calculo",
                newName: "FechaCruceRenta");

            migrationBuilder.AddColumn<string>(
                name: "CodigoTalana",
                schema: "Remuneracion",
                table: "ContratoServicio",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaVerificacionHomologacion",
                schema: "Remuneracion",
                table: "ContratoServicio",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Homologado",
                schema: "Remuneracion",
                table: "ContratoServicio",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Origen",
                schema: "Remuneracion",
                table: "CalculoEmpleadoDia",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CalculoEmpleadoBono",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculoEmpleadoId = table.Column<int>(type: "int", nullable: false),
                    TipoBono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CargadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EstadoEnvio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEnvioTalana = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DetalleError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculoEmpleadoBono", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleadoBono_AspNetUsers_CargadoPorUsuarioId",
                        column: x => x.CargadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleadoBono_CalculoEmpleado_CalculoEmpleadoId",
                        column: x => x.CalculoEmpleadoId,
                        principalSchema: "Remuneracion",
                        principalTable: "CalculoEmpleado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalculoEmpleadoDiaHistorial",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalculoEmpleadoDiaId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CampoModificado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculoEmpleadoDiaHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleadoDiaHistorial_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculoEmpleadoDiaHistorial_CalculoEmpleadoDia_CalculoEmpleadoDiaId",
                        column: x => x.CalculoEmpleadoDiaId,
                        principalSchema: "Remuneracion",
                        principalTable: "CalculoEmpleadoDia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstructuraRenta",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SueldoBase = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Gratificacion = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    FechaVigencia = table.Column<DateOnly>(type: "date", nullable: false),
                    ImportadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstructuraRenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstructuraRenta_AspNetUsers_ImportadoPorUsuarioId",
                        column: x => x.ImportadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstructuraRenta_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Admin",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstructuraRentaBono",
                schema: "Remuneracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstructuraRentaId = table.Column<int>(type: "int", nullable: false),
                    TipoBono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MontoEsperado = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstructuraRentaBono", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstructuraRentaBono_EstructuraRenta_EstructuraRentaId",
                        column: x => x.EstructuraRentaId,
                        principalSchema: "Remuneracion",
                        principalTable: "EstructuraRenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContratoServicio_CodigoTalana",
                schema: "Remuneracion",
                table: "ContratoServicio",
                column: "CodigoTalana",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calculo_UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo",
                column: "UsuarioCruceRentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Calculo_UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo",
                column: "UsuarioValidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleadoBono_CalculoEmpleadoId",
                schema: "Remuneracion",
                table: "CalculoEmpleadoBono",
                column: "CalculoEmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleadoBono_CargadoPorUsuarioId",
                schema: "Remuneracion",
                table: "CalculoEmpleadoBono",
                column: "CargadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleadoDiaHistorial_CalculoEmpleadoDiaId",
                schema: "Remuneracion",
                table: "CalculoEmpleadoDiaHistorial",
                column: "CalculoEmpleadoDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoEmpleadoDiaHistorial_UsuarioId",
                schema: "Remuneracion",
                table: "CalculoEmpleadoDiaHistorial",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstructuraRenta_ClienteId_Cargo_FechaVigencia",
                schema: "Remuneracion",
                table: "EstructuraRenta",
                columns: new[] { "ClienteId", "Cargo", "FechaVigencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstructuraRenta_ImportadoPorUsuarioId",
                schema: "Remuneracion",
                table: "EstructuraRenta",
                column: "ImportadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstructuraRentaBono_EstructuraRentaId",
                schema: "Remuneracion",
                table: "EstructuraRentaBono",
                column: "EstructuraRentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Calculo_AspNetUsers_UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo",
                column: "UsuarioCruceRentaId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Calculo_AspNetUsers_UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo",
                column: "UsuarioValidacionId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calculo_AspNetUsers_UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.DropForeignKey(
                name: "FK_Calculo_AspNetUsers_UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.DropTable(
                name: "CalculoEmpleadoBono",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "CalculoEmpleadoDiaHistorial",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "EstructuraRentaBono",
                schema: "Remuneracion");

            migrationBuilder.DropTable(
                name: "EstructuraRenta",
                schema: "Remuneracion");

            migrationBuilder.DropIndex(
                name: "IX_ContratoServicio_CodigoTalana",
                schema: "Remuneracion",
                table: "ContratoServicio");

            migrationBuilder.DropIndex(
                name: "IX_Calculo_UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.DropIndex(
                name: "IX_Calculo_UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.DropColumn(
                name: "CodigoTalana",
                schema: "Remuneracion",
                table: "ContratoServicio");

            migrationBuilder.DropColumn(
                name: "FechaUltimaVerificacionHomologacion",
                schema: "Remuneracion",
                table: "ContratoServicio");

            migrationBuilder.DropColumn(
                name: "Homologado",
                schema: "Remuneracion",
                table: "ContratoServicio");

            migrationBuilder.DropColumn(
                name: "Origen",
                schema: "Remuneracion",
                table: "CalculoEmpleadoDia");

            migrationBuilder.DropColumn(
                name: "UsuarioCruceRentaId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.DropColumn(
                name: "UsuarioValidacionId",
                schema: "Remuneracion",
                table: "Calculo");

            migrationBuilder.RenameColumn(
                name: "FechaValidacion",
                schema: "Remuneracion",
                table: "Calculo",
                newName: "FechaEnvio");

            migrationBuilder.RenameColumn(
                name: "FechaCruceRenta",
                schema: "Remuneracion",
                table: "Calculo",
                newName: "FechaCierre");
        }
    }
}
