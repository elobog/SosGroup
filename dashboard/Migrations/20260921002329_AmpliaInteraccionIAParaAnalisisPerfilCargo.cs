using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AmpliaInteraccionIAParaAnalisisPerfilCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SolicitudId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ReclutadorId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "PostulanteId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteraccionIA_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "PerfilCargoVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InteraccionIA_PerfilCargoVersion_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                column: "PerfilCargoVersionId",
                principalSchema: "Reclutamiento",
                principalTable: "PerfilCargoVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteraccionIA_PerfilCargoVersion_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA");

            migrationBuilder.DropIndex(
                name: "IX_InteraccionIA_PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA");

            migrationBuilder.DropColumn(
                name: "PerfilCargoVersionId",
                schema: "Reclutamiento",
                table: "InteraccionIA");

            migrationBuilder.AlterColumn<int>(
                name: "SolicitudId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReclutadorId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostulanteId",
                schema: "Reclutamiento",
                table: "InteraccionIA",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
