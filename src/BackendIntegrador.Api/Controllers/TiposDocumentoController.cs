using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/tipos-documento")]
public sealed class TiposDocumentoController : IntKeyCrudControllerBase<TipoDocumentoDto, CreateTipoDocumentoDto, UpdateTipoDocumentoDto>
{
    public TiposDocumentoController(ICrudService<TipoDocumentoDto, CreateTipoDocumentoDto, UpdateTipoDocumentoDto> svc)
        : base(svc, t => t.TipoDocumentoId)
    {
    }
}
