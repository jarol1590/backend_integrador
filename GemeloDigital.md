Actúa como Arquitecto de Soluciones, Científico de Datos y Desarrollador Senior en .NET (C#). Trabajas sobre el repositorio BackendIntegrador: API REST .NET 8 con Clean Architecture (Domain → Application → Infrastructure → Api), EF Core + PostgreSQL, JWT, módulo de usuarios con Facade y DTOs polimórficos por rol.

## CONTEXTO DEL NEGOCIO

Portal "Control Lácteo" con trazabilidad: Productor → Finca → Ordeño → Lote → Muestra → Análisis de Calidad → ResultadoParametro (parámetros configurables: Acidez, grasa, proteína, CCS, etc.).

Entidades relevantes ya existentes en `src/BackendIntegrador.Domain/Entities/`:
- Geográficas: Departamento, Municipio
- Producción: Finca (Latitud, Longitud, MunicipioId), Ordeno (VolumenLitros, fechas)
- Calidad: ParametroCalidad, Muestra, AnalisisCalidad, ResultadoParametro
- Actores: Productor, CentroAcopio, Usuario (4 roles excluyentes)

Cadena de trazabilidad hacia calidad (usar para correlacionar clima ↔ calidad):
Finca → Ordeno → Lote → Muestra → AnalisisCalidad → ResultadoParametro

## OBJETIVO EVOLUTIVO

Implementar un **Gemelo Digital (Digital Twin)** que agregue valor predictivo y analítico para **productores** y **centros de acopio**, permitiendo decisiones preventivas sobre calidad y producción lechera.

## RESTRICCIONES INNEGOCIABLES

### RESTRICCIÓN NEGATIVA (NO HACER)
- El gemelo digital NO debe orientarse a logística, transporte, rutas, optimización de flotas ni recepción en acopio como eje central.
- NO diseñar features de: Transporte, RecepcionAcopio, rutas GPS, tiempos de tránsito, cadena de frío logística.
- Puedes **leer** Lote/Muestra solo como puente hacia resultados de calidad de una finca; no expandir el dominio logístico.

### ENFOQUE PRINCIPAL (SÍ HACER)
- Centrarse en factores **climáticos/ambientales**: temperatura, humedad, precipitación, radiación/UV, estacionalidad, clima regional.
- Modelar cómo el clima impacta:
  - Volumen de producción (Ordeno.VolumenLitros)
  - Calidad láctea (ResultadoParametro: acidez/pH, grasa, proteína, recuento células somáticas, etc.)
- Generar **alertas preventivas** y **escenarios what-if** para productores (por finca) y centros de acopio (vista agregada por municipio/región de sus productores asociados, sin lógica de transporte).

## REQUERIMIENTOS TÉCNICOS OBLIGATORIOS

### 1. Análisis del contexto actual
Antes de proponer diseño, debes escanear el workspace y documentar:

**Datos ya disponibles (inventario):**
- Qué entidades/campos sirven como variables de entrada/salida del gemelo
- Cómo se relacionan Finca.Latitud/Longitud/MunicipioId con ubicación para APIs climáticas
- Qué parámetros de calidad existen en ParametroCalidad y cómo se historizan en ResultadoParametro
- Qué granularidad temporal tienen Ordeno y AnalisisCalidad
- Qué roles pueden consumir qué vistas (Productor → sus fincas; Centro de Acopio → agregación regional; Administrador → global)

**Datos faltantes (gap analysis):**
- Series climáticas históricas y pronósticos (API externa vs simulación)
- Índices derivados (THI — índice de estrés térmico, estrés hídrico, etc.)
- Estado virtual del gemelo (última sincronización, versión del modelo, confianza de predicción)
- Eventos/alertas persistidos
- Metadatos de correlación clima-calidad por finca

**Integraciones externas candidatas (evaluar y recomendar una):**
- Open-Meteo (gratuita, sin API key)
- OpenWeatherMap
- NASA POWER / ERA5 (académico)
- Para MVP académico: preferir opción sin fricción de credenciales, documentando límites de rate limit

### 2. Modelo conceptual del gemelo digital
Define explícitamente:

**Entidad virtual gemela:** ¿FincaDigitalTwin? ¿Un gemelo por FincaId?
- Variables de **entrada (clima):** temp min/max/media, humedad, precipitación, estación del año, THI, etc.
- Variables de **estado interno:** última lectura climática, ventana histórica usada, calidad de datos
- Variables de **salida (predicción/alerta):**
  - Producción esperada (litros/día o por ordeño)
  - Riesgo de desviación en parámetros de calidad (probabilidad o score 0-100)
  - Recomendaciones accionables (ej. ajustar horario de ordeño, sombra, ventilación)

**Ciclo de vida:**
- Sincronización periódica (background job) vs consulta on-demand
- Cómo se versiona el modelo/reglas heurísticas vs ML futuro

**Diagrama obligatorio en Fase 1:** flujo Clima API → Ingesta → Almacenamiento series → Motor analítico → Alertas/API → Frontend

### 3. Casos de uso de valor agregado (mínimo 3)
Proponer escenarios concretos con actor, trigger, acción y métrica de valor:

Ejemplo 1 — Productor:
"Alerta de posible acidificación por ola de calor prolongada"
- Trigger: THI > umbral durante N días + historial de caída de pH en ResultadoParametro
- Acción: POST alerta + recomendación preventiva
- Valor: reducir rechazo de lote por calidad

Ejemplo 2 — Productor:
"Caída proyectada de volumen por estrés térmico estacional"
- Correlacionar Ordeno.VolumenLitros histórico con temperatura en ventana de 7-14 días

Ejemplo 3 — Centro de Acopio:
"Mapa de riesgo de calidad por municipio/región"
- Agregar fincas de productores que entregan a ese centro (sin usar transporte como eje)
- Score de riesgo climático agregado para planificar capacidad de recepción/análisis (solo como contexto operativo, no rutas)

Cada caso debe indicar: endpoints propuestos, DTOs de respuesta y reglas de autorización por rol.

### 4. Arquitectura de integración en .NET
Debes evaluar y recomendar con pros/contras:

**Opción A — Módulo dentro de la misma solución**
- Nuevo proyecto o carpetas: `BackendIntegrador.DigitalTwin` (Application + Infrastructure)
- Misma API con prefijo `/api/gemelo-digital/` o `/api/fincas/{id}/gemelo/`
- PostgreSQL ampliado vs tabla dedicada para series temporales

**Opción B — Servicio separado**
- Microservicio `DigitalTwin.Api` + comunicación HTTP/eventos con BackendIntegrador
- Base de series temporales aparte (InfluxDB, TimescaleDB, PostgreSQL con partición por fecha)

**Opción C — Híbrido MVP académico**
- API transaccional intacta; jobs en Infrastructure; series climáticas en tablas nuevas EF; motor heurístico simple (sin ML en v1)

Criterios de decisión obligatorios:
- No degradar rendimiento de CRUD transaccional actual
- Facilidad de despliegue en entorno universitario (Render + PostgreSQL)
- Extensibilidad futura hacia ML.NET o modelo externo
- Separación de concerns (IClimateDataProvider, ITwinSimulationEngine, ITwinAlertService)

## ALCANCE DE IMPLEMENTACIÓN ESPERADO (para fases posteriores)

Contratos sugeridos (no implementar aún):
- `IClimateDataProvider` — obtiene histórico/pronóstico por lat/lon
- `IFincaDigitalTwinService` — estado del gemelo, sincronización
- `IMilkQualityPredictor` — heurísticas/regresión simple v1
- `ITwinAlertRepository` — persistencia de alertas

Endpoints candidatos (no implementar aún):
- `GET /api/fincas/{fincaId}/gemelo/estado`
- `GET /api/fincas/{fincaId}/gemelo/clima?desde=&hasta=`
- `GET /api/fincas/{fincaId}/gemelo/predicciones?horizonteDias=7`
- `GET /api/fincas/{fincaId}/gemelo/alertas`
- `POST /api/fincas/{fincaId}/gemelo/sincronizar` (admin/productor dueño)
- `GET /api/centros-acopio/{id}/gemelo/riesgo-regional` (centro de acopio)

## RESTRICCIÓN DE EJECUCIÓN — FASES ESTRICTAS

### FASE 1: Revisión del proyecto y plan estratégico (OBLIGATORIO — DETENERSE AQUÍ)
NO escribas código .NET, migraciones EF, controladores ni tests en esta fase.

Debes entregar un **PLAN MAESTRO** que incluya:

1. **Inventario de datos existentes** (tabla: entidad → campos útiles → gap)
2. **Modelo conceptual del gemelo** (diagrama + variables entrada/salida)
3. **3 casos de uso** detallados con valor de negocio
4. **Arquitectura recomendada** (A/B/C) con justificación para este repo académico
5. **Modelo de datos propuesto** (entidades nuevas en borrador, sin código)
6. **Estrategia de API** (rutas, DTOs, autorización por rol)
7. **Plan de fases 2-4** con estimación de complejidad (S/M/L)
8. **Riesgos y mitigaciones** (API climática caída, datos insuficientes, over-engineering)
9. **Criterios de éxito MVP** medibles para el integrador universitario

Al finalizar Fase 1, **DETENTE COMPLETAMENTE** y pregunta:
> "¿Este plan estratégico del Gemelo Digital está alineado con tu visión? ¿Apruebas la arquitectura recomendada y los casos de uso para continuar con Fase 2?"

### FASE 2: Diseño de arquitectura de datos (solo tras aprobación)
- Esquema ER de tablas nuevas
- Decisión PostgreSQL vs time-series DB
- Migraciones EF propuestas
- Índices y políticas de retención de datos climáticos

### FASE 3: Implementación inicial (solo tras aprobación Fase 2)
- Interfaces en Application
- Servicios en Infrastructure (cliente HTTP climático, motor heurístico v1)
- Controlador(es) y DTOs
- Registro DI en Program.cs
- Tests unitarios + integración (mock de API climática)
- Actualización de BackendIntegrador.postman_collection.json

### FASE 4: Refinamiento (opcional)
- Background service para sincronización
- Mejora de modelos predictivos
- Dashboard endpoints agregados

## INSTRUCCIONES DE TRABAJO INMEDIATAS

1. Lee README.md, entidades en Domain, AppDbContext, controladores de Finca/Ordeno/AnalisisCalidad/ParametrosCalidad.
2. Ignora Transporte y RecepcionAcopio como dominio del gemelo (solo mencionar si aparecen en la cadena de trazabilidad).
3. Respeta convenciones existentes: Clean Architecture, DTOs en Application, servicios en Infrastructure, controladores delgados.
4. Ejecuta **únicamente Fase 1** ahora.
5. Usa español en la documentación del plan.
6. Incluye diagramas mermaid donde aporten claridad.

Comienza Fase 1 ahora.