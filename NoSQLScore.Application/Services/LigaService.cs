using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Services;

/// <summary>
/// Casos de uso relacionados con Ligas.
/// </summary>
public class LigaService
{
    private readonly ILigaRepository _ligaRepo;

    public LigaService(ILigaRepository ligaRepo)
    {
        _ligaRepo = ligaRepo;
    }

    public async Task<(LigaResponse? Response, string? Error)> CrearLigaAsync(
        CrearLigaRequest request, Guid creadorId)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (null, "El nombre de la liga es requerido.");

        var liga = new Liga
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            CreadorId = creadorId,
            Miembros = new List<Guid> { creadorId },
            FechaCreacion = DateTime.UtcNow
        };

        await _ligaRepo.CreateAsync(liga);
        return (MapToResponse(liga), null);
    }

    public async Task<PaginatedResponse<LigaResponse>> GetLigasDelUsuarioAsync(
        Guid usuarioId, int page, int limit)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 10;

        var (items, total) = await _ligaRepo.GetByMiembroAsync(usuarioId, page, limit);
        return new PaginatedResponse<LigaResponse>(
            items.Select(MapToResponse).ToList(), total, page, limit);
    }

    public async Task<(bool Success, string? Error)> UnirseAsync(Guid ligaId, Guid usuarioId)
    {
        var liga = await _ligaRepo.GetByIdAsync(ligaId);
        if (liga == null) return (false, "Liga no encontrada.");
        if (liga.Miembros.Contains(usuarioId)) return (false, "Ya eres miembro de esta liga.");

        await _ligaRepo.AddMiembroAsync(ligaId, usuarioId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SalirAsync(Guid ligaId, Guid usuarioId)
    {
        var liga = await _ligaRepo.GetByIdAsync(ligaId);
        if (liga == null) return (false, "Liga no encontrada.");
        if (!liga.Miembros.Contains(usuarioId)) return (false, "No eres miembro de esta liga.");
        if (liga.CreadorId == usuarioId) return (false, "El creador no puede abandonar la liga.");

        await _ligaRepo.RemoveMiembroAsync(ligaId, usuarioId);
        return (true, null);
    }

    public async Task<(List<Guid>? Miembros, string? Error)> GetMiembrosAsync(Guid ligaId)
    {
        var liga = await _ligaRepo.GetByIdAsync(ligaId);
        if (liga == null) return (null, "Liga no encontrada.");
        return (liga.Miembros, null);
    }

    public async Task<(bool? EsAdmin, string? Error)> EsAdminAsync(Guid ligaId, Guid usuarioId)
    {
        var liga = await _ligaRepo.GetByIdAsync(ligaId);
        if (liga == null) return (null, "Liga no encontrada.");
        return (liga.CreadorId == usuarioId, null);
    }

    private static LigaResponse MapToResponse(Liga l) =>
        new(l.Id, l.Nombre, l.Descripcion, l.CreadorId, l.Miembros, l.FechaCreacion);
}
