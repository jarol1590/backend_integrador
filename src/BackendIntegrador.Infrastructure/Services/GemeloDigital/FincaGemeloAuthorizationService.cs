using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class FincaGemeloAuthorizationService : IFincaGemeloAuthorizationService
{
    private readonly AppDbContext _db;

    public FincaGemeloAuthorizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task EnsureCanAccessFincaAsync(int usuarioId, int fincaId, CancellationToken cancellationToken = default)
    {
        if (await IsAdministradorAsync(usuarioId, cancellationToken))
            return;

        var ownsFinca = await _db.Fincas
            .AnyAsync(f => f.FincaId == fincaId && f.Productor.UsuarioId == usuarioId, cancellationToken);

        if (ownsFinca)
            return;

        var centroId = await _db.Usuarios
            .Where(u => u.UsuarioId == usuarioId)
            .Select(u => u.CentroAcopioId)
            .FirstOrDefaultAsync(cancellationToken);

        if (centroId.HasValue)
        {
            var hasLote = await _db.Lotes
                .AnyAsync(l =>
                    l.CentroAcopioId == centroId.Value &&
                    l.Ordeno.FincaId == fincaId,
                    cancellationToken);

            if (hasLote)
                return;
        }

        throw new UnauthorizedAccessException("No tiene permiso para acceder al gemelo de esta finca.");
    }

    public async Task EnsureCanAccessCentroAsync(int usuarioId, int centroAcopioId, CancellationToken cancellationToken = default)
    {
        if (await IsAdministradorAsync(usuarioId, cancellationToken))
            return;

        var belongs = await _db.Usuarios
            .AnyAsync(u => u.UsuarioId == usuarioId && u.CentroAcopioId == centroAcopioId, cancellationToken);

        if (!belongs)
            throw new UnauthorizedAccessException("No tiene permiso para acceder al gemelo de este centro de acopio.");
    }

    public async Task<bool> IsAdministradorAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return await _db.UsuarioRoles
            .AnyAsync(ur =>
                ur.UsuarioId == usuarioId &&
                ur.Rol.Nombre == UsuarioRoleTypes.RolNombreAdministrador,
                cancellationToken);
    }
}
