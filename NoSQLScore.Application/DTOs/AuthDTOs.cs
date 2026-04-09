using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Application.DTOs;

    public record RegistroRequest(string Nombre, string Email, string Password);

    public record LoginRequest(string Email, string Password);

    public record LoginResponse(string Token, Guid UsuarioId, string Nombre);

    public record UsuarioResponse(Guid Id, string Nombre, string Email, string? Avatar, DateTime FechaRegistro);

    public record ActualizarUsuarioRequest(string? Nombre, string? Avatar);

    public record PaginatedResponse<T>(List<T> Data, long Total, int Page, int Limit);
