using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Controllers;

[ApiController]
[Route("api/productores")]
public sealed class ProductoresController : IntKeyCrudControllerBase<ProductorDto, CreateProductorDto, UpdateProductorDto>
{
    public ProductoresController(ICrudService<ProductorDto, CreateProductorDto, UpdateProductorDto> svc)
        : base(svc, p => p.ProductorId)
    {
    }
}
