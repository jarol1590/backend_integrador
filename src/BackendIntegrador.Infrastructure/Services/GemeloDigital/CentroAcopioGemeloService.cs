using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Services.GemeloDigital;

public sealed class CentroAcopioGemeloService : ICentroAcopioGemeloService
{
    private readonly AppDbContext _db;

    public CentroAcopioGemeloService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CentroAcopioRiesgoRegionalDto> GetRiesgoRegionalAsync(
        int centroAcopioId,
        CancellationToken cancellationToken = default)
    {
        var centro = await _db.CentrosAcopio
            .FirstOrDefaultAsync(c => c.CentroAcopioId == centroAcopioId, cancellationToken);

        if (centro is null)
            throw new KeyNotFoundException("Centro de acopio no encontrado.");

        var desde = DateTime.UtcNow.AddDays(-90);
        var fincaIds = await _db.Lotes
            .Where(l => l.CentroAcopioId == centroAcopioId && l.Ordeno.FechaHoraInicio >= desde)
            .Select(l => l.Ordeno.FincaId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var fincas = await _db.Fincas
            .Where(f => fincaIds.Contains(f.FincaId))
            .Include(f => f.Municipio)
            .Include(f => f.GemeloEstado)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var items = new List<RiesgoRegionalFincaDto>();

        foreach (var finca in fincas)
        {
            var alertasActivas = await _db.AlertasGemelo
                .CountAsync(a =>
                    a.FincaId == finca.FincaId &&
                    !a.Leida &&
                    (a.ExpiraUtc == null || a.ExpiraUtc > now),
                    cancellationToken);

            var ultimaLectura = await _db.LecturasClimaticas
                .Where(l => l.FincaId == finca.FincaId)
                .OrderByDescending(l => l.Fecha)
                .FirstOrDefaultAsync(cancellationToken);

            items.Add(new RiesgoRegionalFincaDto(
                finca.FincaId,
                finca.Nombre,
                finca.Municipio.Nombre,
                finca.GemeloEstado?.ScoreRiesgoGlobal ?? 0,
                alertasActivas,
                ultimaLectura?.TempMedia,
                finca.Latitud,
                finca.Longitud));
        }

        var promedio = items.Count > 0
            ? (decimal)items.Average(i => i.ScoreRiesgoGlobal)
            : 0m;

        return new CentroAcopioRiesgoRegionalDto(
            centroAcopioId,
            centro.Nombre,
            DateTime.UtcNow,
            items.OrderByDescending(i => i.ScoreRiesgoGlobal).ToList(),
            promedio);
    }
}
