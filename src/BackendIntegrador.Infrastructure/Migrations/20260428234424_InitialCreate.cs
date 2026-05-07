using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendIntegrador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    DepartamentoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.DepartamentoId);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosCalidad",
                columns: table => new
                {
                    ParametroId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Unidad = table.Column<string>(type: "TEXT", nullable: true),
                    ValorMinimo = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    ValorMaximo = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosCalidad", x => x.ParametroId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolId);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    TipoDocumentoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumento", x => x.TipoDocumentoId);
                });

            migrationBuilder.CreateTable(
                name: "Transportes",
                columns: table => new
                {
                    TransporteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlacaVehiculo = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FechaHoraSalida = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaHoraEntrada = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TemperaturaInicio = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transportes", x => x.TransporteId);
                });

            migrationBuilder.CreateTable(
                name: "Municipios",
                columns: table => new
                {
                    MunicipioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DepartamentoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipios", x => x.MunicipioId);
                    table.ForeignKey(
                        name: "FK_Municipios_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "DepartamentoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CentrosAcopio",
                columns: table => new
                {
                    CentroAcopioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    Latitud = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: true),
                    Longitud = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: true),
                    MunicipioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosAcopio", x => x.CentroAcopioId);
                    table.ForeignKey(
                        name: "FK_CentrosAcopio_Municipios_MunicipioId",
                        column: x => x.MunicipioId,
                        principalTable: "Municipios",
                        principalColumn: "MunicipioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CentroAcopioId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_CentrosAcopio_CentroAcopioId",
                        column: x => x.CentroAcopioId,
                        principalTable: "CentrosAcopio",
                        principalColumn: "CentroAcopioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productores",
                columns: table => new
                {
                    ProductorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Documento = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoDocumentoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productores", x => x.ProductorId);
                    table.ForeignKey(
                        name: "FK_Productores_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "TipoDocumentoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesAcopio",
                columns: table => new
                {
                    RecepcionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransporteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroAcopioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHoraEntrada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    TemperaturaRecepcion = table.Column<int>(type: "INTEGER", nullable: true),
                    VolumenLitrosRecibidos = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesAcopio", x => x.RecepcionId);
                    table.ForeignKey(
                        name: "FK_RecepcionesAcopio_CentrosAcopio_CentroAcopioId",
                        column: x => x.CentroAcopioId,
                        principalTable: "CentrosAcopio",
                        principalColumn: "CentroAcopioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesAcopio_Transportes_TransporteId",
                        column: x => x.TransporteId,
                        principalTable: "Transportes",
                        principalColumn: "TransporteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesAcopio_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "RolId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Fincas",
                columns: table => new
                {
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    Latitud = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: true),
                    Longitud = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: true),
                    ProductorId = table.Column<int>(type: "INTEGER", nullable: false),
                    MunicipioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fincas", x => x.FincaId);
                    table.ForeignKey(
                        name: "FK_Fincas_Municipios_MunicipioId",
                        column: x => x.MunicipioId,
                        principalTable: "Municipios",
                        principalColumn: "MunicipioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fincas_Productores_ProductorId",
                        column: x => x.ProductorId,
                        principalTable: "Productores",
                        principalColumn: "ProductorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ordenos",
                columns: table => new
                {
                    OrdenoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FechaHoraInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VolumenLitros = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    FincaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenos", x => x.OrdenoId);
                    table.ForeignKey(
                        name: "FK_Ordenos_Fincas_FincaId",
                        column: x => x.FincaId,
                        principalTable: "Fincas",
                        principalColumn: "FincaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdenoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroAcopioId = table.Column<int>(type: "INTEGER", nullable: false),
                    VolumenCapturadoLitros = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    TransporteId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.LoteId);
                    table.ForeignKey(
                        name: "FK_Lotes_CentrosAcopio_CentroAcopioId",
                        column: x => x.CentroAcopioId,
                        principalTable: "CentrosAcopio",
                        principalColumn: "CentroAcopioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Ordenos_OrdenoId",
                        column: x => x.OrdenoId,
                        principalTable: "Ordenos",
                        principalColumn: "OrdenoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lotes_Transportes_TransporteId",
                        column: x => x.TransporteId,
                        principalTable: "Transportes",
                        principalColumn: "TransporteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Muestras",
                columns: table => new
                {
                    MuestraId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    TecnicoPorUsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHoraToma = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muestras", x => x.MuestraId);
                    table.ForeignKey(
                        name: "FK_Muestras_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "LoteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Muestras_Usuarios_TecnicoPorUsuarioId",
                        column: x => x.TecnicoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalisisCalidad",
                columns: table => new
                {
                    AnalisisId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MuestraId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaHoraAnalisis = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalisisCalidad", x => x.AnalisisId);
                    table.ForeignKey(
                        name: "FK_AnalisisCalidad_Muestras_MuestraId",
                        column: x => x.MuestraId,
                        principalTable: "Muestras",
                        principalColumn: "MuestraId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosParametro",
                columns: table => new
                {
                    AnalisisId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParametroId = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorResultado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Observacion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosParametro", x => new { x.AnalisisId, x.ParametroId });
                    table.ForeignKey(
                        name: "FK_ResultadosParametro_AnalisisCalidad_AnalisisId",
                        column: x => x.AnalisisId,
                        principalTable: "AnalisisCalidad",
                        principalColumn: "AnalisisId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosParametro_ParametrosCalidad_ParametroId",
                        column: x => x.ParametroId,
                        principalTable: "ParametrosCalidad",
                        principalColumn: "ParametroId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalisisCalidad_MuestraId",
                table: "AnalisisCalidad",
                column: "MuestraId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosAcopio_MunicipioId",
                table: "CentrosAcopio",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_Nombre",
                table: "Departamentos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fincas_MunicipioId",
                table: "Fincas",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "IX_Fincas_ProductorId",
                table: "Fincas",
                column: "ProductorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_CentroAcopioId",
                table: "Lotes",
                column: "CentroAcopioId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_OrdenoId",
                table: "Lotes",
                column: "OrdenoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_TransporteId",
                table: "Lotes",
                column: "TransporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Muestras_LoteId",
                table: "Muestras",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Muestras_TecnicoPorUsuarioId",
                table: "Muestras",
                column: "TecnicoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_DepartamentoId",
                table: "Municipios",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenos_FincaId",
                table: "Ordenos",
                column: "FincaId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosCalidad_Nombre",
                table: "ParametrosCalidad",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productores_Documento",
                table: "Productores",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productores_TipoDocumentoId",
                table: "Productores",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productores_UsuarioId",
                table: "Productores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesAcopio_CentroAcopioId",
                table: "RecepcionesAcopio",
                column: "CentroAcopioId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesAcopio_TransporteId",
                table: "RecepcionesAcopio",
                column: "TransporteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesAcopio_UsuarioId",
                table: "RecepcionesAcopio",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosParametro_ParametroId",
                table: "ResultadosParametro",
                column: "ParametroId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_Nombre",
                table: "TiposDocumento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolId",
                table: "UsuarioRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CentroAcopioId",
                table: "Usuarios",
                column: "CentroAcopioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecepcionesAcopio");

            migrationBuilder.DropTable(
                name: "ResultadosParametro");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "AnalisisCalidad");

            migrationBuilder.DropTable(
                name: "ParametrosCalidad");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Muestras");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Ordenos");

            migrationBuilder.DropTable(
                name: "Transportes");

            migrationBuilder.DropTable(
                name: "Fincas");

            migrationBuilder.DropTable(
                name: "Productores");

            migrationBuilder.DropTable(
                name: "TiposDocumento");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "CentrosAcopio");

            migrationBuilder.DropTable(
                name: "Municipios");

            migrationBuilder.DropTable(
                name: "Departamentos");
        }
    }
}
