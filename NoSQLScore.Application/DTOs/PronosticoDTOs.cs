using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSQLScore.Application.DTOs
{
   public record crearPronosticoRequest(Guid PartidoId, int GolesLocal, int GolesVisitante);
   public record PronosticoResponse(Guid Id, Guid UsuarioId, int GolesLocal, int GolesVisitante, DateTime Timestamp);
   public record RankingItemResponse(Guid UsuarioId, string UsuarioNombre, long Puntos);
   public record CalcularPuntosRequest(Guid PartidoId, Guid LigaId, int ResultadoLocal, int ResultadoVisitante);

}
