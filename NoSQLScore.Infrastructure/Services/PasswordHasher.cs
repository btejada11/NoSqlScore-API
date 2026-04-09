using System;
using System.Collections.Generic;
using System.Text;
using NoSQLScore.Application.Interfaces;

namespace NoSQLScore.Infrastructure.Services;

/// <summary>
/// Implementación de IPasswordHasher usando BCrypt.Net-Next.
/// Solo Infrastructure conoce BCrypt. Application no.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
