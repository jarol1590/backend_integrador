# BackendIntegrador Integration Tests

Este proyecto contiene las pruebas de integración para la API de `BackendIntegrador`, usando `xUnit` y `Microsoft.AspNetCore.Mvc.Testing`.

## Objetivo

Las pruebas verifican el comportamiento de los endpoints HTTP reales contra un servidor de prueba en memoria, incluido el acceso a PostgreSQL efímero (Testcontainers) y la aplicación de migraciones.

## Ejecutar todas las pruebas

Abre un terminal en la raíz del repositorio `backend_integrador` y ejecuta:

```bash
dotnet test test/BackendIntegrador.IntegrationTests/BackendIntegrador.IntegrationTests.csproj
```

## Ejecutar una clase o filtro específico

Ejemplo para ejecutar solo la clase `CrudIntegrationTests`:

```bash
dotnet test test/BackendIntegrador.IntegrationTests/BackendIntegrador.IntegrationTests.csproj --filter FullyQualifiedName~CrudIntegrationTests
```

Si deseas ejecutar solo un método específico:

```bash
dotnet test test/BackendIntegrador.IntegrationTests/BackendIntegrador.IntegrationTests.csproj --filter FullyQualifiedName~CreateFinca_ReturnsCreated
```

## Requisitos

- .NET 8 SDK instalado
- Build exitoso del proyecto API y del proyecto de pruebas

## Estructura del proyecto

- `Common/` — Clase base y utilidades compartidas para las pruebas de integración
- `Endpoints/` — Pruebas de endpoints HTTP contra la API
- `Controllers/` — (si existe) pruebas agrupadas por controlador

## Notas

- Las pruebas usan un `WebApplicationFactory<Program>` y un contenedor PostgreSQL efímero (Testcontainers) para asegurar aislamiento. Requiere Docker en ejecución.
- Si agregas nuevos endpoints o cambios en el esquema de la base de datos, actualiza o agrega pruebas en `Endpoints/`.
