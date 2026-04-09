using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Application.DTOs;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Services;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUsuarioRepository usuarioRepo,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepo = usuarioRepo;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<(LoginResponse? Response, string? Error)> RegistrarAsync(RegistroRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return (null, "Nombre, Email y Password son requeridos.");

        var existe = await _usuarioRepo.GetByEmailAsync(request.Email);
        if (existe != null)
            return (null, "El email ya está registrado.");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FechaRegistro = DateTime.UtcNow
        };

        await _usuarioRepo.CreateAsync(usuario);

        var token = _jwtService.GenerarToken(usuario.Id, usuario.Nombre, usuario.Email);
        return (new LoginResponse(token, usuario.Id, usuario.Nombre), null);
    }

    public async Task<(LoginResponse? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (null, "Email y Password son requeridos.");

        var usuario = await _usuarioRepo.GetByEmailAsync(request.Email);
        if (usuario == null || !_passwordHasher.Verify(request.Password, usuario.PasswordHash))
            return (null, "Credenciales inválidas.");

        var token = _jwtService.GenerarToken(usuario.Id, usuario.Nombre, usuario.Email);
        return (new LoginResponse(token, usuario.Id, usuario.Nombre), null);
    }

    public async Task<UsuarioResponse?> GetUsuarioAsync(Guid id)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(id);
        if (usuario == null) return null;
        return MapToResponse(usuario);
    }

    public async Task<(UsuarioResponse? Response, string? Error)> ActualizarUsuarioAsync(
        Guid id, Guid solicitanteId, ActualizarUsuarioRequest request)
    {
        if (solicitanteId != id)
            return (null, "No tienes permiso para modificar este usuario.");

        var usuario = await _usuarioRepo.GetByIdAsync(id);
        if (usuario == null)
            return (null, "Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(request.Nombre) && request.Avatar == null)
            return (null, "No se proporcionaron campos para actualizar.");

        await _usuarioRepo.UpdateAsync(id, request.Nombre, request.Avatar);

        var actualizado = await _usuarioRepo.GetByIdAsync(id);
        return (MapToResponse(actualizado!), null);
    }

    public async Task<PaginatedResponse<UsuarioResponse>> ListarUsuariosAsync(int page, int limit)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 10;

        var (items, total) = await _usuarioRepo.GetAllAsync(page, limit);
        return new PaginatedResponse<UsuarioResponse>(
            items.Select(MapToResponse).ToList(), total, page, limit);
    }

    private static UsuarioResponse MapToResponse(Usuario u) =>
        new(u.Id, u.Nombre, u.Email, u.Avatar, u.FechaRegistro);
}