using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Services;
using System.Security.Claims;

namespace NoSQLScore.API.Controllers;

[ApiController]
[Route("api/partidos")]
public class PartidosController : ControllerBase
{
    private readonly PartidoService _partidoService;

    public PartidosController(PartidoService partidoService)
    {
        _partidoService = partidoService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrearPartido([FromBody] PartidoRequest request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });

        var (response, error, statusCode) = await _partidoService.CrearPartidoAsync(request, usuarioId.Value);

        return statusCode switch
        {
            201 => CreatedAtAction(nameof(GetPartido), new { id = response!.Id }, response),
            403 => StatusCode(403, new { mensaje = error }),
            404 => NotFound(new { mensaje = error }),
            _ => BadRequest(new { mensaje = error })
        };
    }

    [HttpGet]
    public async Task<IActionResult> ListarPartidos(
        [FromQuery] Guid? ligaId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var result = await _partidoService.ListarPartidosAsync(ligaId, estado, page, limit);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPartido(Guid id)
    {
        var partido = await _partidoService.GetPartidoAsync(id);
        if (partido == null) return NotFound(new { mensaje = "Partido no encontrado." });
        return Ok(partido);
    }

    [HttpPut("{id:guid}/resultado")]
    [Authorize]
    public async Task<IActionResult> CerrarPartido(Guid id, [FromBody] CerrarPartidoRequest request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });

        var (response, error, statusCode) = await _partidoService.CerrarPartidoAsync(id, usuarioId.Value, request);

        return statusCode switch
        {
            200 => Ok(response),
            403 => StatusCode(403, new { mensaje = error }),
            404 => NotFound(new { mensaje = error }),
            _ => BadRequest(new { mensaje = error })
        };
    }

    private Guid? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
