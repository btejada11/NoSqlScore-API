using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Services;

public class PronosticoService
{
    private readonly IPronosticoRepository _pronosticoRepository;

    public PronosticoService(IPronosticoRepository pronosticoRepository)
    {
        _pronosticoRepository = pronosticoRepository;
    }

    public async Task<(PronosticoResponse? Response, string? Error)> CrearPronosticoAsync(
        crearPronosticoRequest request, Guid usuarioId)
    {
        if (request.GolesLocal < 0 || request.GolesVisitante < 0)
            return (null, "Los goles no pueden ser negativos.");

        var pronostico = new Pronostico
        {
            PartidoId = request.PartidoId,
            UsuarioId = usuarioId,
            GolesLocal = request.GolesLocal,
            GolesVisitante = request.GolesVisitante,
            Timestamp = DateTime.UtcNow
        };

        await _pronosticoRepository.GuardarPronosticoAsync(pronostico);
        return (MapToResponse(pronostico), null);
    }

    public async Task<(List<PronosticoResponse> Items, byte[]? SiguientePagina)> GetHistorialAsync(
        Guid usuarioId, int limit, byte[]? paginaToken)
    {
        if (limit < 1) limit = 10;

        var (items, siguienteToken) = await _pronosticoRepository
            .GetPronosticosPorUsuarioAsync(usuarioId, limit, paginaToken);

        return (items.Select(MapToResponse).ToList(), siguienteToken);
    }

    public async Task<List<RankingItemResponse>> GetRankingLigaAsync(Guid ligaId, int top = 10)
    {
        var puntos = await _pronosticoRepository.GetRankingLigaAsync(ligaId, top);
        return puntos
            .OrderByDescending(p => p.TotalPuntos)
            .Select(p => new RankingItemResponse(p.UsuarioId, string.Empty, p.TotalPuntos))
            .ToList();
    }

    public async Task<List<RankingItemResponse>> GetRankingGlobalAsync(int top = 10)
    {
        var puntos = await _pronosticoRepository.GetRankingGlobalAsync(top);
        return puntos
            .OrderByDescending(p => p.TotalPuntos)
            .Select(p => new RankingItemResponse(p.UsuarioId, string.Empty, p.TotalPuntos))
            .ToList();
    }

    public async Task<(bool Success, string? Error)> CalcularPuntosAsync(
        CalcularPuntosRequest request)
    {
        var pronosticos = await _pronosticoRepository
            .GetPronosticosPorPartidoAsync(request.PartidoId);

        if (pronosticos.Count == 0)
            return (true, null); // No hay pronósticos, no hay nada que calcular

        foreach (var pronostico in pronosticos)
        {
            var puntos = CalcularPuntos(
                pronostico.GolesLocal, pronostico.GolesVisitante,
                request.ResultadoLocal, request.ResultadoVisitante);

            if (puntos > 0)
                await _pronosticoRepository.ActualizarPuntosAsync(
                    request.LigaId, pronostico.UsuarioId, puntos);
        }

        return (true, null);
    }

    // 3 puntos si acertó el marcador exacto
    // 1 punto si acertó el ganador o el empate
    // 0 puntos si no acertó nada
    private static long CalcularPuntos(
        int golesLocalPronostico, int golesVisitantePronostico,
        int resultadoLocal, int resultadoVisitante)
    {
        if (golesLocalPronostico == resultadoLocal &&
            golesVisitantePronostico == resultadoVisitante)
            return 3;

        var ganadorPronostico = Math.Sign(golesLocalPronostico - golesVisitantePronostico);
        var ganadorReal = Math.Sign(resultadoLocal - resultadoVisitante);

        if (ganadorPronostico == ganadorReal)
            return 1;

        return 0;
    }

    private static PronosticoResponse MapToResponse(Pronostico p) =>
        new(p.PartidoId, p.UsuarioId, p.GolesLocal, p.GolesVisitante, p.Timestamp);
}