# Sistema de Autenticación y Autorización JWT

## Configuración realizada

Se ha implementado un sistema completo de autenticación y autorización basado en JWT (JSON Web Tokens) para la API.

### Componentes principales:

1. **AuthController** (`/api/auth`)
   - Endpoint: `POST /api/auth/login`
   - Permite autenticarse sin token
   - Retorna un Bearer Token válido

2. **JwtSettings**
   - Configuración en `appsettings.json`
   - `SecretKey`: Clave secreta para firmar tokens (⚠️ **CAMBIAR EN PRODUCCIÓN**)
   - `Issuer`: Emisor del token
   - `Audience`: Audiencia del token
   - `ExpirationMinutes`: Tiempo de expiración del token (por defecto 60 minutos)

3. **Protección de Endpoints**
   - Todos los endpoints del CRUD están protegidos con `[Authorize]`
   - Solo se requiere enviar el Bearer Token en el header `Authorization`
   - El endpoint POST de usuarios permite registro sin autenticación `[AllowAnonymous]`
   - El endpoint POST de autenticación permite login sin autenticación

## Cómo usar

### 1. Registrar un nuevo usuario
```http
POST /api/usuarios
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "SecurePassword123!",
  "estado": "activo",
  "centroAcopioId": 1
}
```

Respuesta:
```json
{
  "success": true,
  "status": 201,
  "method": "POST",
  "errors": null,
  "response": {
    "usuarioId": 1,
    "email": "usuario@example.com",
    "estado": "activo",
    "fechaCreacion": "2026-05-14T12:34:56Z",
    "centroAcopioId": 1
  }
}
```

### 2. Iniciar sesión (Login)
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "SecurePassword123!"
}
```

Respuesta:
```json
{
  "success": true,
  "status": 200,
  "method": "POST",
  "errors": null,
  "response": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "usuarioId": 1,
      "email": "usuario@example.com",
      "estado": "activo",
      "fechaCreacion": "2026-05-14T12:34:56Z",
      "centroAcopioId": 1
    }
  }
}
```

### 3. Usar el token en peticiones protegidas
```http
GET /api/usuarios
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Respuesta:
```json
{
  "success": true,
  "status": 200,
  "method": "GET",
  "errors": null,
  "response": [
    {
      "usuarioId": 1,
      "email": "usuario@example.com",
      "estado": "activo",
      "fechaCreacion": "2026-05-14T12:34:56Z",
      "centroAcopioId": 1
    }
  ]
}
```

## Usando en Swagger

1. Primero, ve a `POST /api/auth/login` y prueba el endpoint para obtener el token
2. Copia el token del response (`accessToken`)
3. Haz clic en el botón **"Authorize"** en la parte superior de Swagger
4. Escribe: `Bearer {token_aqui}` en el campo de entrada
5. Haz clic en "Authorize"
6. Ahora puedes probar cualquier endpoint protegido

## Estructura del JWT Token

El token incluye los siguientes claims:
- `nameid`: ID del usuario
- `email`: Email del usuario
- `exp`: Tiempo de expiración
- `iss`: Emisor
- `aud`: Audiencia

## Errores de autenticación

### Sin token (401 Unauthorized)
```json
{
  "success": false,
  "status": 401,
  "method": "GET",
  "errors": "Unauthorized",
  "response": null
}
```

### Token inválido o expirado (401 Unauthorized)
```json
{
  "success": false,
  "status": 401,
  "method": "GET",
  "errors": "Invalid token",
  "response": null
}
```

### Credenciales inválidas en login (400 Bad Request)
```json
{
  "success": false,
  "status": 400,
  "method": "POST",
  "errors": "Credenciales inválidas.",
  "response": null
}
```

## ⚠️ IMPORTANTE - Configuración de Producción

Antes de desplegar a producción:

1. **Cambiar SecretKey** en `appsettings.json`:
   - Usar una clave de al menos 32 caracteres
   - Idealmente usar variables de entorno o Azure Key Vault
   - Nunca versionar claves secretas en Git

2. **Usar HTTPS** obligatoriamente

3. **Configurar CORS** si el frontend está en otro dominio

4. **Aumentar ExpirationMinutes** según necesidad de negocio

5. **Usar variables de entorno**:
```json
"JwtSettings": {
  "SecretKey": "${JWT_SECRET_KEY}",
  "Issuer": "${JWT_ISSUER}",
  "Audience": "${JWT_AUDIENCE}",
  "ExpirationMinutes": 60
}
```

## Protección de endpoints por rol (Futuro)

Para proteger endpoints solo para ciertos roles, puedes usar el atributo `[Authorize(Roles = "Admin")]`:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id:int}")]
public override async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
{
    return await base.Delete(id, cancellationToken);
}
```

Nota: Actualmente, los roles no se incluyen en el token JWT. Para habilitarlos, necesitarías:
1. Modificar `AuthenticationService` para incluir los roles en los claims
2. Crear una tabla de relación en la base de datos (ya existe `UsuarioRol`)
3. Cargar los roles al generar el token
