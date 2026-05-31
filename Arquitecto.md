Actúa como un Arquitecto de Software y Desarrollador Senior en .NET (C#), experto en diseño de APIs RESTful y patrones de integración.

Actualmente tengo una API en .NET para un portal de "Control Lácteo". El problema principal es que los endpoints están demasiado segregados (antipatrón "Chatty API"). El frontend se ve obligado a consumir múltiples endpoints para realizar acciones básicas, lo que genera problemas de latencia y complejidad en el cliente. 

Necesito rediseñar y consolidar la API para que sea más orientada a casos de uso o pantallas del frontend (patrón BFF / Facade), y quiero empezar exclusivamente por el módulo de MANEJO DE USUARIOS.

Por favor, genera un PROMPT DETALLADO Y EXTENSO que yo pueda utilizar para que me guíes en este desarrollo. El prompt que vas a construir debe exigir el cumplimiento estricto de los siguientes requerimientos técnicos:

### REQUERIMIENTOS TÉCNICOS A INTEGRAR EN EL PROMPT:
1. **Consolidación de Endpoints (Facade/BFF):** Diseñar nuevos endpoints agregados para la gestión de usuarios (ej. un solo endpoint que devuelva el perfil del usuario, sus roles, y los permisos asociados a las fincas/producción láctea).
2. **Uso de DTOs Enriquecidos:** Implementación de Data Transfer Objects (DTOs) diseñados específicamente para las necesidades de las vistas del frontend, evitando el over-fetching o under-fetching de datos.
3. **Optimización en el Acceso a Datos:** Si se usa Entity Framework Core, asegurar que las consultas agrupen la data necesaria de manera eficiente (usando `.Include()` o proyecciones con `.Select()`) para evitar el problema de consultas N+1 al traer datos relacionados del usuario.
4. **Manejo de Transacciones:** Agrupar operaciones de escritura complejas (ej. crear usuario + asignar rol + crear registro inicial en el sistema lácteo) en una sola petición transaccional desde el controlador hasta el servicio.

### RESTRICCIÓN DE EJECUCIÓN (CRÍTICO):
El prompt debe obligar al modelo a trabajar en dos fases estrictas:
- **Fase 1: Planificación y Análisis de Endpoints de Usuario:** Antes de escribir código, el modelo debe analizar el problema y proponer un "contrato" de la nueva API para usuarios. Debe listar qué endpoints actuales asume que existen, y mostrar la estructura exacta en formato JSON de los nuevos Request DTOs y Response DTOs consolidados. No mostrará código en C# hasta que el usuario valide este plan de reestructuración.
- **Fase 2: Implementación en .NET:** Una vez aprobado el plan, procederá a generar el código C# limpio, incluyendo Controladores, Servicios, DTOs y la lógica de mapeo.