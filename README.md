# Intelectia

Ecosistema de escritorio académico para estudiantes universitarios - marketplace de libros, biblioteca personal, herramientas de estudio y grupos de estudio en tiempo real, construido sobre WPF/.NET 10 y ASP.NET Core 10 Clean Architecture.

![.NET 10](https://img.shields.io/badge/.NET-10-informational)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-informational)
![WPF](https://img.shields.io/badge/WPF-.NET%2010-informational)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

> **Status:** Desarrollo local - despliegue en la nube pendiente.
> Desarrollado por **Elias Ochoa**.

---

## Overview

Intelectia es un cliente de escritorio Windows que maneja la adquisición bibliográfica a través de un marketplace con pago integrado, gestión de documentos en una biblioteca personal, y herramientas de estudio activo (notas, citas, traducción). El cliente WPF se comunica exclusivamente con un backend ASP.NET Core 10 mediante HTTPS y WebSocket. Una sola cuenta de usuario puede ejercer simultáneamente los roles de Estudiante y Vendedor sin separación de sesión.

Los estudiantes universitarios operan habitualmente en herramientas desconectadas, sean pestañas de navegador para buscar bibliografía, carpetas locales para gestionar documentos y aplicaciones externas para coordinar grupos de estudio. Intelectia consolida estos flujos en un único cliente autenticado con acceso offline a la biblioteca descargada y colaboración en tiempo real a través de chat de grupo con historial persistente.

Las decisiones de ingeniería se basa en principios explícitos de Clean Architecture con inversión de dependencias estricta en las cuatro capas del backend; autenticación JWT stateless con rotación de refresh tokens persistida mediante Windows Credential Manager; pipeline CQRS con MediatR y FluentValidation que intercepta toda petición antes de ejecutar su handler; filtros globales de EF Core para soft delete en todas las entidades; ChatHub de SignalR con validación de membresía en servidor antes de autorizar la suscripción al canal; verificación de firma en webhook de Stripe vía `EventUtility.ConstructEvent`; ViewModels de WPF sin ninguna referencia al namespace de controles UI; sistema de iconos basado en geometrías SVG-path sin dependencia de librería de iconos externa.

---

## Tech Stack

- **WPF .NET 10** - cliente de escritorio Windows; bindings nativos, DataTemplates, ResourceDictionary para sistema de temas.
- **ASP.NET Core 10** - backend HTTP + WebSocket; middleware centralizado de excepciones, autenticación y autorización.
- **Clean Architecture** - separación en cuatro capas con dependencias unidireccionales; Domain no referencia ningún paquete externo.
- **Entity Framework Core 10 + SQL Server Express** - Code-First con migraciones; 8 migraciones aplicadas; GlobalQueryFilters para soft delete en todas las entidades derivadas de `BaseEntity`.
- **MediatR + CQRS** - cada caso de uso es un Command o Query con su Handler; `ValidationBehavior<TRequest, TResponse>` ejecuta FluentValidation antes de cualquier handler.
- **FluentValidation** - validadores declarativos registrados automáticamente; pipeline forzado en Application layer.
- **AutoMapper** - mapeos configurados por perfil; elimina conversión manual entre entidades y DTOs.
- **SignalR** - `ChatHub` con atributo `[Authorize]`; validación de membresía en base de datos antes de `Groups.AddToGroupAsync`.
- **JWT + Google OAuth 2.0** - tokens stateless; `ClockSkew = TimeSpan.Zero`; refresh token rotado en cada uso; Google OAuth con parámetro `state` para protección CSRF, listener local en `HttpListener`.
- **Stripe SDK** - flujo de checkout en Sandbox; monto en centavos calculado en servidor; webhook idempotente verificado por firma.
- **DeepL API** - traducción contextual dentro del módulo de herramientas de estudio; historial persistido en `TranslationHistory`.
- **Azure Blob Storage** - cliente configurado para almacenamiento de archivos PDF/ePub; despliegue en la nube pendiente.
- **Windows Credential Manager (AdysTech.CredentialManager)** - persiste refresh token cifrado en el almacén del sistema operativo; los fallos no interrumpen el arranque de la aplicación.
- **CommunityToolkit.Mvvm** - `ObservableObject`, `RelayCommand`, generación de código por source generator.
- **dotnet user-secrets** - único mecanismo de gestión de secretos en desarrollo; cero credenciales en el repositorio.

> No hay framework frontend. No hay JavaScript. No hay Docker (entorno local).
> Los secretos se gestionan exclusivamente vía `dotnet user-secrets`.

---

## Architecture

### Pattern

El backend sigue Clean Architecture con dependencias unidireccionales. **Domain** no depende de nada; **Application** depende de Domain; **Infrastructure** implementa las interfaces definidas en Domain y Application; **API** orquesta los tres proyectos anteriores. El cliente **WPF** referencia únicamente `Intelectia.Shared` (DTOs y enums compartidos) y no conoce ninguna capa del backend.

Cada caso de uso está implementado como un Command o Query de **MediatR**, con su Handler. El pipeline de comportamiento `ValidationBehavior<TRequest, TResponse>` ejecuta el validador de FluentValidation correspondiente antes de que el handler reciba la petición. Los controladores no contienen lógica de negocio.

### Solution Structure

```
Intelectia/
├── Intelectia.Domain/          — Entidades, Enums, Interfaces (sin dependencias externas)
├── Intelectia.Application/     — Casos de uso, DTOs, AutoMapper, FluentValidation, MediatR
├── Intelectia.Infrastructure/  — EF Core, Repositorios, Servicios externos (Stripe, DeepL, Azure)
├── Intelectia.API/             — Controllers, ChatHub, Middleware, Program.cs
├── Intelectia.WPF/             — ViewModels, Views, Services, Themes, Controls
└── Intelectia.Shared/          — DTOs y Enums compartidos entre API y WPF
```

- `Intelectia.Domain` - 19 entidades, interfaces de repositorio, enums de dominio. Sin paquetes externos.
- `Intelectia.Application` - 38+ handlers, perfiles de AutoMapper, validadores FluentValidation, interfaz `IApplicationDbContext`.
- `Intelectia.Infrastructure` - `AppDbContext` con GlobalQueryFilters, implementaciones de repositorios, `PaymentService`, `DeepLTranslationService`, `TokenService`, `DatabaseSeeder`.
- `Intelectia.API` - 10 controllers, 40+ endpoints, `ChatHub`, `GlobalExceptionMiddleware`, configuración JWT y SignalR en `Program.cs`.
- `Intelectia.WPF` - 14+ ViewModels con zero referencias a `System.Windows.Controls`, sistema de temas en ResourceDictionary, `ApiClient`, servicios de navegación e inyección de dependencias con `Microsoft.Extensions.DependencyInjection`.
- `Intelectia.Shared` - DTOs de request/response y enums utilizados tanto por el cliente como por la API.

### Security Implementation

- **JWT:** `ValidateLifetime = true`, `ValidateIssuerSigningKey = true`, `ValidateIssuer = true`, `ValidateAudience = true`, `ClockSkew = TimeSpan.Zero`. Configurado en `Program.cs`.
- **Stripe webhook:** Firma verificada en cada petición mediante `EventUtility.ConstructEvent(payload, header, webhookSecret)` antes de procesar el evento. Handler de confirmación idempotente.
- **SignalR ChatHub:** Atributo `[Authorize]` sobre la clase. Los métodos `JoinGroup` y `SendMessage` consultan `GroupMembers` en base de datos antes de suscribir la conexión o aceptar el mensaje.
- **Google OAuth CSRF:** Se genera un `state` aleatorio (`Guid`) en cada inicio de flujo; se valida contra el valor devuelto por el proveedor antes de intercambiar el código de autorización.
- **Soft delete:** `HasQueryFilter(e => !e.IsDeleted)` aplicado globalmente a todas las entidades que heredan de `BaseEntity` en `AppDbContext.OnModelCreating`. Los repositorios no repiten el filtro manualmente.
- **Gestión de secretos:** Todas las claves (JWT, Stripe, DeepL, Google, Azure, SMTP) se almacenan en `dotnet user-secrets`. El repositorio no contiene ningún valor sensible. `appsettings.Development.json` está en `.gitignore`.
- **Cliente WPF:** Los tokens JWT se mantienen en memoria durante la sesión. El refresh token se persiste en Windows Credential Manager con clave descriptiva única. Ninguna API key ni credential se incluye en el código fuente del cliente.

> `Microsoft.EntityFrameworkCore` está referenciado en `Intelectia.Application` a través de la interfaz `IApplicationDbContext` (que expone `DbSet<T>`). Esta dependencia está documentada como compromiso aceptado, eliminarla requiere reescribir 38+ handlers.

---

## Modules

- **Authentication** - Registro y login por email/contraseña, autenticación con Google OAuth 2.0, emisión y rotación de JWT, recuperación de contraseña por correo, persistencia de sesión vía Windows Credential Manager.
- **Marketplace** - Catálogo paginado con 48 libros en el seed inicial, búsqueda por texto, filtros por categoría, ordenamiento por precio, vista de detalle con reseñas de usuarios.
- **Cart & Orders** - Flujo de checkout completo con Stripe Sandbox, máquina de estados de pedido (Pendiente -> Procesando -> Enviado -> Entregado / Cancelado), historial de órdenes por usuario.
- **Personal Library** - Gestión de libros adquiridos y documentos subidos, seguimiento de progreso de lectura, acceso diferenciado por formato de archivo.
- **Study Tools** - Notas al margen vinculadas a libros, gestor de citas bibliográficas en formatos APA, MLA, Chicago e IEEE, traducción contextual vía DeepL con historial persistido.
- **Study Groups** - Creación y exploración de grupos, chat en tiempo real con SignalR, historial de mensajes paginado, validación de membresía en servidor.
- **Vendor Panel** - Activación de perfil de vendedor desde cuenta existente, publicación y gestión de libros, estadísticas de ventas.
- **Profile & Settings** - Edición de datos personales, cambio de contraseña, gestión de sesión activa y cierre de sesión.

---

## Local Development

### Prerequisites

- .NET 10 SDK
- Visual Studio 2022 (v17.8 o superior)
- SQL Server Express o LocalDB (incluido con Visual Studio)
- Git

### Installation

**1. Clonar el repositorio**

```bash
git clone https://github.com/<tu-usuario>/Intelectia.git
cd Intelectia
```

**2. Abrir la solución**

Abrir `Intelectia.slnx` en Visual Studio 2022.

**3. Configurar user-secrets para `Intelectia.API`**

```bash
cd Intelectia.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "<valor>"
dotnet user-secrets set "JwtSettings:Secret" "<valor>"
dotnet user-secrets set "ExternalServices:Stripe:SecretKey" "<valor>"
dotnet user-secrets set "ExternalServices:Stripe:WebhookSecret" "<valor>"
dotnet user-secrets set "ExternalServices:Google:ClientId" "<valor>"
dotnet user-secrets set "ExternalServices:Google:ClientSecret" "<valor>"
dotnet user-secrets set "ExternalServices:DeepL:ApiKey" "<valor>"
dotnet user-secrets set "ExternalServices:Email:SmtpHost" "<valor>"
dotnet user-secrets set "ExternalServices:Email:SmtpUser" "<valor>"
dotnet user-secrets set "ExternalServices:Email:SmtpPassword" "<valor>"
dotnet user-secrets set "ExternalServices:Azure:BlobConnectionString" "<valor>"
```

**4. Aplicar migraciones**

```bash
cd Intelectia.Infrastructure
dotnet ef database update --startup-project ../Intelectia.API
```

**5. Ejecutar el backend**

```bash
cd Intelectia.API
dotnet run
```

**6. Ejecutar el cliente WPF**

Establecer `Intelectia.WPF` como proyecto de inicio en Visual Studio y presionar `F5`, o:

```bash
cd Intelectia.WPF
dotnet run
```

### Stripe Webhook (local)

Para probar el webhook de Stripe en entorno local:

```bash
stripe listen --forward-to https://localhost:5028/api/payments/webhook
```

### Google OAuth

El URI de redirección debe estar registrado en Google Cloud Console como:

```
http://localhost:5100/auth/callback
```

---

## Roadmap

- [ ] Despliegue en la nube (Azure App Service o Railway)
- [ ] Visor de archivos PDF/ePub integrado (PdfiumViewer o equivalente)
- [ ] Suite de pruebas automatizadas (unitarias e integración)
- [ ] Verificación de correo electrónico en el registro
- [ ] Carga activa de archivos en Azure Blob Storage (cliente configurado, flujo no ejercido)
- [ ] Migración de Stripe a modo producción

---

## License

Distribuido bajo la Licencia MIT. Ver `LICENSE` para más detalles.
