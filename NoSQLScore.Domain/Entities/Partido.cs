using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Domain.Entities
{
    public class Partido
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LigaId { get; set; }
        public string Local { get; set; } = string.Empty;
        public string Visitante { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int? ResultadoLocal { get; set; }
        public int? ResultadoVisitante { get; set; }
        public string Estado { get; set; } = "abierto";
        public bool PuntosProcesados { get; set; } = false;
    }
}
