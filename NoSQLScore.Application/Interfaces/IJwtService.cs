using System;
using System.Collections.Generic;
using System.Text;
namespace NoSQLScore.Application.Interfaces;

/// <summary>
/// Contrato para la generación y validación de tokens JWT.
/// Application define el contrato; Infrastructure lo implementa.
/// </summary>
public interface IJwtService
{
    string GenerarToken(Guid usuarioId, string nombre, string email);
}

