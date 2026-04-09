using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;
using NoSQLScore.Infrastructure.Data;

namespace NoSQLScore.Infrastructure.Repositories;

/// <summary>
/// Implementación de IPartidoRepository usando MongoDB.
/// </summary>
public class PartidoRepository : IPartidoRepository
{
    private readonly MongoDbContext _context;

    public PartidoRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Partido?> GetByIdAsync(Guid id) =>
        await _context.Partidos.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<(List<Partido> Items, long Total)> GetAllAsync(
        Guid? ligaId, string? estado, int page, int limit)
    {
        var filtroBuilder = Builders<Partido>.Filter;
        var filtro = filtroBuilder.Empty;

        if (ligaId.HasValue)
            filtro &= filtroBuilder.Eq(p => p.LigaId, ligaId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            filtro &= filtroBuilder.Eq(p => p.Estado, estado.ToLower());

        var total = await _context.Partidos.CountDocumentsAsync(filtro);
        var items = await _context.Partidos
            .Find(filtro)
            .Skip((page - 1) * limit)
            .Limit(limit)
            .ToListAsync();

        return (items, total);
    }

    public async Task CreateAsync(Partido partido) =>
        await _context.Partidos.InsertOneAsync(partido);

    public async Task UpdateResultadoAsync(Guid id, int resultadoLocal, int resultadoVisitante)
    {
        var update = Builders<Partido>.Update
            .Set(p => p.ResultadoLocal, resultadoLocal)
            .Set(p => p.ResultadoVisitante, resultadoVisitante)
            .Set(p => p.Estado, "cerrado");

        await _context.Partidos.UpdateOneAsync(p => p.Id == id, update);
    }
}
