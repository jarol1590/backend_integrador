using BackendIntegrador.Application.Abstractions;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Infrastructure.Persistence;
using BackendIntegrador.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackendIntegrador.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=integrador.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddScoped<ICrudService<UsuarioDto, CreateUsuarioDto, UpdateUsuarioDto>, UsuarioCrudService>();
        services.AddScoped<ICrudService<RolDto, CreateRolDto, UpdateRolDto>, RolCrudService>();
        services.AddScoped<ICrudService<DepartamentoDto, CreateDepartamentoDto, UpdateDepartamentoDto>, DepartamentoCrudService>();
        services.AddScoped<ICrudService<MunicipioDto, CreateMunicipioDto, UpdateMunicipioDto>, MunicipioCrudService>();
        services.AddScoped<ICrudService<CentroAcopioDto, CreateCentroAcopioDto, UpdateCentroAcopioDto>, CentroAcopioCrudService>();
        services.AddScoped<ICrudService<TipoDocumentoDto, CreateTipoDocumentoDto, UpdateTipoDocumentoDto>, TipoDocumentoCrudService>();
        services.AddScoped<ICrudService<ProductorDto, CreateProductorDto, UpdateProductorDto>, ProductorCrudService>();
        services.AddScoped<ICrudService<FincaDto, CreateFincaDto, UpdateFincaDto>, FincaCrudService>();
        services.AddScoped<ICrudService<OrdenoDto, CreateOrdenoDto, UpdateOrdenoDto>, OrdenoCrudService>();
        services.AddScoped<ICrudService<TransporteDto, CreateTransporteDto, UpdateTransporteDto>, TransporteCrudService>();
        services.AddScoped<ICrudService<LoteDto, CreateLoteDto, UpdateLoteDto>, LoteCrudService>();
        services.AddScoped<ICrudService<RecepcionAcopioDto, CreateRecepcionAcopioDto, UpdateRecepcionAcopioDto>, RecepcionAcopioCrudService>();
        services.AddScoped<ICrudService<MuestraDto, CreateMuestraDto, UpdateMuestraDto>, MuestraCrudService>();
        services.AddScoped<ICrudService<AnalisisCalidadDto, CreateAnalisisCalidadDto, UpdateAnalisisCalidadDto>, AnalisisCalidadCrudService>();
        services.AddScoped<ICrudService<ParametroCalidadDto, CreateParametroCalidadDto, UpdateParametroCalidadDto>, ParametroCalidadCrudService>();

        services.AddScoped<IUsuarioRolService, UsuarioRolService>();
        services.AddScoped<IResultadoParametroService, ResultadoParametroService>();

        return services;
    }
}
