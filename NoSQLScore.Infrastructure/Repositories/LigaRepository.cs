using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;
using NoSQLScore.Infrastructure.Data;

namespace NoSQLScore.Infrastructure.Repositories;

/// <summary>
/// Implementación de ILigaRepository usando MongoDB.
/// </summary>
public class LigaRepository : ILigaRepository
{
    private readonly MongoDbContext _context;

    public LigaRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Liga?> GetByIdAsync(Guid id) =>
        await _context.Ligas.Find(l => l.Id == id).FirstOrDefaultAsync();

    public async Task<(List<Liga> Items, long Total)> GetByMiembroAsync(
        Guid usuarioId, int page, int limit)
    {
        var filtro = Builders<Liga>.Filter.AnyEq(l => l.Miembros, usuarioId);
        var total = await _context.Ligas.CountDocumentsAsync(filtro);
        var items = await _context.Ligas
            .Find(filtro)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return (items, total);
    }

    public async Task CreateAsync(Liga liga) =>
        await _context.Ligas.InsertOneAsync(liga);

    public async Task AddMiembroAsync(Guid ligaId, Guid usuarioId)
    {
        var update = Builders<Liga>.Update.Push(l => l.Miembros, usuarioId);
        await _context.Ligas.UpdateOneAsync(l => l.Id == ligaId, update);
    }

    public async Task RemoveMiembroAsync(Guid ligaId, Guid usuarioId)
    {
        var update = Builders<Liga>.Update.Pull(l => l.Miembros, usuarioId);
        await _context.Ligas.UpdateOneAsync(l => l.Id == ligaId, update);
    }
}

