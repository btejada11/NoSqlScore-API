using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Application.DTOs;

public record PartidoRequest(Guid LigaId, string Local, string Visitante, DateTime Fecha);

public record PartidoResponse(
    Guid Id,
    Guid LigaId,
    string Local,
    string Visitante,
    DateTime Fecha,
    int? ResultadoLocal,
    int? ResultadoVisitante,
    string Estado);

public record CerrarPartidoRequest(int ResultadoLocal, int ResultadoVisitante);

