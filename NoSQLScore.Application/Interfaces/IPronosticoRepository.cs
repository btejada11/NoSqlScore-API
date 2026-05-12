using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Application.Interfaces
{
    public interface IPronosticoRepository
    {
        Task GuardarPronosticoAsync(Pronostico pronostico);
        Task<(List<Pronostico> Items, byte[]? PaginaToken)> GetPronosticosPorUsuarioAsync(
            Guid usuarioId, int limit, byte[]? paginaToken);
        Task<List<Pronostico>> GetPronosticosPorPartidoAsync(Guid partidoId);
        Task<List<PuntosLiga>> GetRankingLigaAsync(Guid ligaId, int top);
        Task<List<PuntosGlobal>> GetRankingGlobalAsync(int top);
        Task ActualizarPuntosAsync(Guid ligaId, Guid usuarioId, long puntos);
    }
}
