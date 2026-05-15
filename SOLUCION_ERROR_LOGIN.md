# Solución: Error de Login "Index and length must refer to a location within the string"

## Causa del Problema

El error `"Index and length must refer to a location within the string. (Parameter 'length')"` ocurre cuando BCrypt intenta validar una contraseña contra un hash inválido o vacío.

En tu caso, el usuario `admin@lechecaldas.co` fue creado con un `PasswordHash` que está:
- Vacío (`""`)
- Nulo
- En formato incorrecto (no es un hash BCrypt válido)

Esto puede haber ocurrido porque:
1. El usuario fue creado antes de implementar el hash BCrypt
2. El usuario fue insertado directamente en la BD sin hash
3. Se insertó un valor erróneo en el campo PasswordHash

## Soluciones

### Opción 1: Resetear contraseña usando el endpoint (RECOMENDADO)

**Paso 1:** Primero, crea un usuario de prueba con contraseña válida:
```http
POST http://localhost:5000/api/usuarios
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123456",
  "estado": "activo",
  "centroAcopioId": 1
}
```

**Paso 2:** Loguéate con ese usuario para obtener un token:
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123456"
}
```

Respuesta (copia el `accessToken`):
```json
{
  "success": true,
  "status": 200,
  "method": "POST",
  "errors": null,
  "response": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": { ... }
  }
}
```

**Paso 3:** Usa el token para resetear la contraseña de admin:
```http
POST http://localhost:5000/api/auth/reset-password/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "newPassword": "Admin@123456"
}
```

Ahora podrás hacer login con:
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@lechecaldas.co",
  "password": "Admin@123456"
}
```

---

### Opción 2: Actualizar directamente en SQLite

**Paso 1:** Abre DB Browser for SQLite o similar
- Abre la BD: `integrador.db`

**Paso 2:** Genera un hash BCrypt válido

Ejecuta este código en cualquier aplicación C#:
```csharp
using BCrypt.Net;

var password = "Admin@123456";
var hash = BCrypt.HashPassword(password);
Console.WriteLine(hash);
// Ejemplo de salida: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR0Ifxq
```

**Paso 3:** Ejecuta el SQL UPDATE
```sql
UPDATE Usuario
SET PasswordHash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR0Ifxq'
WHERE Email = 'admin@lechecaldas.co';

-- Verificar
SELECT UsuarioId, Email, PasswordHash, Estado FROM Usuario WHERE Email = 'admin@lechecaldas.co';
```

Ahora podrás hacer login normalmente.

---

### Opción 3: Usar el script C# que se incluyó

Si ejecutas `GenerateBCryptHash.cs`:
```bash
dotnet run --project GenerateBCryptHash.cs
```

Te pedirá que ingreses la contraseña y te generará el hash automáticamente con el SQL listo para copiar.

---

## Cambios Implementados para Prevenir Este Error en el Futuro

### 1. **Validaciones Mejoradas en AuthenticationService**
- ✅ Verifica que el PasswordHash no esté vacío
- ✅ Captura excepciones de BCrypt.Verify()
- ✅ Lanza mensajes de error más descriptivos

### 2. **Nuevos Endpoints**
- `POST /api/auth/change-password` - Cambiar contraseña propia
- `POST /api/auth/reset-password/{usuarioId}` - Resetear contraseña de otro usuario (admin)

### 3. **Nuevo Servicio**
- `IUserManagementService` - Para operaciones administrativas de usuarios

### 4. **DTOs Nuevos**
- `ChangePasswordDto` - Para cambiar contraseña propia
- `ResetPasswordDto` - Para resetear contraseña ajena

---

## Verificación

Después de resetear la contraseña, verifica que funciona correctamente:

```bash
# Login exitoso
POST /api/auth/login
{
  "email": "admin@lechecaldas.co",
  "password": "Admin@123456"
}

# Respuesta esperada (status 200)
{
  "success": true,
  "status": 200,
  "method": "POST",
  "errors": null,
  "response": {
    "accessToken": "eyJ...",
    "usuario": { ... }
  }
}
```

---

## Notas Importantes

1. **Seguridad**: Nunca compartas hashes de contraseñas
2. **Contraseñas**: Siempre usa contraseñas fuertes (mínimo 8 caracteres, mix de tipos)
3. **Admin**: Considera cambiar la contraseña de admin regularmente
4. **Backup**: Realiza backup de tu BD antes de hacer cambios directos

