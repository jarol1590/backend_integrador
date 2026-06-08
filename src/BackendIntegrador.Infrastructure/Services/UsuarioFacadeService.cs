using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Common;
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
                RolNombre = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return usuarios
            .Select(u => new UsuarioListadoDto(
                u.UsuarioId,
                u.Email,
                u.Estado,
                u.FechaCreacion,
                u.CentroAcopioNombre,
                u.RolNombre ?? "Sin rol",
                UsuarioRoleTypes.ResolveTipoFromRolNombre(u.RolNombre) ?? "sin_asignar"))
            .ToList();
    }

    public async Task<UsuarioPerfilBaseDto?> GetPerfilAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var baseData = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.UsuarioId == usuarioId)
            .Select(u => new
            {
                Usuario = u,
                Rol = u.UsuarioRoles.Select(ur => ur.Rol).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baseData?.Rol is null)
            return null;

        var tipo = UsuarioRoleTypes.ResolveTipoFromRolNombre(baseData.Rol.Nombre);
        if (tipo is null)
            return null;

        if (UsuarioRoleTypes.IsProductor(tipo))
        {
            var productor = await _db.Productores
                .AsNoTracking()
                .Include(p => p.Fincas)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);

            if (productor is null)
                return null;

            return UsuarioPerfilMapper.Map(baseData.Usuario, baseData.Rol, tipo, productor);
        }

        if (UsuarioRoleTypes.RequiresCentroAcopio(tipo))
        {
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.CentroAcopio)
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

            if (usuario?.CentroAcopio is null)
                return null;

            return UsuarioPerfilMapper.Map(usuario, baseData.Rol, tipo);
        }

        return UsuarioPerfilMapper.Map(baseData.Usuario, baseData.Rol, tipo);
    }

    public async Task<ProvisionarUsuarioDto?> GetInputAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var data = await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.UsuarioId == usuarioId)
            .Select(u => new
            {
                u.UsuarioId,
                u.Email,
                u.Estado,
                u.CentroAcopioId,
                RolId = u.UsuarioRoles.Select(ur => ur.RolId).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null || data.RolId == 0)
            return null;

        var rolNombre = await _db.Roles
            .Where(r => r.RolId == data.RolId)
            .Select(r => r.Nombre)
            .FirstOrDefaultAsync(cancellationToken);

        var tipo = UsuarioRoleTypes.ResolveTipoFromRolNombre(rolNombre);
        if (tipo is null)
            return null;

        var esProductor = UsuarioRoleTypes.IsProductor(tipo);
        var requiereCentro = UsuarioRoleTypes.RequiresCentroAcopio(tipo);

        int? centroAcopioId = requiereCentro ? data.CentroAcopioId : null;
        ProductorProvisionDto? productorDto = null;

        if (esProductor)
        {
            var productor = await _db.Productores
                .AsNoTracking()
                .Include(p => p.Fincas)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);

            if (productor is not null)
            {
                var primeraFinca = productor.Fincas
                    .OrderBy(f => f.FincaId)
                    .FirstOrDefault();

                productorDto = new ProductorProvisionDto(
                    productor.Nombre,
                    productor.Documento,
                    productor.Telefono,
                    productor.TipoDocumentoId,
                    primeraFinca is null
                        ? null
                        : new FincaInicialDto(
                            primeraFinca.Nombre,
                            primeraFinca.Direccion,
                            primeraFinca.Latitud,
                            primeraFinca.Longitud,
                            primeraFinca.MunicipioId));
            }
        }

        return new ProvisionarUsuarioDto(
            data.Email,
            string.Empty,
            data.Estado,
            data.RolId,
            centroAcopioId,
            productorDto);
    }

    public async Task<UsuarioPerfilBaseDto> ProvisionarAsync(ProvisionarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ValidateEmail(dto.Email);
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("La contraseña es requerida.");

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new InvalidOperationException("El email ya está registrado.");

        var rol = await _db.Roles.FindAsync(new object[] { dto.RolId }, cancellationToken);
        if (rol is null)
            throw new InvalidOperationException("El rol indicado no existe.");

        var tipo = UsuarioRoleValidator.ValidateProvision(rol, dto.CentroAcopioId, dto.Productor);
        await ValidateCentroAcopioExistsAsync(dto.CentroAcopioId, cancellationToken);
        await ValidateProductorDocumentAsync(dto.Productor, null, cancellationToken);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var centroAcopioId = UsuarioRoleTypes.IsProductor(tipo) ? null : dto.CentroAcopioId;

            var usuario = new Usuario
            {
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Estado = dto.Estado,
                FechaCreacion = DateTime.UtcNow,
                CentroAcopioId = centroAcopioId
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(cancellationToken);

            _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.UsuarioId, RolId = dto.RolId });

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

    public async Task<UsuarioPerfilBaseDto> ActualizarAsync(int usuarioId, ActualizarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        ValidateEmail(dto.Email);

        var usuario = await _db.Usuarios
            .Include(u => u.UsuarioRoles)
            .Include(u => u.Productor)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

        if (usuario is null)
            throw new KeyNotFoundException($"Usuario con id {usuarioId} no existe.");

        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.UsuarioId != usuarioId, cancellationToken))
            throw new InvalidOperationException("El email ya está registrado.");

        var rol = await _db.Roles.FindAsync(new object[] { dto.RolId }, cancellationToken);
        if (rol is null)
            throw new InvalidOperationException("El rol indicado no existe.");

        var hadProductor = usuario.Productor is not null;
        var tipo = UsuarioRoleValidator.ValidateUpdate(rol, dto.CentroAcopioId, dto.Productor, hadProductor);
        await ValidateCentroAcopioExistsAsync(dto.CentroAcopioId, cancellationToken);
        await ValidateProductorDocumentAsync(dto.Productor, usuario.Productor?.ProductorId, cancellationToken);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            usuario.Email = dto.Email.Trim();
            usuario.Estado = dto.Estado;
            usuario.CentroAcopioId = UsuarioRoleTypes.IsProductor(tipo) ? null : dto.CentroAcopioId;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            foreach (var ur in usuario.UsuarioRoles.ToList())
                _db.UsuarioRoles.Remove(ur);

            _db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuarioId, RolId = dto.RolId });

            if (UsuarioRoleTypes.IsProductor(tipo) && dto.Productor is not null)
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

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new InvalidOperationException("El email tiene un formato inválido.");
    }

    private async Task ValidateCentroAcopioExistsAsync(int? centroAcopioId, CancellationToken cancellationToken)
    {
        if (!centroAcopioId.HasValue)
            return;

        if (!await _db.CentrosAcopio.AnyAsync(c => c.CentroAcopioId == centroAcopioId.Value, cancellationToken))
            throw new InvalidOperationException("El Centro de Acopio indicado no existe.");
    }

    private async Task ValidateProductorDocumentAsync(
        ProductorProvisionDto? productor,
        int? excludeProductorId,
        CancellationToken cancellationToken)
    {
        if (productor is null)
            return;

        if (await _db.Productores.AnyAsync(
                p => p.Documento == productor.Documento && p.ProductorId != excludeProductorId,
                cancellationToken))
            throw new InvalidOperationException("El documento del productor ya está registrado.");
    }
}
