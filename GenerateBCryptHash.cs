using BCrypt.Net;

// Este script genera un hash BCrypt para la contraseña que ingreses
// Ejecuta: dotnet run

Console.WriteLine("=== Generador de Hash BCrypt ===");
Console.WriteLine("Ingresa la contraseña que deseas hashear:");
var password = Console.ReadLine();

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Error: La contraseña no puede estar vacía.");
    return;
}

try
{
    var hashedPassword = BCrypt.HashPassword(password);
    Console.WriteLine("\n✓ Hash generado exitosamente:");
    Console.WriteLine($"\nContraseña: {password}");
    Console.WriteLine($"Hash BCrypt: {hashedPassword}");

    // Verificar que funciona
    bool verify = BCrypt.Verify(password, hashedPassword);
    Console.WriteLine($"\n✓ Verificación: {(verify ? "CORRECTA" : "INCORRECTA")}");

    Console.WriteLine("\n--- SQL UPDATE STATEMENT ---");
    Console.WriteLine($"UPDATE Usuario");
    Console.WriteLine($"SET PasswordHash = '{hashedPassword}'");
    Console.WriteLine($"WHERE Email = 'admin@lechecaldas.co';");
}
catch (Exception ex)
{
    Console.WriteLine($"Error al generar el hash: {ex.Message}");
}
