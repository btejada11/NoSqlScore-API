using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Interfaces;

/// <summary>
/// Contrato de repositorio para Usuario.
/// La implementación real vive en Infrastructure.
/// Application solo conoce esta interfaz.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<(List<Usuario> Items, long Total)> GetAllAsync(int page, int limit);
    Task CreateAsync(Usuario usuario);
    Task UpdateAsync(Guid id, string? nuevoNombre, string? nuevoAvatar);
}

