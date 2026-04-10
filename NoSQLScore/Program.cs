using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using NoSQLScore.Application.Interfaces;
using NoSQLScore.Application.Services;
using NoSQLScore.Infrastructure.Data;
using NoSQLScore.Infrastructure.Repositories;
using NoSQLScore.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region ================== Infrastructure ==================

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ILigaRepository, LigaRepository>();
builder.Services.AddScoped<IPartidoRepository, PartidoRepository>();

builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

#endregion

#region ================== Application ==================

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LigaService>();
builder.Services.AddScoped<PartidoService>();

#endregion

#region ================== JWT Authentication ==================

var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey es requerido.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

#endregion

#region ================== Controllers & Swagger ==================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NoSQL-Score API",
        Version = "v1",
        Description = "API REST para el proyecto NoSQL-Score — usuarios, ligas y partidos con MongoDB."
    });

    // ✅ Definición JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT. Ejemplo: Bearer {token}"
    });

    // ✅ Requerimiento JWT
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

var app = builder.Build();

#region ================== Middleware ==================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NoSQL-Score API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
