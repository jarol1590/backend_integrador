using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Services;

internal sealed class UsuarioFacadeService : IUsuarioFacadeService
{
    private readonly AppDbContext _db;

    public UsuarioFacadeService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<UsuarioListadoDto>> GetListadoAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _db.Usuarios
            .AsNoTracking()
            .Select(u => new
            {
                u.UsuarioId,
                u.Email,
                u.Estado,
                u.FechaCreacion,
                u.CentroAcopioId,
                CentroAcopioNombre = u.CentroAcopio != null ? u.CentroAcopio.Nombre : null,
                Roles = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList()
            })
            .ToListAsync(cancellationToken);

        return usuarios
            .Select(u => new UsuarioListadoDto(
                u.UsuarioId,
                u.Email,
                u.Estado,
                u.FechaCreacion,
                u.CentroAcopioNombre,
                u.Roles,
                UsuarioAlcanceHelper.DerivarTipoUsuario(u.Roles, u.CentroAcopioId)))
            .ToList();
    }

    public async Task<UsuarioPerfilDto?> GetPerfilAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await LoadUsuarioCompletoAsync(usuarioId, cancellationToken);
        return usuario is null ? null : MapPerfil(usuario);
    }

    public async Task<UsuarioPerfilDto> ProvisionarAsync(ProvisionarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ValidateEmail(dto.Email);
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("La contraseña es requerida.");
        if (dto.RolIds is null || dto.RolIds.Count == 0)
            throw new InvalidOperationException("Debe asignar al menos un rol.");

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new InvalidOperationException("El email ya está registrado.");

        var roles = await _db.Roles
            .Where(r => dto.RolIds.Contains(r.RolId))
            .ToListAsync(cancellationToken);

        if (roles.Count != dto.RolIds.Count)
            throw new InvalidOperationException("Uno o más roles no existen.");

        await ValidateProductorRequirementAsync(roles, dto.Productor, null, cancellationToken);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var usuario = new Usuario
            {
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Estado = dto.Estado,
                FechaCreacion = DateTime.UtcNow,
                CentroAcopioId = dto.CentroAcopioId
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var rolId in dto.RolIds)
                _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.UsuarioId, RolId = rolId });

            if (dto.Productor is not null)
            {
                var productor = new Productor
                {
                    Nombre = dto.Productor.Nombre,
                    Documento = dto.Productor.Documento,
                    Telefono = dto.Productor.Telefono,
                    UsuarioId = usuario.UsuarioId,
                    TipoDocumentoId = dto.Productor.TipoDocumentoId
                };
                _db.Productores.Add(productor);
                await _db.SaveChangesAsync(cancellationToken);

                if (dto.Productor.FincaInicial is not null)
                {
                    _db.Fincas.Add(new Finca
                    {
                        Nombre = dto.Productor.FincaInicial.Nombre,
                        Direccion = dto.Productor.FincaInicial.Direccion,
                        Latitud = dto.Productor.FincaInicial.Latitud,
                        Longitud = dto.Productor.FincaInicial.Longitud,
                        MunicipioId = dto.Productor.FincaInicial.MunicipioId,
                        ProductorId = productor.ProductorId
                    });
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (await GetPerfilAsync(usuario.UsuarioId, cancellationToken))!;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<UsuarioPerfilDto> ActualizarAsync(int usuarioId, ActualizarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ValidateEmail(dto.Email);
        if (dto.RolIds is null || dto.RolIds.Count == 0)
            throw new InvalidOperationException("Debe asignar al menos un rol.");

        var usuario = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
            .Include(u => u.Productor)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

        if (usuario is null)
            throw new KeyNotFoundException($"Usuario con id {usuarioId} no existe.");

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.UsuarioId != usuarioId, cancellationToken))
            throw new InvalidOperationException("El email ya está registrado.");

        var roles = await _db.Roles
            .Where(r => dto.RolIds.Contains(r.RolId))
            .ToListAsync(cancellationToken);

        if (roles.Count != dto.RolIds.Count)
            throw new InvalidOperationException("Uno o más roles no existen.");

        await ValidateProductorRequirementAsync(roles, dto.Productor, usuario.Productor?.ProductorId, cancellationToken);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            usuario.Email = dto.Email.Trim();
            usuario.Estado = dto.Estado;
            usuario.CentroAcopioId = dto.CentroAcopioId;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var rolesActuales = usuario.UsuarioRoles.ToList();
            var rolesNuevos = dto.RolIds.ToHashSet();

            foreach (var ur in rolesActuales.Where(ur => !rolesNuevos.Contains(ur.RolId)))
                _db.UsuarioRoles.Remove(ur);

            var idsActuales = rolesActuales.Select(ur => ur.RolId).ToHashSet();
            foreach (var rolId in rolesNuevos.Where(id => !idsActuales.Contains(id)))
                _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioId, RolId = rolId });

            if (dto.Productor is not null)
            {
                if (usuario.Productor is null)
                {
                    _db.Productores.Add(new Productor
                    {
                        Nombre = dto.Productor.Nombre,
                        Documento = dto.Productor.Documento,
                        Telefono = dto.Productor.Telefono,
                        UsuarioId = usuarioId,
                        TipoDocumentoId = dto.Productor.TipoDocumentoId
                    });
                }
                else
                {
                    usuario.Productor.Nombre = dto.Productor.Nombre;
                    usuario.Productor.Documento = dto.Productor.Documento;
                    usuario.Productor.Telefono = dto.Productor.Telefono;
                    usuario.Productor.TipoDocumentoId = dto.Productor.TipoDocumentoId;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return (await GetPerfilAsync(usuarioId, cancellationToken))!;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DesactivarAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _db.Usuarios.FindAsync(new object[] { usuarioId }, cancellationToken);
        if (usuario is null)
            throw new KeyNotFoundException($"Usuario con id {usuarioId} no existe.");

        usuario.Estado = "inactivo";
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario?> LoadUsuarioCompletoAsync(int usuarioId, CancellationToken cancellationToken) =>
        await _db.Usuarios
            .AsNoTracking()
            .Include(u => u.CentroAcopio!)
                .ThenInclude(c => c.Municipio)
                .ThenInclude(m => m.Fincas)
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .Include(u => u.Productor!)
                .ThenInclude(p => p.Fincas)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

    private static UsuarioPerfilDto MapPerfil(Usuario usuario)
    {
        var roles = usuario.UsuarioRoles
            .Select(ur => new RolResumenDto(ur.Rol.RolId, ur.Rol.Nombre, ur.Rol.Descripcion))
            .ToList();

        CentroAcopioResumenDto? centro = usuario.CentroAcopio is null
            ? null
            : new CentroAcopioResumenDto(usuario.CentroAcopio.CentroAcopioId, usuario.CentroAcopio.Nombre);

        return new UsuarioPerfilDto(
            usuario.UsuarioId,
            usuario.Email,
            usuario.Estado,
            usuario.FechaCreacion,
            centro,
            roles,
            UsuarioAlcanceHelper.BuildAlcance(usuario));
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new InvalidOperationException("El email tiene un formato inválido.");
    }

    private async Task ValidateProductorRequirementAsync(
        IReadOnlyList<Rol> roles,
        ProductorProvisionDto? productor,
        int? excludeProductorId,
        CancellationToken cancellationToken)
    {
        var requiereProductor = roles.Any(r =>
            r.Nombre.Contains("productor", StringComparison.OrdinalIgnoreCase));

        if (!requiereProductor)
            return;

        if (productor is null)
            throw new InvalidOperationException("Los usuarios con rol Productor requieren datos de productor.");

        if (await _db.Productores.AnyAsync(
                p => p.Documento == productor.Documento && p.ProductorId != excludeProductorId,
                cancellationToken))
            throw new InvalidOperationException("El documento del productor ya está registrado.");
    }
}
