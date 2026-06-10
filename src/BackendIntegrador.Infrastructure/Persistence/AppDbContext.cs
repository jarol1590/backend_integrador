using BackendIntegrador.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendIntegrador.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<CentroAcopio> CentrosAcopio => Set<CentroAcopio>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Productor> Productores => Set<Productor>();
    public DbSet<Trabajador> Trabajadores => Set<Trabajador>();
    public DbSet<Finca> Fincas => Set<Finca>();
    public DbSet<Ordeno> Ordenos => Set<Ordeno>();
    public DbSet<Transporte> Transportes => Set<Transporte>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<RecepcionAcopio> RecepcionesAcopio => Set<RecepcionAcopio>();
    public DbSet<Muestra> Muestras => Set<Muestra>();
    public DbSet<AnalisisCalidad> AnalisisCalidad => Set<AnalisisCalidad>();
    public DbSet<ParametroCalidad> ParametrosCalidad => Set<ParametroCalidad>();
    public DbSet<ResultadoParametro> ResultadosParametro => Set<ResultadoParametro>();
    public DbSet<FincaGemeloEstado> FincasGemeloEstado => Set<FincaGemeloEstado>();
    public DbSet<LecturaClimatica> LecturasClimaticas => Set<LecturaClimatica>();
    public DbSet<PrediccionGemelo> PrediccionesGemelo => Set<PrediccionGemelo>();
    public DbSet<AlertaGemelo> AlertasGemelo => Set<AlertaGemelo>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioRol>(e =>
        {
            e.HasKey(ur => new { ur.UsuarioId, ur.RolId });
            e.HasOne(ur => ur.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(ur => ur.UsuarioId);
            e.HasOne(ur => ur.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(ur => ur.RolId);
        });

        modelBuilder.Entity<ResultadoParametro>(e =>
        {
            e.HasKey(r => new { r.AnalisisId, r.ParametroId });
            e.Property(r => r.ValorResultado).HasPrecision(18, 6);
            e.HasOne(r => r.Analisis).WithMany(a => a.Resultados).HasForeignKey(r => r.AnalisisId);
            e.HasOne(r => r.Parametro).WithMany(p => p.Resultados).HasForeignKey(r => r.ParametroId);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.PasswordHash).HasMaxLength(512);
            e.Property(u => u.Estado).HasMaxLength(64);
            e.HasOne(u => u.CentroAcopio).WithMany(c => c.Usuarios).HasForeignKey(u => u.CentroAcopioId).IsRequired(false);
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.HasIndex(r => r.Nombre).IsUnique();
            e.Property(r => r.Nombre).HasMaxLength(128);
        });

        modelBuilder.Entity<Departamento>(e =>
        {
            e.HasIndex(d => d.Nombre).IsUnique();
            e.Property(d => d.Nombre).HasMaxLength(128);
        });

        modelBuilder.Entity<Municipio>(e =>
        {
            e.Property(m => m.Nombre).HasMaxLength(128);
            e.HasOne(m => m.Departamento).WithMany(d => d.Municipios).HasForeignKey(m => m.DepartamentoId);
        });

        modelBuilder.Entity<CentroAcopio>(e =>
        {
            e.Property(c => c.Nombre).HasMaxLength(256);
            e.Property(c => c.Latitud).HasPrecision(18, 8);
            e.Property(c => c.Longitud).HasPrecision(18, 8);
            e.HasOne(c => c.Municipio).WithMany(m => m.CentrosAcopio).HasForeignKey(c => c.MunicipioId);
        });

        modelBuilder.Entity<TipoDocumento>(e =>
        {
            e.HasIndex(t => t.Nombre).IsUnique();
            e.Property(t => t.Nombre).HasMaxLength(64);
        });

        modelBuilder.Entity<Productor>(e =>
        {
            e.HasIndex(p => p.Documento).IsUnique();
            e.HasIndex(p => p.UsuarioId).IsUnique();
            e.Property(p => p.Nombre).HasMaxLength(256);
            e.Property(p => p.Documento).HasMaxLength(64);
            e.HasOne(p => p.Usuario).WithOne(u => u.Productor).HasForeignKey<Productor>(p => p.UsuarioId);
            e.HasOne(p => p.TipoDocumento).WithMany(t => t.Productores).HasForeignKey(p => p.TipoDocumentoId);
        });

        modelBuilder.Entity<Trabajador>(e =>
        {
            e.HasIndex(t => t.Documento).IsUnique();
            e.HasIndex(t => t.UsuarioId).IsUnique();
            e.Property(t => t.Nombre).HasMaxLength(256);
            e.Property(t => t.Documento).HasMaxLength(64);
            e.HasOne(t => t.Usuario).WithOne(u => u.Trabajador).HasForeignKey<Trabajador>(t => t.UsuarioId);
            e.HasOne(t => t.TipoDocumento).WithMany(td => td.Trabajadores).HasForeignKey(t => t.TipoDocumentoId);
        });

        modelBuilder.Entity<Finca>(e =>
        {
            e.Property(f => f.Nombre).HasMaxLength(256);
            e.Property(f => f.Latitud).HasPrecision(18, 8);
            e.Property(f => f.Longitud).HasPrecision(18, 8);
            e.HasOne(f => f.Productor).WithMany(p => p.Fincas).HasForeignKey(f => f.ProductorId);
            e.HasOne(f => f.Municipio).WithMany(m => m.Fincas).HasForeignKey(f => f.MunicipioId);
        });

        modelBuilder.Entity<Ordeno>(e =>
        {
            e.Property(o => o.Codigo).HasMaxLength(64);
            e.HasIndex(o => o.Codigo).IsUnique();
            e.Property(o => o.VolumenLitros).HasPrecision(18, 4);
            e.HasOne(o => o.Finca).WithMany(f => f.Ordenos).HasForeignKey(o => o.FincaId);
        });

        modelBuilder.Entity<Transporte>(e =>
        {
            e.Property(t => t.PlacaVehiculo).HasMaxLength(32);
            e.HasMany(t => t.Lotes).WithOne(l => l.Transporte).HasForeignKey(l => l.TransporteId).IsRequired(false);
        });

        modelBuilder.Entity<Lote>(e =>
        {
            e.Property(l => l.Codigo).HasMaxLength(64);
            e.HasIndex(l => l.Codigo).IsUnique();
            e.Property(l => l.VolumenCapturadoLitros).HasPrecision(18, 4);
            e.HasOne(l => l.Ordeno).WithMany(o => o.Lotes).HasForeignKey(l => l.OrdenoId);
            e.HasOne(l => l.CentroAcopio).WithMany(c => c.Lotes).HasForeignKey(l => l.CentroAcopioId).IsRequired(false);
        });

        modelBuilder.Entity<RecepcionAcopio>(e =>
        {
            e.Property(r => r.VolumenLitrosRecibidos).HasPrecision(18, 4);
            e.HasOne(r => r.Transporte).WithMany(t => t.Recepciones).HasForeignKey(r => r.TransporteId);
            e.HasOne(r => r.CentroAcopio).WithMany(c => c.Recepciones).HasForeignKey(r => r.CentroAcopioId);
            e.HasOne(r => r.Usuario).WithMany(u => u.RecepcionesRegistradas).HasForeignKey(r => r.UsuarioId);
        });

        modelBuilder.Entity<Muestra>(e =>
        {
            e.HasOne(m => m.Lote).WithMany(l => l.Muestras).HasForeignKey(m => m.LoteId);
            e.HasOne(m => m.TecnicoUsuario).WithMany(u => u.MuestrasComoTecnico).HasForeignKey(m => m.TecnicoPorUsuarioId);
        });

        modelBuilder.Entity<AnalisisCalidad>(e =>
        {
            e.HasOne(a => a.Muestra).WithMany(m => m.Analisis).HasForeignKey(a => a.MuestraId);
        });

        modelBuilder.Entity<ParametroCalidad>(e =>
        {
            e.HasIndex(p => new { p.CentroAcopioId, p.Nombre }).IsUnique();
            e.Property(p => p.Nombre).HasMaxLength(128);
            e.Property(p => p.ValorMinimo).HasPrecision(18, 6);
            e.Property(p => p.ValorMaximo).HasPrecision(18, 6);
            e.Property(p => p.Descripcion).HasMaxLength(512);
            e.HasOne(p => p.CentroAcopio).WithMany().HasForeignKey(p => p.CentroAcopioId).IsRequired(false);
        });

        modelBuilder.Entity<FincaGemeloEstado>(e =>
        {
            e.HasKey(g => g.FincaId);
            e.Property(g => g.VersionMotor).HasMaxLength(32);
            e.Property(g => g.FuenteClima).HasMaxLength(64);
            e.Property(g => g.EstadoSync).HasMaxLength(32);
            e.Property(g => g.UltimoError).HasMaxLength(1024);
            e.HasOne(g => g.Finca).WithOne(f => f.GemeloEstado).HasForeignKey<FincaGemeloEstado>(g => g.FincaId);
        });

        modelBuilder.Entity<LecturaClimatica>(e =>
        {
            e.HasKey(l => l.LecturaId);
            e.HasIndex(l => new { l.FincaId, l.Fecha }).IsUnique();
            e.HasIndex(l => l.Fecha);
            e.Property(l => l.TempMin).HasPrecision(8, 2);
            e.Property(l => l.TempMax).HasPrecision(8, 2);
            e.Property(l => l.TempMedia).HasPrecision(8, 2);
            e.Property(l => l.HumedadMedia).HasPrecision(8, 2);
            e.Property(l => l.PrecipitacionMm).HasPrecision(8, 2);
            e.Property(l => l.ThiMax).HasPrecision(8, 2);
            e.Property(l => l.Fuente).HasMaxLength(64);
            e.HasOne(l => l.Finca).WithMany(f => f.LecturasClimaticas).HasForeignKey(l => l.FincaId);
        });

        modelBuilder.Entity<PrediccionGemelo>(e =>
        {
            e.HasKey(p => p.PrediccionId);
            e.HasIndex(p => new { p.FincaId, p.TipoPrediccion, p.HorizonteDias });
            e.HasIndex(p => p.GeneradaUtc);
            e.Property(p => p.TipoPrediccion).HasMaxLength(64);
            e.Property(p => p.Valor).HasPrecision(18, 4);
            e.Property(p => p.Confianza).HasPrecision(4, 3);
            e.Property(p => p.Unidad).HasMaxLength(32);
            e.HasOne(p => p.Finca).WithMany(f => f.PrediccionesGemelo).HasForeignKey(p => p.FincaId);
        });

        modelBuilder.Entity<AlertaGemelo>(e =>
        {
            e.HasKey(a => a.AlertaId);
            e.HasIndex(a => new { a.FincaId, a.Leida, a.CreadaUtc });
            e.HasIndex(a => a.ExpiraUtc);
            e.Property(a => a.TipoAlerta).HasMaxLength(64);
            e.Property(a => a.Severidad).HasMaxLength(16);
            e.Property(a => a.Titulo).HasMaxLength(256);
            e.Property(a => a.Mensaje).HasMaxLength(1024);
            e.Property(a => a.Recomendacion).HasMaxLength(1024);
            e.HasOne(a => a.Finca).WithMany(f => f.AlertasGemelo).HasForeignKey(a => a.FincaId);
        });

        modelBuilder.Entity<DeviceToken>(e =>
        {
            e.HasKey(dt => dt.Id);
            e.HasIndex(dt => new { dt.UsuarioId, dt.Token }).IsUnique();
            e.Property(dt => dt.Token).HasMaxLength(512);
            e.Property(dt => dt.Platform).HasMaxLength(16);
            e.HasOne(dt => dt.Usuario).WithMany().HasForeignKey(dt => dt.UsuarioId);
        });

        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
