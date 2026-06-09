# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BackendIntegrador.sln .
COPY src/BackendIntegrador.Api/BackendIntegrador.Api.csproj src/BackendIntegrador.Api/
COPY src/BackendIntegrador.Application/BackendIntegrador.Application.csproj src/BackendIntegrador.Application/
COPY src/BackendIntegrador.Domain/BackendIntegrador.Domain.csproj src/BackendIntegrador.Domain/
COPY src/BackendIntegrador.Infrastructure/BackendIntegrador.Infrastructure.csproj src/BackendIntegrador.Infrastructure/

RUN dotnet restore src/BackendIntegrador.Api/BackendIntegrador.Api.csproj

COPY src/ src/

RUN dotnet publish src/BackendIntegrador.Api/BackendIntegrador.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BackendIntegrador.Api.dll"]
