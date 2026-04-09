using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NoSQLScore.Domain.Entities;

namespace NoSQLScore.Infrastructure.Data;

/// <summary>
/// Acceso a las colecciones MongoDB.
/// Centraliza la configuración del cliente y la creación de índices.
/// Solo Infrastructure conoce esta clase.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        // Serializar Guid como string en MongoDB
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
    }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDBSettings:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDBSettings:ConnectionString no está configurado.");
        var databaseName = configuration["MongoDBSettings:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDBSettings:DatabaseName no está configurado.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);

        CrearIndices();
    }

    public IMongoCollection<Usuario> Usuarios =>
        _database.GetCollection<Usuario>("Usuarios");

    public IMongoCollection<Liga> Ligas =>
        _database.GetCollection<Liga>("Ligas");

    public IMongoCollection<Partido> Partidos =>
        _database.GetCollection<Partido>("Partidos");

    private void CrearIndices()
    {
        var emailIndex = new CreateIndexModel<Usuario>(
            Builders<Usuario>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "idx_email_unico" });

        Usuarios.Indexes.CreateOne(emailIndex);
    }
}

