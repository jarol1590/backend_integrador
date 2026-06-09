using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendIntegrador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCentroAcopioToParametroCalidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParametrosCalidad_Nombre",
                table: "ParametrosCalidad");

            migrationBuilder.AddColumn<int>(
                name: "CentroAcopioId",
                table: "ParametrosCalidad",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "ParametrosCalidad",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Orden",
                table: "ParametrosCalidad",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosCalidad_CentroAcopioId_Nombre",
                table: "ParametrosCalidad",
                columns: new[] { "CentroAcopioId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParametrosCalidad_CentrosAcopio_CentroAcopioId",
                table: "ParametrosCalidad",
                column: "CentroAcopioId",
                principalTable: "CentrosAcopio",
                principalColumn: "CentroAcopioId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParametrosCalidad_CentrosAcopio_CentroAcopioId",
                table: "ParametrosCalidad");

            migrationBuilder.DropIndex(
                name: "IX_ParametrosCalidad_CentroAcopioId_Nombre",
                table: "ParametrosCalidad");

            migrationBuilder.DropColumn(
                name: "CentroAcopioId",
                table: "ParametrosCalidad");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "ParametrosCalidad");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "ParametrosCalidad");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosCalidad_Nombre",
                table: "ParametrosCalidad",
                column: "Nombre",
                unique: true);
        }
    }
}
