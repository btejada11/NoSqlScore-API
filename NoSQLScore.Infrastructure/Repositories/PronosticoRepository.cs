using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassandra;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Domain.Entities;
using NoSQLScore.Infrastructure.Data;

namespace NoSQLScore.Infrastructure.Repositories
{
    public class PronosticoRepository : IPronosticoRepository
    {

        private readonly ISession _session;
        private readonly PreparedStatement _guardarPronostico;
        private readonly PreparedStatement _buscarPorPartido;
        private readonly PreparedStatement _sumarPuntosLiga;
        private readonly PreparedStatement _sumarPuntosGlobal;
        private readonly PreparedStatement _topLiga;
        private readonly PreparedStatement _topGlobal;
        private readonly PreparedStatement _historialUsuario;

        public PronosticoRepository(CassandraContext context)
        {
            _session = context.Session;
            _guardarPronostico= _session.Prepare(@"Insert into pronosticos (partido_id, usuario_id, goles_local, goles_visitante, timestamp) values (?, ?, ?, ?, ?)");
            _buscarPorPartido = _session.Prepare(@"Select partido_id, usuario_id, goles_local, goles_visitante, timestamp from pronosticos where partido_id = ?");
            _historialUsuario = _session.Prepare(@"Select partido_id, usuario_id, goles_local, goles_visitante, timestamp from pronosticos where usuario_id = ? limit ?");
            _sumarPuntosLiga = _session.Prepare(@"Update puntos_liga set total_puntos = total_puntos + ? where liga_id = ? and usuario_id = ?");
            _sumarPuntosGlobal = _session.Prepare(@"Update puntos_global set total_puntos = total_puntos + ? where usuario_id = ?");
            _topLiga = _session.Prepare(@"Select liga_id, usuario_id, total_puntos from puntos_liga where liga_id = ? order by total_puntos desc limit ?");
            _topGlobal = _session.Prepare(@"Select usuario_id, total_puntos from puntos_global order by total_puntos desc limit ?");
        }
        public async Task ActualizarPuntosAsync(Guid ligaId, Guid usuarioId, long puntos)
        {
            var ligaQuery = _sumarPuntosLiga.Bind(puntos, ligaId, usuarioId);
            await _session.ExecuteAsync(ligaQuery);

            var globalQuery = _sumarPuntosGlobal.Bind(puntos, usuarioId);
            await _session.ExecuteAsync(globalQuery);
        }

        public async Task<List<Pronostico>> GetPronosticosPorPartidoAsync(Guid partidoId)
        {
            var consulta = _buscarPorPartido.Bind(partidoId);
            var resultado = await _session.ExecuteAsync(consulta);
            return resultado.Select(ConvertirFila).ToList();
        }

        public async Task<(List<Pronostico> Items, byte[]? PaginaToken)> GetPronosticosPorUsuarioAsync(Guid usuarioId, int limit, byte[]? paginaToken)
        {
            var consulta = new SimpleStatement("Select partido_id, usuario_id, goles_local, goles_visitante, timestamp from pronosticos").SetPageSize(limit);
            if (paginaToken != null)
                consulta.SetPagingState(paginaToken);
            var resultado = await _session.ExecuteAsync(consulta);
            var items= resultado
                .Where(Row => Row.GetValue<Guid>("usuario_id") == usuarioId)
                .Select(ConvertirFila).ToList();
            return (items, resultado.PagingState);
        }

        public async Task<List<PuntosGlobal>> GetRankingGlobalAsync(int top)
        {
            var resultado= await _session.ExecuteAsync(_topGlobal.Bind());
            return resultado.Select(row => new PuntosGlobal
            {
                UsuarioId = row.GetValue<Guid>("usuario_id"),
                TotalPuntos = row.GetValue<long>("total_puntos")
            }).OrderByDescending(p => p.TotalPuntos).Take(top).ToList();
        }

        public async Task<List<PuntosLiga>> GetRankingLigaAsync(Guid ligaId, int top)
        {
            var consulta= _topLiga.Bind(ligaId);
            var resultado= await _session.ExecuteAsync(consulta);

            return resultado.Select(row=> new PuntosLiga
            {
                LigaId=row.GetValue<Guid>("liga_id"),
                UsuarioId = row.GetValue<Guid>("usuario_id"),
                TotalPuntos = row.GetValue<long>("total_puntos")
            }).OrderByDescending(p=>p.TotalPuntos).Take(top).ToList();
        }

        public async Task GuardarPronosticoAsync(Pronostico pronostico)
        {
            var consulta = _guardarPronostico.Bind(
                pronostico.PartidoId, 
                pronostico.UsuarioId, 
                pronostico.GolesLocal, 
                pronostico.GolesVisitante, 
                pronostico.Timestamp);
            await _session.ExecuteAsync(consulta);
        }
        private static Pronostico ConvertirFila(Row fila) => new()
        {
            PartidoId = fila.GetValue<Guid>("partido_id"),
            UsuarioId = fila.GetValue<Guid>("usuario_id"),
            GolesLocal = fila.GetValue<int>("goles_local"),
            GolesVisitante = fila.GetValue<int>("goles_visitante"),
            Timestamp = fila.GetValue<DateTime>("timestamp")
        };
    }
}
