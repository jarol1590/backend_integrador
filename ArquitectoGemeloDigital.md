Actúa como un Arquitecto de Soluciones, Científico de Datos y Desarrollador Senior en .NET. 

Tengo un portal de "Control Lácteo" desarrollado en .NET. Quiero dar el siguiente paso evolutivo en el proyecto e implementar un Gemelo Digital (Digital Twin) que agregue un valor predictivo y analítico real para los productores y centros de acopio. 

Existen dos premisas fundamentales para este diseño:
1. RESTRICCIÓN NEGATIVA: El gemelo digital NO debe estar orientado a logística, transporte o diseño de rutas.
2. ENFOQUE PRINCIPAL: El gemelo digital DEBE centrarse en temas climáticos/ambientales (temperatura, humedad, estacionalidad, clima regional) y cómo estos factores impactan directamente en la CALIDAD de la leche (volumen de producción, acidez, niveles de grasa/proteína, recuento de células somáticas, etc.) para permitir la toma de decisiones preventivas.

Por favor, genera un PROMPT DETALLADO Y EXTENSO que yo pueda ejecutar en este IDE para guiar la construcción de este sistema. El prompt debe exigir el cumplimiento de los siguientes requerimientos:

### REQUERIMIENTOS TÉCNICOS A INTEGRAR EN EL PROMPT:
1. **Análisis del Contexto Actual:** El modelo deberá revisar la estructura de mi proyecto .NET actual (entidades, base de datos, servicios) para identificar qué puntos de datos ya existen (ej. registros de calidad de leche por finca) y qué datos faltarían simular o integrar (ej. APIs meteorológicas de terceros).
2. **Modelo Conceptual del Gemelo Digital:** Definición de cómo se representará virtualmente la finca/entorno y qué variables de entrada (clima) y salida (calidad/producción) tendrá el modelo.
3. **Casos de Uso de Valor Agregado:** Proponer 2 o 3 escenarios específicos donde este gemelo digital tome decisiones o genere alertas útiles (ej. "Alerta de posible acidificación por ola de calor prolongada").
4. **Arquitectura de Integración en .NET:** Cómo coexistirá el motor del gemelo digital (y su posible almacenamiento de series temporales o eventos) con la API transaccional actual sin degradar su rendimiento, ¿Se debe incluir en la misma solución? ¿Se debe tener una solución aparte con su propio despliegue e interconectarlos?.

### RESTRICCIÓN DE EJECUCIÓN (CRÍTICO):
El prompt debe obligar al modelo a trabajar en fases estrictas y detenerse obligatoriamente en la Fase 1:
- **Fase 1: Revisión del Proyecto y Plan Estratégico (ESPERAR APROBACIÓN):** Antes de generar cualquier código, el modelo debe escanear el workspace actual, analizar la estructura del proyecto y presentar un PLAN MAESTRO. Este plan debe detallar el alcance del gemelo digital, las entidades involucradas, las fuentes de datos climáticos requeridas y el valor de negocio esperado. *El modelo debe detenerse por completo aquí y preguntar si el enfoque es útil y está alineado con la visión del proyecto.*
- **Fase 2: Diseño de Arquitectura de Datos:** Una vez aprobado el plan, el modelo propondrá las nuevas tablas/entidades necesarias, si se requiere una base de datos de series de tiempo (como InfluxDB) o si se adaptará la actual, y cómo se estructurará el backend para soportar las simulaciones.
- **Fase 3: Implementación Inicial:** Generación de los contratos (interfaces), servicios de integración climática y endpoints de simulación/predicción en .NET.