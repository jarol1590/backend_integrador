namespace BackendIntegrador.Application.Abstractions;

public interface ICrudService<TReadDto, TCreateDto, TUpdateDto>
{
    Task<IReadOnlyList<TReadDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TReadDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, TUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
