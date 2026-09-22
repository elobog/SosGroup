using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class PermiteAccesoRetornoSinSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SolicitudId",
                schema: "Reclutamiento",
                table: "PostulanteAccesoToken",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SolicitudId",
                schema: "Reclutamiento",
                table: "PostulanteAccesoToken",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
