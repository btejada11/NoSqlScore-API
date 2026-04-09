using System;
using System.Collections.Generic;
using System.Text;

namespace NoSQLScore.Application.DTOs;

public record CrearLigaRequest(string Nombre, string? Descripcion);

public record LigaResponse(
    Guid Id,
    string Nombre,
    string? Descripcion,
    Guid CreadorId,
    List<Guid> Miembros,
    DateTime FechaCreacion);
