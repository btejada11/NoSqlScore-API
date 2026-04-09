using System;
using System.Collections.Generic;
using System.Text;
namespace NoSQLScore.Application.Interfaces;

/// <summary>
/// Contrato para el hashing y verificación de contraseñas.
/// Application define el contrato; Infrastructure lo implementa con BCrypt.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
