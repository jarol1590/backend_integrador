-- Script para resetear la contraseña de admin@lechecaldas.co
-- La contraseña será: Admin@123456
-- Hash BCrypt generado: $2a$11$somehash (esto es un ejemplo)

-- Para generar el hash correcto, ejecuta este código en C#:
-- string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123456");
-- Console.WriteLine(hashedPassword);

-- Luego reemplaza el valor en el UPDATE de abajo

-- OPCIÓN 1: Si conoces el hash correcto
UPDATE Usuario
SET PasswordHash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR0Ifxq'
WHERE Email = 'admin@lechecaldas.co';

-- Verificar que se actualizo
SELECT UsuarioId, Email, PasswordHash, Estado FROM Usuario WHERE Email = 'admin@lechecaldas.co';
