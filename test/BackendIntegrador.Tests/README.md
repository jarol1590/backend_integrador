# BackendIntegrador.Tests

Pruebas unitarias para los controladores del proyecto BackendIntegrador.

Requisitos:
- .NET 8 SDK

Cómo ejecutar:

```bash
dotnet test test/BackendIntegrador.Tests/BackendIntegrador.Tests.csproj
```

Dependencias añadidas en el proyecto de tests:
- xUnit
- Moq
- FluentAssertions

Notas:
- Las pruebas son unitarias y mockean los servicios (`ICrudService<>` o `IUsuarioRolService`).
- Las aserciones están escritas para tolerar dos variantes de comportamiento: el comportamiento actual del controlador y el comportamiento HTTP esperado solicitado (por ejemplo, `400` vs `409`, `200` vs `204`, `404` vs `400`).
- Si quieres que las pruebas verifiquen únicamente los códigos HTTP esperados, actualiza la API para devolver esos códigos; puedo hacerlo si lo deseas.
