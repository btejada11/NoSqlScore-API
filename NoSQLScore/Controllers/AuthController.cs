using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Services;
using LoginRequest = NoSQLScore.Application.DTOs.LoginRequest;


namespace NoSQLScore.API.Controllers;

/// <summary>
/// Maneja registro, login y gestión de usuarios.
/// Solo llama a AuthService (Application). No conoce MongoDB ni JWT.
/// </summary>
[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/registro
    [HttpPost("auth/registro")]
    [ProducesResponseType(typeof(LoginResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Registro([FromBody] RegistroRequest request)
    {
        var (response, error) = await _authService.RegistrarAsync(request);
        if (error != null) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(GetUsuario), new { id = response!.UsuarioId }, response);
    }

    // POST /api/auth/login
    [HttpPost("auth/login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (response, error) = await _authService.LoginAsync(request);
        if (error == "Credenciales inválidas.") return Unauthorized(new { mensaje = error });
        if (error != null) return BadRequest(new { mensaje = error });
        return Ok(response);
    }

    // GET /api/usuarios/{id}
    [HttpGet("usuarios/{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUsuario(Guid id)
    {
        var usuario = await _authService.GetUsuarioAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });
        return Ok(usuario);
    }

    // PUT /api/usuarios/{id}
    [HttpPut("usuarios/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioRequest request)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(usuarioIdClaim, out var solicitanteId))
            return Unauthorized(new { mensaje = "Token inválido." });

        var (response, error) = await _authService.ActualizarUsuarioAsync(id, solicitanteId, request);

        if (error == "No tienes permiso para modificar este usuario.")
            return StatusCode(403, new { mensaje = error });
        if (error == "Usuario no encontrado.")
            return NotFound(new { mensaje = error });
        if (error != null)
            return BadRequest(new { mensaje = error });

        return Ok(response);
    }

    // GET /api/usuarios — lista con paginación (requiere token)
    [HttpGet("usuarios")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResponse<UsuarioResponse>), 200)]
    public async Task<IActionResult> ListarUsuarios(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        var result = await _authService.ListarUsuariosAsync(page, limit);
        return Ok(result);
    }
}
