using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendIntegrador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGemeloDigital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasGemelo",
                columns: table => new
                {
                    AlertaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoAlerta = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Severidad = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Mensaje = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Recomendacion = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreadaUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiraUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Leida = table.Column<bool>(type: "INTEGER", nullable: false),
                    LeidaUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasGemelo", x => x.AlertaId);
                    table.ForeignKey(
                        name: "FK_AlertasGemelo_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "FincaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FincasGemeloEstado",
                columns: table => new
                {
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimaSyncUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VersionMotor = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FuenteClima = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ScoreRiesgoGlobal = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoSync = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UltimoError = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreadoUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FincasGemeloEstado", x => x.FincaId);
                    table.ForeignKey(
                        name: "FK_FincasGemeloEstado_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "FincaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LecturasClimaticas",
                columns: table => new
                {
                    LecturaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TempMin = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    TempMax = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    TempMedia = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: false),
                    HumedadMedia = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    PrecipitacionMm = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    ThiMax = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    DiasConsecutivosCalor = table.Column<int>(type: "INTEGER", nullable: false),
                    Fuente = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturasClimaticas", x => x.LecturaId);
                    table.ForeignKey(
                        name: "FK_LecturasClimaticas_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "FincaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrediccionesGemelo",
                columns: table => new
                {
                    PrediccionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneradaUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HorizonteDias = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoPrediccion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Confianza = table.Column<decimal>(type: "TEXT", precision: 4, scale: 3, nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    DetalleJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrediccionesGemelo", x => x.PrediccionId);
                    table.ForeignKey(
                        name: "FK_PrediccionesGemelo_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "FincaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasGemelo_ExpiraUtc",
                table: "AlertasGemelo",
                column: "ExpiraUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasGemelo_FincaId_Leida_CreadaUtc",
                table: "AlertasGemelo",
                columns: new[] { "FincaId", "Leida", "CreadaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LecturasClimaticas_Fecha",
                table: "LecturasClimaticas",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_LecturasClimaticas_FincaId_Fecha",
                table: "LecturasClimaticas",
                columns: new[] { "FincaId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesGemelo_FincaId_TipoPrediccion_HorizonteDias",
                table: "PrediccionesGemelo",
                columns: new[] { "FincaId", "TipoPrediccion", "HorizonteDias" });

            migrationBuilder.CreateIndex(
                name: "IX_PrediccionesGemelo_GeneradaUtc",
                table: "PrediccionesGemelo",
                column: "GeneradaUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasGemelo");

            migrationBuilder.DropTable(
                name: "FincasGemeloEstado");

            migrationBuilder.DropTable(
                name: "LecturasClimaticas");

            migrationBuilder.DropTable(
                name: "PrediccionesGemelo");
        }
    }
}
