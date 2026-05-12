using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSQLScore.Domain.Entities
{
    public class PuntosGlobal
    {
        public Guid UsuarioId { get; set; }
        public long TotalPuntos { get; set; }
    }
}
