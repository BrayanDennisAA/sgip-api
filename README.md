# SGIP - Sistema de Gestión de Inversiones y Préstamos

Api Restful para la gestión de préstamos e inversiones con simulación y solicitud de préstamos con cálculos financieros
procesamiento de transacciones con garantías de idempotencia, flujo de aprobación de préstamos.

## Funcionalidades Implementadas

### Gestión de Préstamos

- Simulación de préstamos utilizando Sistema Francés y Sistema Alemán.
- Generación automática de cronograma de pagos.
- Solicitud de préstamos, con aprobación automática simulando scoring (monto < $10,000 y menos de 2 préstamos activos).
- Flujo de aprobación/rechazo manual para el resto de los casos.
- Consulta de préstamos por identificador y listado con filtro por usuario..

### Gestión de Transacciones

- Registro de transacciones con garantía de idempotencia mediante el
  header `Idempotency-Key`.
- Prevención de transacciones duplicadas a nivel de aplicación (chequeo
  previo) y a nivel de base de datos (índice único).
- Consulta y listado de transacciones con filtros por tipo y estado.

### Reglas de Negocio

- Validación de monto mínimo y máximo.
- Validación de plazo permitido.
- Validación de máximo de préstamos activos por cliente.
- Validación de capacidad de pago (cuota total ≤ 40% de ingresos
  declarados).

---

## Links Remotas (Producción)

