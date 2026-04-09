using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;
using NoSQLScore.Infrastructure.Data;

namespace NoSQLScore.Infrastructure.Repositories;

/// <summary>
/// Implementación de IUsuarioRepository usando MongoDB.
/// Solo esta clase (en Infrastructure) conoce MongoDB.Driver.
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly MongoDbContext _context;

    public UsuarioRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id) =>
        await _context.Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _context.Usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<(List<Usuario> Items, long Total)> GetAllAsync(int page, int limit)
    {
        var total = await _context.Usuarios.CountDocumentsAsync(_ => true);
        var items = await _context.Usuarios
            .Find(_ => true)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return (items, total);
    }

    public async Task CreateAsync(Usuario usuario) =>
        await _context.Usuarios.InsertOneAsync(usuario);

    public async Task UpdateAsync(Guid id, string? nuevoNombre, string? nuevoAvatar)
    {
        var updates = new List<UpdateDefinition<Usuario>>();

        if (!string.IsNullOrWhiteSpace(nuevoNombre))
            updates.Add(Builders<Usuario>.Update.Set(u => u.Nombre, nuevoNombre));

        if (nuevoAvatar != null)
            updates.Add(Builders<Usuario>.Update.Set(u => u.Avatar, nuevoAvatar));

        if (updates.Count == 0) return;

        var combined = Builders<Usuario>.Update.Combine(updates);
        await _context.Usuarios.UpdateOneAsync(u => u.Id == id, combined);
    }
}
