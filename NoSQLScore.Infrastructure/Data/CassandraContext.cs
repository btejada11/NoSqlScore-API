using Cassandra;
using Microsoft.Extensions.Configuration;

namespace NoSQLScore.Infrastructure.Data;

public class CassandraContext : IDisposable
{
    private readonly Cluster _cluster;
    public ISession Session { get; }

    public CassandraContext(IConfiguration configuration)
    {
        var contactPoints = configuration.GetSection("CassandraSettings:ContactPoints").Get<string[]>() ?? ["127.0.0.1"];
        var port = int.Parse(configuration["CassandraSettings:Port"] ?? "9042");
        var keyspace = configuration["CassandraSettings:Keyspace"] ?? "nosqlscore";
        var username = configuration["CassandraSettings:Username"] ?? "cassandra";
        var password = configuration["CassandraSettings:Password"] ?? "cassandra";

        _cluster = Cluster.Builder()
            .AddContactPoints(contactPoints)
            .WithPort(port)
            .WithCredentials(username, password)
            .WithLoadBalancingPolicy(new DCAwareRoundRobinPolicy())
            .Build();

        Session = _cluster.Connect();
        InicializarKeyspaceYTablas(keyspace);
    }

    private void InicializarKeyspaceYTablas(string keyspace)
    {
        // Crear keyspace si no existe (replication factor 3 para los 3 nodos)
        Session.Execute($@"
            CREATE KEYSPACE IF NOT EXISTS {keyspace}
            WITH replication = {{
                'class': 'SimpleStrategy',
                'replication_factor': 3
            }}");

        Session.ChangeKeyspace(keyspace);

        // Tabla de pronósticos
        Session.Execute(@"
            CREATE TABLE IF NOT EXISTS pronosticos (
                partido_id UUID,
                usuario_id UUID,
                goles_local INT,
                goles_visitante INT,
                timestamp TIMESTAMP,
                PRIMARY KEY ((partido_id), usuario_id)
            )");

        // Tabla de puntos por liga (counter)
        Session.Execute(@"
            CREATE TABLE IF NOT EXISTS puntos_liga (
                liga_id UUID,
                usuario_id UUID,
                total_puntos COUNTER,
                PRIMARY KEY ((liga_id), usuario_id)
            )");

        // Tabla de puntos globales (counter)
        Session.Execute(@"
            CREATE TABLE IF NOT EXISTS puntos_global (
                usuario_id UUID PRIMARY KEY,
                total_puntos COUNTER
            )");
    }

    public void Dispose()
    {
        Session.Dispose();
        _cluster.Dispose();
    }
}