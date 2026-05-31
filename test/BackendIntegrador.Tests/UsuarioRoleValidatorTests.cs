using System;
using BackendIntegrador.Application.Common;
using BackendIntegrador.Application.Dtos;
using BackendIntegrador.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BackendIntegrador.Tests;

public class UsuarioRoleValidatorTests
{
    private static Rol Rol(string nombre) => new() { RolId = 1, Nombre = nombre };

    [Fact]
    public void ValidateProvision_ProductorWithCentroAcopio_Throws()
    {
        var act = () => UsuarioRoleValidator.ValidateProvision(
            Rol(UsuarioRoleTypes.RolNombreProductor),
            centroAcopioId: 1,
            productor: new ProductorProvisionDto("Juan", "123", null, 1, null));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Centro de Acopio*");
    }

    [Fact]
    public void ValidateProvision_TrabajadorWithProductor_Throws()
    {
        var act = () => UsuarioRoleValidator.ValidateProvision(
            Rol(UsuarioRoleTypes.RolNombreTrabajadorCentroAcopio),
            centroAcopioId: 1,
            productor: new ProductorProvisionDto("Juan", "123", null, 1, null));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*productor*");
    }

    [Fact]
    public void ValidateProvision_CentroAcopioWithoutCentro_Throws()
    {
        var act = () => UsuarioRoleValidator.ValidateProvision(
            Rol(UsuarioRoleTypes.RolNombreCentroAcopio),
            centroAcopioId: null,
            productor: null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Centro de Acopio*");
    }

    [Fact]
    public void ValidateProvision_ProductorWithoutDatos_Throws()
    {
        var act = () => UsuarioRoleValidator.ValidateProvision(
            Rol(UsuarioRoleTypes.RolNombreProductor),
            centroAcopioId: null,
            productor: null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*datos de productor*");
    }

    [Fact]
    public void ValidateProvision_AdministradorValido_ReturnsTipo()
    {
        var tipo = UsuarioRoleValidator.ValidateProvision(
            Rol(UsuarioRoleTypes.RolNombreAdministrador),
            centroAcopioId: null,
            productor: null);

        tipo.Should().Be(UsuarioRoleTypes.Administrador);
    }

    [Fact]
    public void ValidateUpdate_ProductorToTrabajadorWhenHadProductor_Throws()
    {
        var act = () => UsuarioRoleValidator.ValidateUpdate(
            Rol(UsuarioRoleTypes.RolNombreTrabajadorCentroAcopio),
            centroAcopioId: 1,
            productor: null,
            hadProductorRecord: true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*registro de productor*");
    }
}
