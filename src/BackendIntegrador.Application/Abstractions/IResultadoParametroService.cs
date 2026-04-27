using BackendIntegrador.Application.Dtos;

namespace BackendIntegrador.Application.Abstractions;

public interface IResultadoParametroService
{
    Task<IReadOnlyList<ResultadoParametroDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ResultadoParametroDto?> GetAsync(int analisisId, int parametroId, CancellationToken cancellationToken = default);
    Task<ResultadoParametroDto> CreateAsync(CreateResultadoParametroDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int analisisId, int parametroId, UpdateResultadoParametroDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int analisisId, int parametroId, CancellationToken cancellationToken = default);
}
