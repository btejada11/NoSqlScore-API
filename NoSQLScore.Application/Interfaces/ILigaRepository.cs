using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Interfaces;

/// <summary>
/// Contrato de repositorio para Liga.
/// La implementación real vive en Infrastructure.
/// </summary>
public interface ILigaRepository
{
    Task<Liga?> GetByIdAsync(Guid id);
    Task<(List<Liga> Items, long Total)> GetByMiembroAsync(Guid usuarioId, int page, int limit);
    Task CreateAsync(Liga liga);
    Task AddMiembroAsync(Guid ligaId, Guid usuarioId);
    Task RemoveMiembroAsync(Guid ligaId, Guid usuarioId);
}
