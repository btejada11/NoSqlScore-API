using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSQLScore.Domain.Entities
{
    public class Pronostico
    {
        public  Guid PartidoId { get; set; }
        public Guid UsuarioId { get; set; }
        public int GolesLocal { get; set; }
        public int GolesVisitante { get; set; }
        public DateTime Timestamp { get; set; }= DateTime.UtcNow;
    }
}
