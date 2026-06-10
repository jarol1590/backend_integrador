using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendIntegrador.Infrastructure.Migrations
{
    public partial class AddCodigoToOrdenoAndLote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Ordenos",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Lotes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenos_Codigo",
                table: "Ordenos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_Codigo",
                table: "Lotes",
                column: "Codigo",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ordenos_Codigo",
                table: "Ordenos");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_Codigo",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Ordenos");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Lotes");
        }
    }
}
