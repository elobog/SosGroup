using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AgregaPrecalificacionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrecalificacionConfig",
                schema: "Reclutamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    SexoPeso = table.Column<int>(type: "int", nullable: false),
                    SexoObjetivo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EdadPeso = table.Column<int>(type: "int", nullable: false),
                    EdadMin = table.Column<int>(type: "int", nullable: false),
                    EdadMax = table.Column<int>(type: "int", nullable: false),
                    ComunaPeso = table.Column<int>(type: "int", nullable: false),
                    ComunaObjetivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ExperienciaPeso = table.Column<int>(type: "int", nullable: false),
                    ExperienciaMinimo = table.Column<int>(type: "int", nullable: false),
                    RubroPeso = table.Column<int>(type: "int", nullable: false),
                    RubroObjetivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrecalificacionConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrecalificacionConfig_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalSchema: "Reclutamiento",
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrecalificacionConfig_SolicitudId",
                schema: "Reclutamiento",
                table: "PrecalificacionConfig",
                column: "SolicitudId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrecalificacionConfig",
                schema: "Reclutamiento");
        }
    }
}