| Link | URl |
|------|-----|
| Swagger | [https://sgip-api-production-6ecd.up.railway.app/swagger/index.html](https://sgip-api-production-6ecd.up.railway.app/swagger/index.html) |
| Api | [https://sgip-api-production-6ecd.up.railway.app/api](https://sgip-api-production-6ecd.up.railway.app/api)|

## Links Locales (Desarrollo)

| Link | URl |
|------|-----|
| Swagger | [http://localhost:5100/swagger/index.html](http://localhost:5100/swagger/index.html) |
| Api | [http://localhost:5100/api](http://localhost:5100/api) |

## Links Locales (Docker Compose)

| Link | URl |
|------|-----|
| Swagger | [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html) |
| Api | [http://localhost:8080/api](http://localhost:8080/api) |    

---

## Tecnologías utilizadas

| Tecnología | Versión |
|------------|---------|
| .NET | 10.0 |
| Entity Framework Core | 10.0 |
| PostgreSQL | 18.0 |
| Docker | 29 |
| Docker Compose | 5.5 |
| FluentValidation | 12.1 |
| Unit Testing | xUnit 2.9 |

### Decisiones técnicas

Se ha decidido utilizar .NET 10.0 para aprovechar las últimas características del lenguaje y del framework, así como para garantizar un rendimiento óptimo y soporte a largo plazo. Entity Framework Core 10.0 se utiliza como ORM para facilitar la interacción con la base de datos PostgreSQL 18.0, que es conocida por su robustez y escalabilidad.

---

## Instalación y configuración

- Clona el repositorio:

```bash
git clone https://github.com/BrayanDennisAA/sgip-api.git

cd sgip-api
```

- Crear el archivo `appsettings.Development.json` en la siguiente ruta: `src/Sgip.WebApi/` y agregar la configuración de la base de datos y logging:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sgip_db;Username=postgres;Password=yourpassword"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
``` 

- Instalar las dependencias y ejecutar las migraciones:

```bash
dotnet restore
dotnet build
```

```bash
dotnet ef database update --project src/Sgip.Infrastructure --startup-project ./src/Sgip.WebApi
```

- Ejecutar la aplicación:

```bash
dotnet run --project src/Sgip.WebApi
```

- Acceder a la documentación de Swagger en [http://localhost:5100/swagger/index.html](http://localhost:5100/swagger/index.html) para explorar los endpoints disponibles.

### Ejecución de pruebas

Ejecutar el siguiente comando en la raíz del proyecto para correr las pruebas:

```bash
dotnet test
```

### Ejecución con Docker Compose

```bash
docker-compose up --build
```
Las migraciones y el seed data se aplican automáticamente al arrancar el
contenedor de la API — no hace falta correr `dotnet ef database update` a
mano en este flujo.

## Endpoints Disponibles

### Loans

| Método | Endpoint |
|----------|----------|
| POST | /api/loans/simulate |
| POST | /api/loans |
| GET | /api/loans |
| GET | /api/loans/{id} |
| GET | /api/loans/{id}/schedule |
| PATCH | /api/loans/{id}/approve |
| PATCH | /api/loans/{id}/reject |

### Transactions

| Método | Endpoint |
|----------|----------|
| POST | /api/transactions |
| GET | /api/transactions |
| GET | /api/transactions/{id} |

## Manejo de errores

La API responde errores en formato [RFC 7807](https://www.rfc-editor.org/rfc/rfc7807)
(`application/problem+json`), de forma consistente sin importar el origen
del error:

| Código | Cuándo | Origen |
|---|---|---|
| `400` | JSON malformado, tipo de dato incorrecto, campo requerido faltante en el body | Binding de ASP.NET Core (`InvalidModelStateResponseFactory`) |
| `400` | Falla una regla de FluentValidation (rango de monto, plazo, etc.) | Validación en el controller antes de llamar al servicio |
| `404` | El recurso solicitado no existe | `Result<T>` con `ErrorType.NotFound` |
| `409` | El recurso existe pero no está en el estado correcto (ej. aprobar un préstamo que ya no está `Pending`) | `Result<T>` con `ErrorType.Conflict` |
| `422` | Se violó una regla de negocio evaluable de antemano (monto fuera de rango a nivel de dominio, máximo de préstamos activos, capacidad de pago) | `Result<T>` con `ErrorType.Validation` |
| `500` | Error no anticipado (infraestructura, bug) | Excepción real, capturada por `ExceptionHandlingMiddleware` |

Ejemplo de respuesta (`409`):
```json
{
  "status": 409,
  "title": "Conflicto de estado",
  "detail": "Solo se pueden aprobar préstamos en estado Pending. Estado actual: Approved.",
  "instance": "/api/loans/3fa85f64-.../approve",
  "code": "conflict",
  "traceId": "00-fd24f5a7..."
}
```

### Por qué `Result<T>` en vez de excepciones para el flujo de negocio

- **`Result<T>`** para todo fallo que el propio servicio puede anticipar
  antes de que pase — cero costo de excepción, y el tipo de error
  (`ErrorType.Validation/Conflict/NotFound`) viaja como dato, no como
  jerarquía de excepciones que el caller tiene que interpretar con `catch`.
- **Excepciones**, solo para lo que de verdad no se puede anticipar (falla
  de conexión a la base de datos, condición de carrera detectada por EF
  Core) — ahí sí se paga el costo, porque es infrecuente por diseño y el
  stack trace real importa para diagnosticar.
---

## Estructura del proyecto

```
src/
├── Sgip.Application/          # Servicios de aplicación, DTOs, validadores FluentValidation
├── Sgip.Domain/                # Entidades, enums, estrategias de cálculo (Strategy), excepciones de negocio
├── Sgip.Infrastructure/        # Repositorios, DbContext, Unit of Work, seed data
├── Sgip.WebApi/                # Controladores, Program.cs, configuración de la API
 
tests/
├── Sgip.UnitTests/             # Cálculo financiero, estrategias, validadores, reglas de negocio
├── Sgip.IntegrationTests/      # idempotencia
```
---
## Arquitectura

Arquitectura en capas inspirada en DDD, sin aplicarlo de forma estricta:
separación clara entre Dominio, Aplicación, Infraestructura y presentación
(WebApi), con las dependencias apuntando siempre hacia adentro (WebApi →
Infrastructure/Application → Domain; Domain no conoce a nadie).
 
- **Dominio**: entidades, enums, y las estrategias de cálculo de cuota
  (`IInstallmentStrategy` + implementaciones Fixed/Decreasing). Sin
  dependencias externas.
- **Aplicación**: servicios que orquestan casos de uso, DTOs, validadores
  de FluentValidation, interfaces de repositorio y de Unit of Work.
- **Infraestructura**: implementación de repositorios, `DbContext`,
  `UnitOfWork`, seed data.
- **WebApi**: controladores, middleware de manejo de excepciones,
  configuración de Swagger/DI (composition root — el único punto que
  conoce todas las implementaciones concretas).

### Patrones de diseño utilizados

- **Repository Pattern**: Se utiliza para abstraer la lógica de acceso a datos y proporcionar una interfaz consistente para interactuar con la base de datos.

- **Unit of Work Pattern**: Se utiliza para agrupar operaciones de base de datos en una única transacción, garantizando la consistencia de los datos.

- **Dependency Injection**: Se utiliza para gestionar las dependencias entre los componentes del sistema, facilitando la inyección de servicios y promoviendo la modularidad y testabilidad del código.

- **Strategy Pattern**: Se utiliza para implementar diferentes estrategias de cálculo de cuotas de préstamos, permitiendo la selección dinámica de la estrategia adecuada según el tipo de préstamo.

- **Factory Pattern**: Se utiliza para crear instancias de estrategias de cálculo de cuotas de préstamos, encapsulando la lógica de creación y permitiendo la extensión del sistema con nuevas estrategias sin modificar el código existente.

- **Idempotency Pattern**: Se utiliza para garantizar que las operaciones de transacción sean idempotentes, evitando la duplicación de transacciones en caso de reintentos o fallos en la comunicación.

- **Result Pattern** — los servicios de aplicación (`LoanService`,
  `TransactionService`) devuelven `Result<T>` en vez de lanzar excepciones
  para los fallos esperables del negocio (validación, conflicto de estado,
  recurso no encontrado). Las excepciones quedan reservadas exclusivamente
  para lo genuinamente inesperado (fallas de infraestructura), que sigue
  cubriendo `ExceptionHandlingMiddleware` como red de seguridad.

---
## Decisiones de diseño

- **DTOs + FluentValidation** para separar validación de forma (rangos,
  campos requeridos) de las reglas de negocio que dependen del estado de
  la base de datos (máximo de préstamos activos, capacidad de pago), que
  viven en los servicios de aplicación y lanzan `BusinessRuleException`.
- **PostgreSQL** por transacciones y constraints reales, que es lo que
  sostiene la garantía de idempotencia ante condiciones de carrera.
- **Entity Framework Core**: Se eligió Entity Framework Core como 
  ORM para facilitar la interacción con la base de datos PostgreSQL, aprovechando sus características de mapeo objeto-relacional y soporte para migraciones.
- **Docker Compose** como bonus para desarrollo local.
- **`Result<T>` sobre excepciones para el flujo de negocio esperable**

### Trade-offs

Se tomaron algunas decisiones de diseño que implican ciertos trade-offs:

#### Arquitectura vs Velocidad de Desarrollo
Se implementó una arquitectura en capas con separación entre Dominio, Aplicación, Infraestructura y API. Esto incrementó el esfuerzo inicial de desarrollo, pero permitió mejorar la mantenibilidad, testabilidad y escalabilidad de la solución.

#### Seguridad vs Tiempo Disponible
La autenticación no formaba parte de los requisitos obligatorios de la prueba. Se decidió utilizar un UserId fijo para enfocar el desarrollo en la lógica financiera, la idempotencia y las reglas de negocio.

#### Simplicidad vs Consistencia de Datos
Se implementó una estrategia de idempotencia respaldada por restricciones únicas en la base de datos. Aunque esto agrega complejidad técnica, garantiza que una transacción no pueda procesarse dos veces.

#### Funcionalidad vs Calidad
Se priorizó la correcta implementación de los cálculos financieros, validaciones y pruebas automatizadas antes que incorporar funcionalidades complementarias como notificaciones, cache distribuido o procesamiento asíncrono.

---
## Testing

### Unit tests
- Cálculo de cuota fija (sistema francés).
- Generación de cronograma de pagos, incluyendo el caso de fecha de pago
  cuando el día base no existe en el mes destino (ej. día 31 → día 30).
- Validación de monto mínimo y máximo.
- Validación de plazo.

### Integration tests
- Deduplicación de transacciones con el mismo `idempotency_key`
  (incluyendo el caso de key vacía y el de keys distintas creando
  transacciones separadas).
- Persistencia de préstamos con su cronograma relacionado.
- Flujo de aprobación: transición de estado + creación de transacción de
  desembolso, verificando que ambas queden persistidas juntas.

## Limitaciones

- No se implementó autenticación.
- No se implementó cache distribuido.
- No se implementó paginación en los endpoints de listado.
- No hay test automatizado de la atomicidad transaccional ante fallos
  reales
- Los límites de negocio están duplicados con el frontend

## Mejoras Futuras

- JWT + Refresh Tokens.
- CQRS.
- Event Driven Architecture.
- Redis.
- Modular Monolith.
- Paginación en listados.
- Testcontainers para testear atomicidad transaccional real.

## Simplificaciones Realizadas

Para priorizar la entrega de una solución funcional y desplegada se realizaron las siguientes simplificaciones:

- `userId` fijo en lugar de autenticación.
- No se implementaron transferencias entre cuentas 
- TEA simulada por rango de monto, no calculada por un motor de scoring
  real.
- Sin procesamiento asíncrono de eventos.

---

Autor: Brayan Dennis Aguilar Aparicio