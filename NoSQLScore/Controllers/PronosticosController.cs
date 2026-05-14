using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Services;
using System.Security.Claims;

namespace NoSQLScore.API.Controllers;

[ApiController]
[Route("api")]
public class PronosticosController : ControllerBase
{
    private readonly PronosticoService _pronosticoService;

    public PronosticosController(PronosticoService pronosticoService)
    {
        _pronosticoService = pronosticoService;
    }

    // POST /api/pronosticos
    [HttpPost("pronosticos")]
    [Authorize]
    [ProducesResponseType(typeof(PronosticoResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CrearPronostico([FromBody] crearPronosticoRequest request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });

        var (response, error) = await _pronosticoService.CrearPronosticoAsync(request, usuarioId.Value);
        if (error != null) return BadRequest(new { mensaje = error });

        return Created($"/api/pronosticos", response);
    }

    // GET /api/usuarios/{id}/pronosticos
    [HttpGet("usuarios/{id:guid}/pronosticos")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetHistorial(
        Guid id,
        [FromQuery] int limit = 10,
        [FromQuery] string? token = null)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });

        // Convertir el token de string a bytes si viene en la query
        byte[]? paginaToken = token != null ? Convert.FromBase64String(token) : null;

        var (items, siguienteToken) = await _pronosticoService.GetHistorialAsync(id, limit, paginaToken);

        return Ok(new
        {
            data = items,
            siguienteToken = siguienteToken != null ? Convert.ToBase64String(siguienteToken) : null,
            limit
        });
    }

    // GET /api/ranking/liga/{ligaId}
    [HttpGet("ranking/liga/{ligaId:guid}")]
    [ProducesResponseType(typeof(List<RankingItemResponse>), 200)]
    public async Task<IActionResult> GetRankingLiga(Guid ligaId)
    {
        var ranking = await _pronosticoService.GetRankingLigaAsync(ligaId);
        return Ok(ranking);
    }

    // GET /api/ranking/global
    [HttpGet("ranking/global")]
    [ProducesResponseType(typeof(List<RankingItemResponse>), 200)]
    public async Task<IActionResult> GetRankingGlobal()
    {
        var ranking = await _pronosticoService.GetRankingGlobalAsync();
        return Ok(ranking);
    }

    // POST /api/calcular-puntos (solo para el Módulo 3)
    [HttpPost("calcular-puntos")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CalcularPuntos([FromBody] CalcularPuntosRequest request)
    {
        var (success, error) = await _pronosticoService.CalcularPuntosAsync(request);
        if (!success) return BadRequest(new { mensaje = error });
        return Ok(new { mensaje = "Puntos calculados correctamente." });
    }

    private Guid? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}