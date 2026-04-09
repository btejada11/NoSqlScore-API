using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Domain.Entities
{
    public class Liga
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public Guid CreadorId { get; set; }
        public List<Guid> Miembros { get; set; } = new();
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
