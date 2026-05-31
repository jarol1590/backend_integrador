Actúa como un Arquitecto de Bases de Datos y Desarrollador Senior en .NET (C#) experto en Entity Framework Core y diseño de APIs RESTful.

Estamos refactorizando el módulo de usuarios para un portal de "Control Lácteo". Actualmente tenemos una única tabla de base de datos para los Usuarios, pero el sistema maneja 4 roles distintos con reglas de negocio y relaciones estrictamente excluyentes:
1. Administrador global
2. Centro de Acopio
3. Productor
4. Trabajador Centro de acopio

Existen las siguientes restricciones relacionales críticas que deben ser evaluadas:
- Un "Centro de Acopio" o un "Trabajador Centro de acopio" NO pueden tener fincas asociadas bajo ninguna circunstancia.
- Un "Productor" (Productor lácteo) NO puede estar asociado a un Centro de Acopio como lugar de trabajo.

Necesito rediseñar la forma en que se extrae y retorna la información en los endpoints de usuario para que el frontend reciba respuestas limpias y específicas según el rol, evitando enviar propiedades innecesarias o nulas (evitar devolver un array de "fincas" a un trabajador, por ejemplo). Además, se deben actualizar las pruebas y la colección de Postman.

Por favor, genera un PROMPT DETALLADO Y EXTENSO que yo pueda utilizar para guiar este desarrollo paso a paso. El prompt debe exigir el cumplimiento estricto de los siguientes requerimientos técnicos:

### REQUERIMIENTOS TÉCNICOS A INTEGRAR EN EL PROMPT:
1. **Análisis de Base de Datos y Consultas Condicionales:** Diseñar la estrategia de acceso a datos en Entity Framework Core. Se debe utilizar carga condicional (Conditional Includes) o proyecciones (`.Select()`) que evalúen el rol del usuario antes de intentar hacer `JOIN` con tablas que no le corresponden.
2. **DTOs Dinámicos/Polimórficos:** Creación de Response DTOs que se adapten al rol del usuario. El contrato de la API debe mutar inteligentemente (ej. `ProductorResponseDto` vs `TrabajadorResponseDto`) u omitir propiedades irrelevantes para no contaminar el cliente.
3. **Validación de Reglas de Negocio:** Implementación de validaciones a nivel de servicio para asegurar la integridad estructural (lanzar excepciones si se intenta asignar una finca a un Centro de Acopio).
4. **Actualización de Artefactos de Pruebas:** Modificación o creación de Pruebas Unitarias/Integración (xUnit/NUnit) que cubran las restricciones de los roles.
5. **Postman Collection:** Generación del JSON de la colección de Postman actualizada con ejemplos de respuesta esperados para cada uno de los 4 roles.

### RESTRICCIÓN DE EJECUCIÓN (CRÍTICO):
El prompt debe obligar al modelo a trabajar en fases estrictas y detenerse en la Fase 1 hasta recibir aprobación:
- **Fase 1: Análisis de Arquitectura y Planificación (ESPERAR APROBACIÓN):** Antes de escribir código .NET, el modelo debe presentar un esquema del diseño propuesto. Debe incluir:
  - Cómo estructurará las consultas en EF Core dadas las reglas exclusivas.
  - El diseño JSON de las respuestas de la API para cada uno de los 4 roles (Response DTOs).
  - Qué lógica de validación se implementará para proteger la base de datos de inconsistencias.
  *El modelo debe detenerse aquí y preguntar si el plan es correcto.*
- **Fase 2: Implementación Backend:** Una vez aprobado, generar los DTOs, Controladores, Servicios y las consultas en EF Core.
- **Fase 3: Pruebas y Postman:** Finalmente, proporcionar el código de los tests y el JSON de la colección de Postman.