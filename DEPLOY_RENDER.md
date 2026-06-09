# Despliegue de BackendIntegrador en Render

Guía paso a paso para desplegar la API **Control Lácteo** en [Render](https://render.com) usando **Docker** y **PostgreSQL**.

---

## Tabla de contenidos

1. [Prerrequisitos](#prerrequisitos)
2. [Base de datos PostgreSQL en Render](#base-de-datos-postgresql-en-render)
3. [Desplegar la API (Web Service Docker)](#desplegar-la-api-web-service-docker)
4. [Variables de entorno en Render](#variables-de-entorno-en-render)
5. [Desarrollo local con Docker](#desarrollo-local-con-docker)
6. [Desarrollo local sin Docker](#desarrollo-local-sin-docker)
7. [Verificación post-despliegue](#verificación-post-despliegue)
8. [Solución de problemas](#solución-de-problemas)

---

## Prerrequisitos

| Requisito | Detalle |
|-----------|---------|
| Cuenta Render | [render.com](https://render.com) |
| Repositorio Git | GitHub o GitLab conectado a Render |
| .NET 8 SDK | Solo para desarrollo local (`dotnet run`, `dotnet ef`) |
| Docker Desktop | Solo si pruebas localmente con `docker compose` |

---

## Base de datos PostgreSQL en Render

### 1. Crear la base de datos (si aún no existe)

1. Render Dashboard → **New** → **PostgreSQL**
2. Nombre: `integrador-lacteos` (o el que prefieras)
3. Región: la misma que usarás para la API (ej. Oregon)
4. Plan: Free o superior según necesidad
5. Crear y esperar a que esté **Available**

### 2. Obtener credenciales de conexión

En el panel de la base de datos verás dos URLs:

| URL | Uso |
|-----|-----|
| **Internal Database URL** | Web Service de Render en el **mismo proyecto/región** (recomendada en producción) |
| **External Database URL** | Desarrollo local, Docker en tu PC, herramientas externas |

Formato típico:

```
postgresql://usuario:password@dpg-xxxx-a.oregon-postgres.render.com/integrador_lacteos
```

### 3. Convertir a cadena Npgsql

Render requiere **SSL**. Usa este formato en variables de entorno:

```
Host=dpg-xxxx-a.oregon-postgres.render.com;Port=5432;Database=integrador_lacteos;Username=integrador_lacteos_user;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

Sustituye host, usuario, contraseña y base según tu panel de Render.

### 4. Esquema de base de datos

La API aplica migraciones automáticamente al iniciar (`Database.Migrate()` en `Program.cs`). No necesitas ejecutar `dotnet ef database update` manualmente en Render si el servicio arranca correctamente.

Para aplicar migraciones desde tu PC (opcional):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project src/BackendIntegrador.Infrastructure --startup-project src/BackendIntegrador.Api
```

(Configura antes `appsettings.Development.local.json` con la External Database URL.)

---

## Desplegar la API (Web Service Docker)

### Paso 1: Crear Web Service

1. Render Dashboard → **New** → **Web Service**
2. Conecta el repositorio de **BackendIntegrador**
3. Configuración:

| Campo | Valor |
|-------|-------|
| **Name** | `backendintegrador-api` (o el que prefieras) |
| **Region** | Misma que PostgreSQL |
| **Branch** | `main` |
| **Runtime** | **Docker** |
| **Dockerfile Path** | `./Dockerfile` |
| **Instance Type** | Free o superior |

### Paso 2: Puerto del contenedor

El [`Dockerfile`](Dockerfile) expone el puerto **8080** y define:

```
ENV ASPNETCORE_URLS=http://+:8080
```

En Render, en la sección **Advanced** del Web Service, confirma que el **Port** del servicio es **8080** (valor por defecto para Docker en Render).

### Paso 3: Variables de entorno

Configura las variables de la [sección siguiente](#variables-de-entorno-en-render) **antes** del primer deploy o inmediatamente después de crear el servicio.

### Paso 4: Deploy

1. Clic en **Create Web Service** (o **Manual Deploy** → **Deploy latest commit**)
2. Espera el build de la imagen Docker y el arranque del contenedor
3. Revisa **Logs**:
   - Debe aparecer `Applying migration` o confirmación de migraciones ya aplicadas
   - Sin errores `Failed to connect` ni `password authentication failed`

### Paso 5: URL pública

Render asigna una URL como:

```
https://backendintegrador-api.onrender.com
```

Los endpoints de la API están bajo `/api/...`. Swagger **no** se muestra en `Production` (comportamiento esperado); usa Postman para probar.

---

## Variables de entorno en Render

En el Web Service → **Environment** → **Add Environment Variable**:

### Obligatorias

| Variable | Ejemplo / descripción |
|----------|----------------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena Npgsql con SSL (ver sección BD). Usa **Internal URL** desglosada si la API y la BD están en Render. |
| `JwtSettings__SecretKey` | Clave segura de **mínimo 32 caracteres** (genera una única para producción) |
| `JwtSettings__Issuer` | `BackendIntegrador` |
| `JwtSettings__Audience` | `BackendIntegradorClients` |
| `SeedData__Enabled` | `false` |

### Opcionales (email / SMTP)

| Variable | Descripción |
|----------|-------------|
| `EmailSettings__Password` | Contraseña SMTP (ej. Gmail App Password) |
| `EmailSettings__SmtpServer` | `smtp.gmail.com` (ya en `appsettings.json` si no se sobreescribe) |
| `EmailSettings__Username` | Email remitente |

### Ejemplo mínimo (valores ficticios)

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=dpg-xxxx-a.oregon-postgres.render.com;Port=5432;Database=integrador_lacteos;Username=integrador_lacteos_user;Password=***;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__SecretKey=genera-una-clave-segura-de-al-menos-32-caracteres
JwtSettings__Issuer=BackendIntegrador
JwtSettings__Audience=BackendIntegradorClients
SeedData__Enabled=false
```

**Importante:** No subas contraseñas al repositorio. Configúralas solo en el panel de Render o en `.env` local (gitignored).

---

## Desarrollo local con Docker

Útil para probar la imagen Docker contra PostgreSQL en Render (External URL).

### 1. Configurar `.env`

```powershell
copy .env.example .env
```

Edita `.env` con los valores de la **External Database URL** de Render:

```env
POSTGRES_HOST=dpg-xxxx-a.oregon-postgres.render.com
POSTGRES_PORT=5432
POSTGRES_DB=integrador_lacteos
POSTGRES_USER=integrador_lacteos_user
POSTGRES_PASSWORD=tu_password
JWT_SECRET_KEY=tu-clave-jwt-local-o-la-de-produccion
```

### 2. Levantar el contenedor

```powershell
docker compose up -d --build
```

### 3. Probar

| Recurso | URL |
|---------|-----|
| API | `http://localhost:5111` |
| Swagger | `http://localhost:5111/swagger` |

Swagger está habilitado porque `docker-compose.yml` usa `ASPNETCORE_ENVIRONMENT=Docker`.

### 4. Detener

```powershell
docker compose down
```

---

## Desarrollo local sin Docker

### 1. Configuración local (no versionada)

```powershell
copy src\BackendIntegrador.Api\appsettings.Development.local.json.example src\BackendIntegrador.Api\appsettings.Development.local.json
```

Edita `appsettings.Development.local.json` con la External Database URL en formato Npgsql + SSL.

### 2. Ejecutar la API

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/BackendIntegrador.Api/BackendIntegrador.Api.csproj
```

| Recurso | URL |
|---------|-----|
| API | `http://localhost:5111` |
| Swagger | `http://localhost:5111/swagger` |

---

## Verificación post-despliegue

### 1. Logs en Render

- Servicio en estado **Live**
- Sin excepciones de conexión a PostgreSQL al iniciar

### 2. Probar autenticación (Postman)

Importa [`BackendIntegrador.postman_collection.json`](BackendIntegrador.postman_collection.json).

1. Variable `baseUrl` = `https://tu-servicio.onrender.com`
2. **Auth → POST login** con un usuario existente en la BD
3. Verifica respuesta `200` con `accessToken`

### 3. Endpoints protegidos

Con el token en el header `Authorization: Bearer {token}`:

- `GET /api/usuarios`
- `GET /api/departamentos`

---

## Solución de problemas

| Síntoma | Causa probable | Qué hacer |
|---------|----------------|-----------|
| `Failed to connect to ...:5432` | Host/puerto incorrectos o SSL faltante | Revisa `ConnectionStrings__DefaultConnection`; añade `SSL Mode=Require;Trust Server Certificate=true` |
| `password authentication failed` | Usuario/contraseña incorrectos | Copia de nuevo desde Render → PostgreSQL → Connections |
| `database "..." does not exist` | Nombre de BD incorrecto | Verifica `Database=` en la cadena (ej. `integrador_lacteos`) |
| Build Docker falla | Error de compilación | Ejecuta `dotnet build` localmente y corrige errores |
| App Live pero 401 en login | `JwtSettings__SecretKey` distinta a la usada al hashear tokens | Usa la misma clave JWT en todos los entornos o resetea usuarios |
| Swagger no aparece en Render | `ASPNETCORE_ENVIRONMENT=Production` | Normal; usa Postman. Para Swagger en staging, usa entorno `Docker` solo en local |
| Servicio Free se "duerme" | Plan gratuito de Render | Primera petición tras inactividad puede tardar ~30–60 s |

---

## Resumen rápido

```text
PostgreSQL (Render)  →  Internal URL  →  ConnectionStrings__DefaultConnection
        ↓
Web Service (Docker, puerto 8080)  →  Variables JWT + SeedData__Enabled=false
        ↓
https://tu-servicio.onrender.com  →  Postman / Frontend
```

Para más detalle del dominio y la API, consulta [`README.md`](README.md).
