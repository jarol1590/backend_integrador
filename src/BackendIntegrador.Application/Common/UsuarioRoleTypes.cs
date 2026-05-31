namespace BackendIntegrador.Application.Common;

public static class UsuarioRoleTypes
{
    public const string Administrador = "administrador";
    public const string CentroAcopio = "centro_acopio";
    public const string Productor = "productor";
    public const string TrabajadorCentroAcopio = "trabajador_centro_acopio";

    public const string RolNombreAdministrador = "Administrador";
    public const string RolNombreCentroAcopio = "Centro de Acopio";
    public const string RolNombreProductor = "Productor";
    public const string RolNombreTrabajadorCentroAcopio = "Trabajador Centro de acopio";

    private static readonly Dictionary<string, string> NombreToTipo = new(StringComparer.OrdinalIgnoreCase)
    {
        [RolNombreAdministrador] = Administrador,
        [RolNombreCentroAcopio] = CentroAcopio,
        [RolNombreProductor] = Productor,
        [RolNombreTrabajadorCentroAcopio] = TrabajadorCentroAcopio,
    };

    public static string? ResolveTipoFromRolNombre(string? rolNombre) =>
        rolNombre is null ? null : NombreToTipo.GetValueOrDefault(rolNombre.Trim());

    public static bool RequiresCentroAcopio(string tipoUsuario) =>
        tipoUsuario is CentroAcopio or TrabajadorCentroAcopio;

    public static bool IsProductor(string tipoUsuario) =>
        tipoUsuario == Productor;
}
