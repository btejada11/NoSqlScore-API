using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Services;
using System.Security.Claims;
namespace NoSQLScore.API.Controllers;

[ApiController]
[Route("api/ligas")]
public class LigasController : ControllerBase
{
    private readonly LigaService _ligaService;
    public LigasController(LigaService ligaService)
    {
        _ligaService = ligaService;
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CrearLiga([FromBody] CrearLigaRequest request)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });
        var (response, error) = await _ligaService.CrearLigaAsync(request, usuarioId.Value);
        if (error != null) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(GetMiembros), new { id = response!.Id }, response);
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLigasDelUsuario(
        [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });
        var result = await _ligaService.GetLigasDelUsuarioAsync(usuarioId.Value, page, limit);
        return Ok(result);
    }
    [HttpPost("{id:guid}/miembros")]
    [Authorize]
    public async Task<IActionResult> UnirseALiga(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });
        var (success, error) = await _ligaService.UnirseAsync(id, usuarioId.Value);
        if (!success && error == "Liga no encontrada.")
            return NotFound(new { mensaje = error });
        if (!success)
            return BadRequest(new { mensaje = error });
        return Ok(new { mensaje = "Te has unido a la liga correctamente." });
    }
    [HttpDelete("{id:guid}/miembros")]
    [Authorize]
    public async Task<IActionResult> SalirDeLiga(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized(new { mensaje = "Token inválido." });
        var (success, error) = await _ligaService.SalirAsync(id, usuarioId.Value);
        if (!success && error == "Liga no encontrada.")
            return NotFound(new { mensaje = error });
        if (!success)
            return BadRequest(new { mensaje = error });
        return Ok(new { mensaje = "Has salido de la liga." });
    }
    [HttpGet("{id:guid}/miembros")]
    public async Task<IActionResult> GetMiembros(Guid id)
    {
        var (miembros, error) = await _ligaService.GetMiembrosAsync(id);
        if (miembros == null) return NotFound(new { mensaje = error });
        return Ok(miembros);
    }
    [HttpGet("{id:guid}/es-admin")]
    public async Task<IActionResult> EsAdmin(Guid id, [FromQuery] Guid usuarioId)
    {
        var (esAdmin, error) = await _ligaService.EsAdminAsync(id, usuarioId);
        if (esAdmin == null) return NotFound(new { mensaje = error });
        return Ok(new { esAdmin });
    }
    private Guid? ObtenerUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
