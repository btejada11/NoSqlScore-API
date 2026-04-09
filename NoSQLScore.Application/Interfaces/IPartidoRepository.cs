using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Interfaces;

/// <summary>
/// Contrato de repositorio para Partido.
/// La implementación real vive en Infrastructure.
/// </summary>
public interface IPartidoRepository
{
    Task<Partido?> GetByIdAsync(Guid id);
    Task<(List<Partido> Items, long Total)> GetAllAsync(Guid? ligaId, string? estado, int page, int limit);
    Task CreateAsync(Partido partido);
    Task UpdateResultadoAsync(Guid id, int resultadoLocal, int resultadoVisitante);
}
