using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AgregaAccesoPostulantePortalPublico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAceptacionPolitica",
                schema: "Reclutamiento",
                table: "Postulante",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PoliticaAceptada",
                schema: "Reclutamiento",
                table: "Postulante",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PostulanteAccesoToken",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    NombreContacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorreoContacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PostulanteId = table.Column<int>(type: "int", nullable: true),
                    Proposito = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsadoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostulanteAccesoToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostulanteAccesoToken_Postulante_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Postulante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostulanteAccesoToken_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteAccesoToken_PostulanteId",
                schema: "Reclutamiento",
                table: "PostulanteAccesoToken",
                column: "PostulanteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteAccesoToken_SolicitudId",
                schema: "Reclutamiento",
                table: "PostulanteAccesoToken",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_PostulanteAccesoToken_Token",
                schema: "Reclutamiento",
                table: "PostulanteAccesoToken",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostulanteAccesoToken",
                schema: "Reclutamiento");

            migrationBuilder.DropColumn(
                name: "FechaAceptacionPolitica",
                schema: "Reclutamiento",
                table: "Postulante");

            migrationBuilder.DropColumn(
                name: "PoliticaAceptada",
                schema: "Reclutamiento",
                table: "Postulante");
        }
    }
}
