using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AmpliaPerfilCargoFormatoEstandarSOSGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AniosExperienciaMinimo",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Area",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CondicionesEspeciales",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConocimientosTecnicos",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducacionMinima",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Habilidades",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetivoCargo",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrigenDocumento",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportaA",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerfilCargoDocumentoOriginal",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoVersionId = table.Column<int>(type: "int", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RutaBlob = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CargadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaCarga = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargoDocumentoOriginal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargoDocumentoOriginal_AspNetUsers_CargadoPorUsuarioId",
                        column: x => x.CargadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilCargoDocumentoOriginal_PerfilCargoVersion_PerfilCargoVersionId",
                        column: x => x.PerfilCargoVersionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilCargoFuncionCategoria",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoVersionId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargoFuncionCategoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargoFuncionCategoria_PerfilCargoVersion_PerfilCargoVersionId",
                        column: x => x.PerfilCargoVersionId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilCargoFuncionTarea",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerfilCargoFuncionCategoriaId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilCargoFuncionTarea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilCargoFuncionTarea_PerfilCargoFuncionCategoria_PerfilCargoFuncionCategoriaId",
                        column: x => x.PerfilCargoFuncionCategoriaId,
                        principalSchema: "Reclutamiento",
                        principalTable: "PerfilCargoFuncionCategoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoDocumentoOriginal_CargadoPorUsuarioId",
                schema: "Reclutamiento",
                table: "PerfilCargoDocumentoOriginal",
                column: "CargadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoDocumentoOriginal_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "PerfilCargoDocumentoOriginal",
                column: "PerfilCargoVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoFuncionCategoria_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "PerfilCargoFuncionCategoria",
                column: "PerfilCargoVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilCargoFuncionTarea_PerfilCargoFuncionCategoriaId",
                schema: "Reclutamiento",
                table: "PerfilCargoFuncionTarea",
                column: "PerfilCargoFuncionCategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfilCargoDocumentoOriginal",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PerfilCargoFuncionTarea",
                schema: "Reclutamiento");

            migrationBuilder.DropTable(
                name: "PerfilCargoFuncionCategoria",
                schema: "Reclutamiento");

            migrationBuilder.DropColumn(
                name: "AniosExperienciaMinimo",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "Area",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "CondicionesEspeciales",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "ConocimientosTecnicos",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "EducacionMinima",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "Habilidades",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "ObjetivoCargo",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "OrigenDocumento",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");

            migrationBuilder.DropColumn(
                name: "ReportaA",
                schema: "Reclutamiento",
                table: "PerfilCargoVersion");
        }
    }
}
