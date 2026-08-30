# SGIP - Sistema de Gestión de Inversiones y Préstamos

Api Restful para la gestión de préstamos e inversiones con simulación y solicitud de préstamos con cálculos financieros
procesamiento de transacciones con garantías de idempotencia, flujo de aprobación de préstamos.

## Funcionalidades Implementadas

### Gestión de Préstamos

- Simulación de préstamos utilizando Sistema Francés y Sistema Alemán.
- Generación automática de cronograma de pagos.
- Solicitud de préstamos.
- Consulta de préstamos por identificador.
- Listado de préstamos.

### Gestión de Transacciones

- Registro de transacciones.
- Garantía de idempotencia mediante IdempotencyKey.
- Prevención de transacciones duplicadas.
- Consulta y listado de transacciones.

### Reglas de Negocio

- Validación de monto mínimo y máximo.
- Validación de plazo permitido.
- Validación de máximo de préstamos activos por cliente.
- Validación de capacidad de pago.

---

## Links Remotas (Producción)

| Link | URl |
|------|-----|
| Swagger |  |
| Api |  |

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
---

## Estructura del proyecto

```
src/
├── Sgip.Application/          # Capa de aplicación con servicios, DTOs y validaciones
├── Sgip.Domain/               # Capa de dominio con entidades y lógica de negocio
├── Sgip.Infrastructure/       # Capa de infraestructura con repositorios y contexto de base de datos
├── Sgip.WebApi/               # Capa de presentación con controladores y configuración de la API

tests/
├── Sgip.IntegrationTests/      # Pruebas de integración
├── Sgip.UnitTests/             # Pruebas unitarias
```
---
## Arquitectura

Se ha implementado una arquitectura basada en capas, siguiendo los principios de DDD (Domain-Driven Design) no estrictamente, pero con una separación clara de responsabilidades entre las capas de aplicación, dominio e infraestructura. Esto permite una mayor flexibilidad y mantenibilidad del código, así como una mejor organización de los componentes del sistema.

Se separaron en las siguientes capas:

- **Capa de Aplicación**: Contiene los servicios de aplicación, DTOs y validaciones. Esta capa se encarga de orquestar la lógica de negocio y coordinar las operaciones entre las diferentes capas.

- **Capa de Dominio**: Contiene las entidades del dominio y la lógica de negocio. Esta capa representa el núcleo del sistema y encapsula las reglas de negocio y comportamientos específicos del dominio.

- **Capa de Infraestructura**: Contiene los repositorios y el contexto de base de datos. Esta capa se encarga de la persistencia de datos y la interacción con la base de datos PostgreSQL.

- **Capa de Presentación**: Contiene los controladores y la configuración de la API. Esta capa expone los endpoints RESTful y maneja las solicitudes y respuestas HTTP.

### Patrones de diseño utilizados

- **Repository Pattern**: Se utiliza para abstraer la lógica de acceso a datos y proporcionar una interfaz consistente para interactuar con la base de datos.

- **Unit of Work Pattern**: Se utiliza para agrupar operaciones de base de datos en una única transacción, garantizando la consistencia de los datos.

- **Dependency Injection**: Se utiliza para gestionar las dependencias entre los componentes del sistema, facilitando la inyección de servicios y promoviendo la modularidad y testabilidad del código.

- **Strategy Pattern**: Se utiliza para implementar diferentes estrategias de cálculo de cuotas de préstamos, permitiendo la selección dinámica de la estrategia adecuada según el tipo de préstamo.

- **Factory Pattern**: Se utiliza para crear instancias de estrategias de cálculo de cuotas de préstamos, encapsulando la lógica de creación y permitiendo la extensión del sistema con nuevas estrategias sin modificar el código existente.

- **Idempotency Pattern**: Se utiliza para garantizar que las operaciones de transacción sean idempotentes, evitando la duplicación de transacciones en caso de reintentos o fallos en la comunicación.

---
## Decisiones de diseño

Al diseñar la arquitectura del sistema, se tomaron varias decisiones clave para garantizar la escalabilidad, mantenibilidad y robustez del sistema:


- **Uso de patrones de diseño**: Se implementaron varios patrones de diseño, como Repository, Unit of Work, Strategy, Factory e Idempotency, para abordar problemas comunes en el desarrollo de software y mejorar la calidad del código.

- **Uso de DTOs y validaciones**: Se utilizaron DTOs para transferir datos entre las capas del sistema y se implementaron validaciones utilizando FluentValidation para garantizar la integridad de los datos.

- **Uso de Entity Framework Core**: Se eligió Entity Framework Core como ORM para facilitar la interacción con la base de datos PostgreSQL, aprovechando sus características de mapeo objeto-relacional y soporte para migraciones.

- **Uso de PostgreSQL**: Se eligió PostgreSQL como base de datos por su robustez, escalabilidad y soporte para características avanzadas como transacciones y concurrencia.

- **Uso de Docker y Docker Compose**: Se decidió utilizar Docker y Docker Compose para facilitar el despliegue y la gestión del entorno de desarrollo, permitiendo la creación de contenedores aislados para la aplicación y la base de datos.

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


## Limitaciones

- No se implementó autenticación.
- No se implementaron notificaciones.
- No se implementó cache distribuido.
- No se implementó paginación en los endpoints de listado.

## Mejoras Futuras

- JWT + Refresh Tokens.
- CQRS.
- Event Driven Architecture.
- Redis.
- Modular Monolith.

## Simplificaciones Realizadas

Para priorizar la entrega de una solución funcional y desplegada se realizaron las siguientes simplificaciones:

- Se utilizó un UserId fijo en lugar de implementar autenticación.
- No se implementaron transferencias entre cuentas.
- Se utilizó una simulación para obtener la tasa de interés.
- No se implementó procesamiento asíncrono de eventos.

---
Pruebas implementadas:

- Cálculo de cuota fija.
- Generación de número de cuotas.
- Validación de monto mínimo.
- Validación de monto máximo.
- Validación de plazo.
- Validación de idempotencia.
- Creación de préstamos.

---

Autor: Brayan Dennis Aguilar Aparicio