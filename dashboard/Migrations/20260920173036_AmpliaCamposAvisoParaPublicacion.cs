using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AmpliaCamposAvisoParaPublicacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ubicacion",
                schema: "Reclutamiento",
                table: "Aviso",
                newName: "PalabrasClave");

            migrationBuilder.AddColumn<int>(
                name: "AniosExperiencia",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comuna",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CursoDeseable",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EdadMaxima",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EdadMinima",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducacionEstado",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducacionMinima",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmpleoInclusivo",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JornadaTexto",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JornadaTipo",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NivelCargo",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalarioPeriodicidad",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalarioPublicado",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoContrato",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TurnoTexto",
                schema: "Reclutamiento",
                table: "Aviso",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AniosExperiencia",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "Comuna",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "CursoDeseable",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "EdadMaxima",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "EdadMinima",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "EducacionEstado",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "EducacionMinima",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "EmpleoInclusivo",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "JornadaTexto",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "JornadaTipo",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "NivelCargo",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "Region",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "SalarioPeriodicidad",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "SalarioPublicado",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "TipoContrato",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.DropColumn(
                name: "TurnoTexto",
                schema: "Reclutamiento",
                table: "Aviso");

            migrationBuilder.RenameColumn(
                name: "PalabrasClave",
                schema: "Reclutamiento",
                table: "Aviso",
                newName: "Ubicacion");
        }
    }
}
