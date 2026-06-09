using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendIntegrador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrabajadorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trabajadores",
                columns: table => new
                {
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Documento = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajadores", x => x.TrabajadorId);
                    table.ForeignKey(
                        name: "FK_Trabajadores_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "TipoDocumentoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trabajadores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_Documento",
                table: "Trabajadores",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_TipoDocumentoId",
                table: "Trabajadores",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_UsuarioId",
                table: "Trabajadores",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trabajadores");
        }
    }
}
