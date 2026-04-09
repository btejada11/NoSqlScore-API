using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Services;

/// <summary>
/// Casos de uso relacionados con Partidos.
/// </summary>
public class PartidoService
{
    private readonly IPartidoRepository _partidoRepo;
    private readonly ILigaRepository _ligaRepo;

    public PartidoService(IPartidoRepository partidoRepo, ILigaRepository ligaRepo)
    {
        _partidoRepo = partidoRepo;
        _ligaRepo = ligaRepo;
    }

    public async Task<(PartidoResponse? Response, string? Error, int StatusCode)> CrearPartidoAsync(
        PartidoRequest request, Guid solicitanteId)
    {
        if (string.IsNullOrWhiteSpace(request.Local) || string.IsNullOrWhiteSpace(request.Visitante))
            return (null, "Los nombres de los equipos son requeridos.", 400);

        var liga = await _ligaRepo.GetByIdAsync(request.LigaId);
        if (liga == null) return (null, "Liga no encontrada.", 404);

        if (liga.CreadorId != solicitanteId)
            return (null, "Solo el administrador de la liga puede crear partidos.", 403);

        var partido = new Partido
        {
            LigaId = request.LigaId,
            Local = request.Local,
            Visitante = request.Visitante,
            Fecha = request.Fecha,
            Estado = "abierto",
            PuntosProcesados = false
        };

        await _partidoRepo.CreateAsync(partido);
        return (MapToResponse(partido), null, 201);
    }

    public async Task<PaginatedResponse<PartidoResponse>> ListarPartidosAsync(
        Guid? ligaId, string? estado, int page, int limit)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 10;

        var (items, total) = await _partidoRepo.GetAllAsync(ligaId, estado, page, limit);
        return new PaginatedResponse<PartidoResponse>(
            items.Select(MapToResponse).ToList(), total, page, limit);
    }

    public async Task<PartidoResponse?> GetPartidoAsync(Guid id)
    {
        var partido = await _partidoRepo.GetByIdAsync(id);
        return partido == null ? null : MapToResponse(partido);
    }

    public async Task<(PartidoResponse? Response, string? Error, int StatusCode)> CerrarPartidoAsync(
        Guid id, Guid solicitanteId, CerrarPartidoRequest request)
    {
        var partido = await _partidoRepo.GetByIdAsync(id);
        if (partido == null) return (null, "Partido no encontrado.", 404);
        if (partido.Estado == "cerrado") return (null, "El partido ya está cerrado.", 400);

        if (request.ResultadoLocal < 0 || request.ResultadoVisitante < 0)
            return (null, "Los resultados no pueden ser negativos.", 400);

        var liga = await _ligaRepo.GetByIdAsync(partido.LigaId);
        if (liga == null) return (null, "Liga no encontrada.", 404);

        if (liga.CreadorId != solicitanteId)
            return (null, "Solo el administrador de la liga puede cerrar partidos.", 403);

        await _partidoRepo.UpdateResultadoAsync(id, request.ResultadoLocal, request.ResultadoVisitante);

        // TODO: Llamar al Módulo 4 (Pronósticos / Cassandra) para procesar puntos.
        // Una vez implementado el Módulo 4, inyectar IPronosticosService y llamar:
        //   await _pronosticosService.ProcesarResultadoAsync(id, partido.LigaId,
        //       request.ResultadoLocal, request.ResultadoVisitante);

        var actualizado = await _partidoRepo.GetByIdAsync(id);
        return (MapToResponse(actualizado!), null, 200);
    }

    private static PartidoResponse MapToResponse(Partido p) =>
        new(p.Id, p.LigaId, p.Local, p.Visitante, p.Fecha,
            p.ResultadoLocal, p.ResultadoVisitante, p.Estado);
}

