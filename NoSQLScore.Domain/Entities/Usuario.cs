using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
