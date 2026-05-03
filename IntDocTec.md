# Intelectia — Documentación Técnica

> Ecosistema académico unificado: Marketplace de libros + Hub de gestión personal  
> Stack: WPF .NET 8 · ASP.NET Core Web API · SignalR · EF Core · SQL Server Express · JWT · Google OAuth · Stripe · DeepL · Azure Blob Storage

---

## Índice

1. [Visión General del Sistema](#1-visión-general-del-sistema)
2. [Stack Tecnológico](#2-stack-tecnológico)
3. [Arquitectura del Sistema](#3-arquitectura-del-sistema)
4. [Estructura de la Solución](#4-estructura-de-la-solución)
5. [Modelo de Dominio y Base de Datos](#5-modelo-de-dominio-y-base-de-datos)
6. [Contratos de API](#6-contratos-de-api)
7. [Arquitectura en Tiempo Real — SignalR](#7-arquitectura-en-tiempo-real--signalr)
8. [Modo Offline](#8-modo-offline)
9. [Integraciones Externas](#9-integraciones-externas)
10. [Sistema de Diseño UI](#10-sistema-de-diseño-ui)
11. [Seguridad y Autenticación](#11-seguridad-y-autenticación)
12. [Roadmap de Construcción por Fases](#12-roadmap-de-construcción-por-fases)

---

## 1. Visión General del Sistema

Intelectia es un ecosistema de escritorio para el ámbito académico universitario que unifica en una sola interfaz tres flujos de trabajo que actualmente operan de forma fragmentada: la adquisición de materiales bibliográficos, la gestión personal de documentos y las herramientas de estudio activo.

### Módulos funcionales

| Módulo | Descripción |
|---|---|
| **Autenticación** | Registro, login por email/contraseña y Google OAuth. Recuperación de contraseña por correo. |
| **Marketplace** | Catálogo de libros (físicos y digitales) con búsqueda, filtros, detalle, reseñas y comparación de precios. |
| **Carrito y Pedidos** | Flujo de compra completo con gestión de direcciones, pago vía Stripe y ciclo de vida de pedidos (Pendiente → Procesando → Enviado → Entregado / Cancelado). |
| **Biblioteca Personal** | Gestión de libros adquiridos y documentos subidos por el usuario. Visor con acceso offline para contenido descargado. |
| **Herramientas de Estudio** | Notas al margen, resaltado, gestor de citas bibliográficas (APA, MLA, Chicago, IEEE), traducción contextual vía DeepL. |
| **Grupos de Estudio** | Creación, exploración y pertenencia a grupos. Chat en tiempo real vía SignalR. |
| **Panel de Vendedor** | Publicación de libros, gestión de inventario, historial y estadísticas de ventas. |
| **Perfil y Ajustes** | Datos personales, foto de perfil, seguridad, direcciones de envío y métodos de pago. |

### Principio de roles no excluyentes

Un usuario puede actuar simultáneamente como Estudiante y como Vendedor. No hay separación de cuentas. El sistema detecta qué perfil tiene activo cada usuario y habilita los módulos correspondientes. Un Estudiante puede convertirse en Vendedor desde su perfil en cualquier momento sin crear una cuenta nueva.

---

## 2. Stack Tecnológico

| Capa | Tecnología | Justificación |
|---|---|---|
| Cliente | WPF / .NET 8 | Escritorio Windows nativo, soporte a largo plazo, MVVM maduro |
| Patrón cliente | MVVM | Separación limpia View/ViewModel/Model, testeable |
| Backend | ASP.NET Core 8 Web API | Ecosistema Microsoft coherente, rendimiento, soporte nativo a EF Core y SignalR |
| Tiempo real | SignalR (ASP.NET Core) | Hub de chat integrado en el mismo proceso del API |
| ORM | Entity Framework Core 8 | Code-first migrations, LINQ, relación natural con SQL Server |
| Base de datos | SQL Server Express | Gratuito, integración nativa con Visual Studio, estándar enterprise .NET |
| Autenticación | JWT + Google OAuth 2.0 | JWT para sesiones stateless, Google OAuth para login social |
| Pagos | Stripe SDK (Sandbox) | Estándar de la industria, sandbox gratuito completo |
| Traducción | DeepL API Free | 500k caracteres/mes, calidad superior a Google Translate |
| Almacenamiento | Azure Blob Storage | Capa gratuita 5GB/12 meses, escalable, CDN-ready |
| IDE | Visual Studio 2022 | Soporte completo para todo el stack |

---

## 3. Arquitectura del Sistema

### 3.1 Diagrama de comunicación general

```
┌─────────────────────────────────────────────────┐
│                 CLIENTE WPF                      │
│  ┌────────────┐  ┌──────────────────────────┐   │
│  │  Views     │  │      ViewModels           │   │
│  │  (XAML)    │◄─┤  (lógica de presentación) │   │
│  └────────────┘  └──────────┬───────────────┘   │
│                             │                    │
│  ┌──────────────────────────▼───────────────┐   │
│  │           Services Layer                  │   │
│  │  ApiClient · AuthService · CacheService   │   │
│  │  OfflineService · SignalRService          │   │
│  └──────────────────────────┬───────────────┘   │
└─────────────────────────────┼───────────────────┘
                              │ HTTPS / WSS
┌─────────────────────────────▼───────────────────┐
│              BACKEND — ASP.NET Core 8            │
│                                                  │
│  ┌───────────┐  ┌────────────────────────────┐  │
│  │Controllers│  │       SignalR Hubs          │  │
│  │  (API)    │  │   ChatHub · NotifHub        │  │
│  └─────┬─────┘  └────────────┬───────────────┘  │
│        │                     │                   │
│  ┌─────▼─────────────────────▼───────────────┐  │
│  │           Application Layer                │  │
│  │   Use Cases · Services · DTOs · Interfaces │  │
│  └─────────────────┬─────────────────────────┘  │
│                    │                             │
│  ┌─────────────────▼─────────────────────────┐  │
│  │         Infrastructure Layer              │  │
│  │  EF Core · Repos · BlobService            │  │
│  │  StripeService · DeepLService             │  │
│  │  EmailService · GoogleAuthService         │  │
│  └─────────────────┬─────────────────────────┘  │
└────────────────────┼────────────────────────────┘
                     │
        ┌────────────┴──────────────┐
        │                           │
┌───────▼────────┐     ┌────────────▼──────────┐
│  SQL Server    │     │   Azure Blob Storage   │
│  Express       │     │  (PDFs, EPUBs, covers) │
└────────────────┘     └───────────────────────┘
                     + DeepL API · Stripe API · Google OAuth
```

### 3.2 Arquitectura del Backend — Clean Architecture

El backend sigue los principios de Clean Architecture con cuatro capas con dependencias unidireccionales hacia el interior:

```
API ──► Application ──► Domain
 └──► Infrastructure ──► Application
```

**Domain** — Sin dependencias externas.
Contiene entidades de negocio, value objects, enums y las interfaces de repositorio que el resto del sistema debe implementar. No sabe nada de EF Core, HTTP ni bases de datos.

**Application** — Depende solo de Domain.
Contiene los casos de uso (Use Cases / Commands / Queries), DTOs, interfaces de servicios externos (IStripeService, IDeepLService, IBlobService) y la lógica de negocio orquestada. Si se usa MediatR para CQRS, los Handlers viven aquí.

**Infrastructure** — Implementa las interfaces de Application y Domain.
Aquí vive EF Core (DbContext, configuraciones Fluent API, migraciones), las implementaciones de repositorios, los clientes HTTP para Stripe, DeepL, Google y Azure Blob.

**API** — Punto de entrada.
Controllers REST, SignalR Hubs, Middleware (JWT, excepciones globales, rate limiting), configuración de DI en Program.cs.

### 3.3 Arquitectura del Cliente WPF — MVVM

```
Views (XAML)
  │  DataBinding / Commands
  ▼
ViewModels
  │  Llama a
  ▼
Services (ApiClient, AuthService, SignalRService, CacheService)
  │  HTTP / WebSocket
  ▼
Backend API
```

Cada pantalla principal tiene un ViewModel dedicado. Los ViewModels no conocen ni referencian controles UI. Los datos fluyen únicamente a través de bindings y comandos (ICommand / RelayCommand).

---

## 4. Estructura de la Solución

```
Intelectia/
│
├── Intelectia.sln
│
├── src/
│   │
│   ├── Backend/
│   │   │
│   │   ├── Intelectia.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── StudentProfile.cs
│   │   │   │   ├── VendorProfile.cs
│   │   │   │   ├── Book.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── UserBook.cs
│   │   │   │   ├── Document.cs
│   │   │   │   ├── Order.cs
│   │   │   │   ├── OrderDetail.cs
│   │   │   │   ├── Address.cs
│   │   │   │   ├── PaymentMethod.cs
│   │   │   │   ├── StudyGroup.cs
│   │   │   │   ├── GroupMember.cs
│   │   │   │   ├── GroupMessage.cs
│   │   │   │   ├── Note.cs
│   │   │   │   ├── Citation.cs
│   │   │   │   └── SavedTranslation.cs
│   │   │   ├── Enums/
│   │   │   │   ├── BookFormat.cs
│   │   │   │   ├── BookStatus.cs
│   │   │   │   ├── OrderStatus.cs
│   │   │   │   ├── CitationStyle.cs
│   │   │   │   └── GroupMemberRole.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── Repositories/
│   │   │   │   │   ├── IUserRepository.cs
│   │   │   │   │   ├── IBookRepository.cs
│   │   │   │   │   ├── IOrderRepository.cs
│   │   │   │   │   ├── IGroupRepository.cs
│   │   │   │   │   └── IUnitOfWork.cs
│   │   │   │   └── Services/
│   │   │   │       ├── IStripeService.cs
│   │   │   │       ├── IDeepLService.cs
│   │   │   │       ├── IBlobService.cs
│   │   │   │       └── IEmailService.cs
│   │   │   └── Intelectia.Domain.csproj
│   │   │
│   │   ├── Intelectia.Application/
│   │   │   ├── UseCases/
│   │   │   │   ├── Auth/
│   │   │   │   ├── Books/
│   │   │   │   ├── Library/
│   │   │   │   ├── Orders/
│   │   │   │   ├── Groups/
│   │   │   │   ├── Translation/
│   │   │   │   ├── Notes/
│   │   │   │   ├── Citations/
│   │   │   │   └── Vendors/
│   │   │   ├── DTOs/
│   │   │   │   ├── Auth/
│   │   │   │   ├── Books/
│   │   │   │   ├── Orders/
│   │   │   │   └── ...
│   │   │   ├── Mappings/          ← AutoMapper Profiles
│   │   │   ├── Validators/        ← FluentValidation
│   │   │   └── Intelectia.Application.csproj
│   │   │
│   │   ├── Intelectia.Infrastructure/
│   │   │   ├── Persistence/
│   │   │   │   ├── IntelectiaDbContext.cs
│   │   │   │   ├── Configurations/    ← Fluent API por entidad
│   │   │   │   ├── Repositories/
│   │   │   │   └── Migrations/
│   │   │   ├── ExternalServices/
│   │   │   │   ├── StripeService.cs
│   │   │   │   ├── DeepLService.cs
│   │   │   │   ├── BlobService.cs
│   │   │   │   ├── GoogleAuthService.cs
│   │   │   │   └── EmailService.cs
│   │   │   └── Intelectia.Infrastructure.csproj
│   │   │
│   │   └── Intelectia.API/
│   │       ├── Controllers/
│   │       │   ├── AuthController.cs
│   │       │   ├── UsersController.cs
│   │       │   ├── BooksController.cs
│   │       │   ├── LibraryController.cs
│   │       │   ├── OrdersController.cs
│   │       │   ├── PaymentsController.cs
│   │       │   ├── GroupsController.cs
│   │       │   ├── TranslationController.cs
│   │       │   ├── NotesController.cs
│   │       │   ├── CitationsController.cs
│   │       │   └── VendorsController.cs
│   │       ├── Hubs/
│   │       │   └── ChatHub.cs
│   │       ├── Middleware/
│   │       │   ├── ExceptionMiddleware.cs
│   │       │   └── RequestLoggingMiddleware.cs
│   │       ├── Extensions/          ← ServiceCollection extensions
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       └── Intelectia.API.csproj
│   │
│   └── Client/
│       └── Intelectia.WPF/
│           ├── Core/
│           │   ├── BaseViewModel.cs       ← INotifyPropertyChanged base
│           │   ├── RelayCommand.cs        ← ICommand implementation
│           │   ├── AsyncRelayCommand.cs
│           │   └── NavigationService.cs
│           ├── Services/
│           │   ├── ApiClient.cs           ← HttpClient wrapper
│           │   ├── AuthService.cs         ← JWT storage, refresh
│           │   ├── SignalRService.cs       ← Hub connection manager
│           │   ├── CacheService.cs        ← In-memory + disk cache
│           │   └── OfflineService.cs      ← Local file management
│           ├── ViewModels/
│           │   ├── Auth/
│           │   │   ├── LoginViewModel.cs
│           │   │   ├── RegisterViewModel.cs
│           │   │   └── RecoverViewModel.cs
│           │   ├── Dashboard/
│           │   │   └── DashboardViewModel.cs
│           │   ├── Library/
│           │   │   ├── LibraryViewModel.cs
│           │   │   ├── DocumentViewerViewModel.cs
│           │   │   └── BookViewerViewModel.cs
│           │   ├── Marketplace/
│           │   │   ├── MarketplaceViewModel.cs
│           │   │   ├── BookDetailViewModel.cs
│           │   │   └── CartViewModel.cs
│           │   ├── Orders/
│           │   │   ├── CheckoutViewModel.cs
│           │   │   └── OrderHistoryViewModel.cs
│           │   ├── Groups/
│           │   │   ├── GroupsViewModel.cs
│           │   │   ├── GroupDetailViewModel.cs
│           │   │   └── ChatViewModel.cs
│           │   ├── Vendor/
│           │   │   ├── VendorDashboardViewModel.cs
│           │   │   ├── PublishBookViewModel.cs
│           │   │   └── SalesViewModel.cs
│           │   └── Profile/
│           │       ├── ProfileViewModel.cs
│           │       ├── SecurityViewModel.cs
│           │       ├── AddressesViewModel.cs
│           │       └── PaymentMethodsViewModel.cs
│           ├── Views/
│           │   ├── Auth/
│           │   ├── Dashboard/
│           │   ├── Library/
│           │   ├── Marketplace/
│           │   ├── Orders/
│           │   ├── Groups/
│           │   ├── Vendor/
│           │   └── Profile/
│           ├── Controls/              ← Custom WPF Controls reutilizables
│           │   ├── BookCard.xaml
│           │   ├── ProductCard.xaml
│           │   ├── ReadingCard.xaml
│           │   ├── ChatMessage.xaml
│           │   ├── GroupCard.xaml
│           │   ├── RoundedButton.xaml
│           │   ├── SearchBox.xaml
│           │   └── StarRating.xaml
│           ├── Converters/
│           │   ├── BoolToVisibilityConverter.cs
│           │   ├── NullToVisibilityConverter.cs
│           │   ├── PriceFormatter.cs
│           │   └── DateTimeFormatter.cs
│           ├── Themes/
│           │   ├── Colors.xaml
│           │   ├── Typography.xaml
│           │   ├── Buttons.xaml
│           │   ├── Inputs.xaml
│           │   ├── Cards.xaml
│           │   └── App.xaml           ← Merge de todos los ResourceDictionaries
│           └── App.xaml
│
└── shared/
    └── Intelectia.Shared/             ← DTOs y contratos compartidos (opcional)
        └── Intelectia.Shared.csproj
```

---

## 5. Modelo de Dominio y Base de Datos

### 5.1 Decisiones de diseño previas al schema

**Roles simultáneos:** Se implementa con una tabla `UserRoles` many-to-many y perfiles extendidos opcionales (`StudentProfile`, `VendorProfile`). Un usuario puede tener ambos perfiles. EF Core navegará a ellos con propiedades de navegación nullable.

**Herencia:** Table Per Hierarchy (TPH) descartado. Se usa composición: `User` como entidad base con propiedades de perfil en tablas separadas ligadas por FK 1-a-1. Más limpio para consultas.

**Archivos:** Las rutas físicas (PDFs, EPUBs, portadas) nunca se almacenan en la base de datos como rutas locales. Solo se guarda la URL del blob en Azure. El cliente WPF gestiona la ruta local en caché de forma independiente.

**Soft delete:** Todas las entidades principales tendrán un campo `IsDeleted` + `DeletedAt` para no destruir referencias en pedidos o histórico.

### 5.2 Entidades del Dominio

```csharp
// ─── User ───────────────────────────────────────────────
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PasswordHash { get; set; }   // null si solo Google OAuth
    public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; }       // Azure Blob URL
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navegación
    public StudentProfile? StudentProfile { get; set; }
    public VendorProfile? VendorProfile { get; set; }
    public ICollection<Address> Addresses { get; set; }
    public ICollection<PaymentMethod> PaymentMethods { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<UserBook> Library { get; set; }
    public ICollection<GroupMember> GroupMemberships { get; set; }
    public ICollection<Note> Notes { get; set; }
    public ICollection<Citation> Citations { get; set; }
}

// ─── StudentProfile ──────────────────────────────────────
public class StudentProfile
{
    public Guid UserId { get; set; }
    public string? Career { get; set; }
    public int? Semester { get; set; }
    public string? Institution { get; set; }
    public User User { get; set; }
}

// ─── VendorProfile ───────────────────────────────────────
public class VendorProfile
{
    public Guid UserId { get; set; }
    public decimal Reputation { get; set; }      // 0.0 - 5.0
    public int TotalSales { get; set; }
    public string? Phone { get; set; }
    public string? StripeAccountId { get; set; }
    public User User { get; set; }
    public ICollection<Book> Books { get; set; }
}

// ─── Book ─────────────────────────────────────────────────
public class Book
{
    public Guid Id { get; set; }
    public string? ISBN { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string? Editorial { get; set; }
    public string? Edition { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }         // Azure Blob URL
    public string? FileUrl { get; set; }          // Azure Blob URL (digital only)
    public decimal Price { get; set; }
    public BookFormat Format { get; set; }        // Physical | Digital | AudioBook
    public BookStatus Status { get; set; }        // New | Used
    public int Stock { get; set; }                // 0 para ilimitado (digital)
    public bool IsAvailable { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime PublishedAt { get; set; }
    public Guid VendorId { get; set; }
    public Guid CategoryId { get; set; }

    // Navegación
    public VendorProfile Vendor { get; set; }
    public Category Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
    public ICollection<UserBook> UserLibraries { get; set; }
}

// ─── Category ─────────────────────────────────────────────
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<Book> Books { get; set; }
}

// ─── UserBook (Biblioteca Personal) ──────────────────────
public class UserBook
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public int ProgressPercent { get; set; }      // 0 - 100
    public bool IsDownloaded { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? LastOpenedAt { get; set; }

    public User User { get; set; }
    public Book Book { get; set; }
}

// ─── Document (Archivos personales del usuario) ───────────
public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string? Author { get; set; }
    public string FileUrl { get; set; }           // Azure Blob URL
    public long SizeBytes { get; set; }
    public string Format { get; set; }            // PDF | EPUB
    public bool IsPrivate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UploadedAt { get; set; }

    public User User { get; set; }
    public ICollection<Note> Notes { get; set; }
    public ICollection<Citation> Citations { get; set; }
}

// ─── Order ────────────────────────────────────────────────
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }       // Pending | Processing | Shipped | Delivered | Cancelled
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? ShippingAddress { get; set; }  // JSON snapshot al momento de la compra
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; }
    public ICollection<OrderDetail> Details { get; set; }
}

// ─── OrderDetail ──────────────────────────────────────────
public class OrderDetail
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }        // Snapshot del precio al momento de compra
    public BookFormat Format { get; set; }

    public Order Order { get; set; }
    public Book Book { get; set; }
}

// ─── Address ──────────────────────────────────────────────
public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string? Phone { get; set; }
    public bool IsDefault { get; set; }

    public User User { get; set; }
}

// ─── PaymentMethod ────────────────────────────────────────
public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StripePaymentMethodId { get; set; }
    public string Last4 { get; set; }
    public string Brand { get; set; }             // Visa | Mastercard | etc.
    public string ExpiryMonth { get; set; }
    public string ExpiryYear { get; set; }
    public bool IsDefault { get; set; }

    public User User { get; set; }
}

// ─── StudyGroup ───────────────────────────────────────────
public class StudyGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsPrivate { get; set; }
    public int MaxMembers { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; }
    public ICollection<GroupMember> Members { get; set; }
    public ICollection<GroupMessage> Messages { get; set; }
}

// ─── GroupMember ──────────────────────────────────────────
public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public GroupMemberRole Role { get; set; }     // Admin | Member
    public DateTime JoinedAt { get; set; }

    public StudyGroup Group { get; set; }
    public User User { get; set; }
}

// ─── GroupMessage ─────────────────────────────────────────
public class GroupMessage
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }

    public StudyGroup Group { get; set; }
    public User Sender { get; set; }
}

// ─── Note ─────────────────────────────────────────────────
public class Note
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? BookId { get; set; }
    public string Content { get; set; }
    public int? PageNumber { get; set; }
    public string? Color { get; set; }            // Hex color del resaltado
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; }
    public Document? Document { get; set; }
}

// ─── Citation ─────────────────────────────────────────────
public class Citation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? DocumentId { get; set; }
    public Guid? BookId { get; set; }
    public string OriginalText { get; set; }
    public CitationStyle Style { get; set; }      // APA | MLA | Chicago | IEEE
    public string GeneratedReference { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}

// ─── SavedTranslation ─────────────────────────────────────
public class SavedTranslation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OriginalText { get; set; }
    public string TranslatedText { get; set; }
    public string SourceLanguage { get; set; }
    public string TargetLanguage { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}
```

### 5.3 Enums

```csharp
public enum BookFormat    { Physical, Digital, AudioBook }
public enum BookStatus    { New, Used }
public enum OrderStatus   { Pending, Processing, Shipped, Delivered, Cancelled }
public enum CitationStyle { APA, MLA, Chicago, IEEE }
public enum GroupMemberRole { Admin, Member }
```

---

## 6. Contratos de API

Todos los endpoints requieren `Authorization: Bearer {token}` salvo que se indique `[Público]`.  
Base URL: `https://localhost:{port}/api`

### 6.1 Autenticación

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| POST | `/auth/register` | Registro con email y contraseña | Público |
| POST | `/auth/login` | Login con email y contraseña | Público |
| POST | `/auth/google` | Login/registro con Google OAuth code | Público |
| POST | `/auth/refresh` | Renovar access token con refresh token | Público |
| POST | `/auth/logout` | Invalidar refresh token | Requerida |
| POST | `/auth/forgot-password` | Enviar correo de recuperación | Público |
| POST | `/auth/reset-password` | Restablecer contraseña con token del correo | Público |

**POST /auth/register — Request:**
```json
{
  "fullName": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "password": "MiContraseña123!"
}
```
**Response 201:**
```json
{
  "message": "Cuenta creada. Por favor verifica tu correo electrónico."
}
```

**POST /auth/login — Request:**
```json
{
  "email": "juan@ejemplo.com",
  "password": "MiContraseña123!"
}
```
**Response 200:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "fullName": "Juan Pérez",
    "email": "juan@ejemplo.com",
    "avatarUrl": null,
    "isVendor": false,
    "isStudent": true
  }
}
```

### 6.2 Usuarios y Perfil

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/users/me` | Obtener perfil completo del usuario autenticado |
| PUT | `/users/me` | Actualizar nombre y foto de perfil |
| PUT | `/users/me/password` | Cambiar contraseña |
| GET | `/users/me/student-profile` | Obtener perfil de estudiante |
| PUT | `/users/me/student-profile` | Crear o actualizar perfil de estudiante |
| POST | `/users/me/become-vendor` | Activar rol de vendedor |
| GET | `/users/me/addresses` | Listar direcciones de envío |
| POST | `/users/me/addresses` | Agregar dirección |
| PUT | `/users/me/addresses/{id}` | Editar dirección |
| DELETE | `/users/me/addresses/{id}` | Eliminar dirección |
| GET | `/users/me/payment-methods` | Listar métodos de pago |
| POST | `/users/me/payment-methods` | Agregar método de pago (Stripe) |
| DELETE | `/users/me/payment-methods/{id}` | Eliminar método de pago |

### 6.3 Catálogo de Libros

| Método | Ruta | Descripción | Auth |
|---|---|---|---|
| GET | `/books` | Listar libros con paginación y filtros | Público |
| GET | `/books/{id}` | Detalle de un libro | Público |
| GET | `/books/categories` | Listar todas las categorías | Público |
| GET | `/books/{id}/reviews` | Reseñas de un libro | Público |
| POST | `/books/{id}/reviews` | Publicar reseña | Requerida |

**GET /books — Query Params:**
```
?search=calculo
&categoryId=guid
&format=Digital          (Physical|Digital|AudioBook)
&status=New              (New|Used)
&minPrice=10
&maxPrice=50
&page=1
&pageSize=12
&sortBy=price            (price|title|date|rating)
&sortDir=asc
```

**Response 200:**
```json
{
  "items": [
    {
      "id": "guid",
      "title": "Cálculo de una Variable",
      "author": "James Stewart",
      "coverUrl": "https://blob.azure.com/...",
      "price": 11.99,
      "format": "Digital",
      "status": "New",
      "rating": 4.8,
      "reviewCount": 110,
      "vendorName": "Editorial Académica"
    }
  ],
  "totalCount": 48,
  "page": 1,
  "pageSize": 12,
  "totalPages": 4
}
```

### 6.4 Biblioteca Personal

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/library/books` | Listar libros adquiridos en la biblioteca |
| GET | `/library/books/{bookId}/download-url` | Obtener URL firmada temporal de Azure Blob |
| PUT | `/library/books/{bookId}/progress` | Actualizar progreso de lectura |
| GET | `/library/documents` | Listar documentos personales subidos |
| POST | `/library/documents` | Subir documento (multipart/form-data) |
| GET | `/library/documents/{id}/download-url` | Obtener URL firmada temporal |
| DELETE | `/library/documents/{id}` | Eliminar documento personal |

### 6.5 Pedidos y Pagos

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/orders` | Historial de pedidos del usuario |
| GET | `/orders/{id}` | Detalle de un pedido |
| POST | `/orders/{id}/cancel` | Cancelar pedido (si aplica) |
| POST | `/payments/create-intent` | Crear PaymentIntent en Stripe |
| POST | `/payments/confirm` | Confirmar pago y generar pedido |
| GET | `/payments/receipt/{orderId}` | Generar recibo del pedido |

**POST /payments/create-intent — Request:**
```json
{
  "items": [
    { "bookId": "guid", "quantity": 1, "format": "Digital" }
  ],
  "addressId": "guid",
  "paymentMethodId": "guid",
  "discountCode": "ESTUDIANTE25"
}
```
**Response 200:**
```json
{
  "clientSecret": "pi_xxx_secret_yyy",
  "amount": 8420,
  "currency": "mxn",
  "orderId": "guid-temporal"
}
```

### 6.6 Traducción

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/translation/translate` | Traducir texto vía DeepL |
| GET | `/translation/history` | Historial de traducciones guardadas |
| POST | `/translation/history` | Guardar traducción en el historial |
| DELETE | `/translation/history/{id}` | Eliminar traducción del historial |

**POST /translation/translate — Request:**
```json
{
  "text": "The mitochondria is the powerhouse of the cell.",
  "sourceLang": "EN",
  "targetLang": "ES"
}
```
**Response 200:**
```json
{
  "originalText": "The mitochondria is the powerhouse...",
  "translatedText": "La mitocondria es la central energética...",
  "sourceLang": "EN",
  "targetLang": "ES",
  "detectedLang": "EN"
}
```

### 6.7 Grupos de Estudio

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/groups` | Explorar grupos disponibles |
| GET | `/groups/my` | Grupos del usuario autenticado |
| GET | `/groups/{id}` | Detalle de un grupo |
| POST | `/groups` | Crear grupo |
| PUT | `/groups/{id}` | Editar grupo (solo Admin) |
| DELETE | `/groups/{id}` | Eliminar grupo (solo Admin) |
| POST | `/groups/{id}/join` | Unirse a un grupo |
| POST | `/groups/{id}/leave` | Abandonar un grupo |
| GET | `/groups/{id}/members` | Listar miembros |
| GET | `/groups/{id}/messages` | Historial de mensajes (paginado) |

### 6.8 Notas y Citas

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/notes` | Listar notas del usuario |
| POST | `/notes` | Crear nota |
| PUT | `/notes/{id}` | Editar nota |
| DELETE | `/notes/{id}` | Eliminar nota |
| GET | `/citations` | Listar citas bibliográficas |
| POST | `/citations` | Crear cita |
| DELETE | `/citations/{id}` | Eliminar cita |
| GET | `/citations/export` | Exportar bibliografía (`?format=APA&bookId=guid`) |

### 6.9 Panel de Vendedor

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/vendors/me/books` | Libros publicados por el vendedor |
| POST | `/vendors/me/books` | Publicar nuevo libro (multipart/form-data) |
| PUT | `/vendors/me/books/{id}` | Editar libro publicado |
| DELETE | `/vendors/me/books/{id}` | Eliminar libro del marketplace |
| GET | `/vendors/me/sales` | Historial de ventas |
| GET | `/vendors/me/stats` | Estadísticas: ingresos, libros vendidos, rating |

---

## 7. Arquitectura en Tiempo Real — SignalR

### Hub endpoint
```
wss://localhost:{port}/hubs/chat
```

### Flujo de conexión

1. El cliente WPF establece `HubConnection` con el JWT en el header.
2. El servidor autentica el token antes de aceptar la conexión.
3. Al unirse a un grupo, el cliente invoca `JoinGroup(groupId)`.
4. El servidor añade la conexión al grupo de SignalR correspondiente.
5. Los mensajes se envían al grupo, no a usuarios individuales.

### Métodos del Hub (servidor → cliente)

| Evento | Payload | Descripción |
|---|---|---|
| `ReceiveMessage` | `MessageDto` | Nuevo mensaje en el grupo |
| `UserJoined` | `{ userId, fullName }` | Miembro se unió al grupo |
| `UserLeft` | `{ userId, fullName }` | Miembro abandonó el grupo |
| `TypingIndicator` | `{ userId, fullName, isTyping }` | Indicador de escritura |
| `MessageEdited` | `MessageDto` | Mensaje editado |
| `MessageDeleted` | `{ messageId }` | Mensaje eliminado |

### Métodos del Hub (cliente → servidor)

| Método | Parámetros | Descripción |
|---|---|---|
| `JoinGroup` | `groupId: string` | Suscribirse a un grupo |
| `LeaveGroup` | `groupId: string` | Desuscribirse del grupo |
| `SendMessage` | `groupId, content` | Enviar mensaje al grupo |
| `SendTyping` | `groupId, isTyping` | Notificar estado de escritura |
| `EditMessage` | `messageId, newContent` | Editar mensaje propio |
| `DeleteMessage` | `messageId` | Eliminar mensaje propio |

---

## 8. Modo Offline

### Principio

La aplicación requiere conexión para todas las operaciones de red (API, chat, pagos, traducción). Sin embargo, los documentos y libros digitales **previamente descargados** son accesibles sin conexión a través de una caché local en el sistema de archivos del cliente.

### Funcionamiento

**Al descargar un libro o documento:**
1. El cliente solicita al API una URL firmada temporal de Azure Blob.
2. Descarga el archivo y lo almacena en la ruta local:
   ```
   %AppData%\Intelectia\Cache\{userId}\books\{bookId}.pdf
   %AppData%\Intelectia\Cache\{userId}\documents\{docId}.pdf
   ```
3. Registra la ruta local en un archivo de manifiesto local:
   ```
   %AppData%\Intelectia\Cache\{userId}\manifest.json
   ```

**Al abrir contenido:**
1. El `OfflineService` verifica si existe una entrada en el manifiesto local.
2. Si existe y el archivo es válido: abre desde disco, sin llamada al API.
3. Si no existe: solicita descarga o muestra mensaje de conexión requerida.

**Invalidación de caché:**
- Al detectar conexión después de estar offline, el cliente verifica el hash/ETag del archivo en el API.
- Si cambió, descarga la versión actualizada.

### Estructura del manifiesto local
```json
{
  "userId": "guid",
  "entries": [
    {
      "resourceId": "guid",
      "type": "Book",
      "localPath": "C:\\Users\\...\\Intelectia\\Cache\\books\\guid.pdf",
      "downloadedAt": "2025-01-15T10:30:00Z",
      "etag": "abc123"
    }
  ]
}
```

---

## 9. Integraciones Externas

### 9.1 Google OAuth 2.0

**Flujo:**
1. El cliente WPF abre el navegador del sistema con la URL de autorización de Google.
2. Google redirige al usuario de vuelta con un `authorization_code`.
3. El cliente envía ese código al endpoint `POST /auth/google`.
4. El backend lo intercambia con Google por tokens de acceso y obtiene el perfil del usuario.
5. Si el email ya está registrado, hace login. Si no, crea cuenta y hace login.
6. Devuelve JWT al cliente igual que un login normal.

**Configuración necesaria:**
- Client ID y Client Secret en Google Cloud Console.
- Redirect URI registrada: `http://localhost/oauth/callback` (esquema local para WPF).

### 9.2 Stripe Sandbox

**Flujo de pago:**
1. El cliente envía los items del carrito al API.
2. El API calcula el total y crea un `PaymentIntent` en Stripe.
3. El API devuelve el `clientSecret` del PaymentIntent al cliente.
4. El cliente WPF usa Stripe.net SDK para confirmar el pago con el método de pago del usuario.
5. El API recibe el webhook de Stripe `payment_intent.succeeded`.
6. El API actualiza el estado del pedido a `Processing` y, si es digital, registra los libros en la biblioteca del usuario.

**Webhook endpoint:** `POST /api/webhooks/stripe`

**Tarjetas de prueba (Sandbox):**
- `4242 4242 4242 4242` — Pago exitoso
- `4000 0000 0000 9995` — Fondos insuficientes

### 9.3 DeepL API Free

- Límite: 500,000 caracteres por mes.
- La llamada se realiza exclusivamente desde el backend. El cliente nunca tiene acceso a la API key.
- Endpoint utilizado: `POST https://api-free.deepl.com/v2/translate`
- La API key se almacena en `appsettings.json` bajo una sección `ExternalServices:DeepL:ApiKey`, nunca en el cliente.
- Se implementa un contador de uso mensual en base de datos para alertar cuando se acerca al límite.

### 9.4 Azure Blob Storage

**Estructura de contenedores:**
```
intelectia-storage/
├── book-covers/          (público, lectura anónima para mostrar portadas)
├── book-files/           (privado, acceso solo por URL firmada temporal)
├── document-files/       (privado, acceso solo por URL firmada temporal)
└── avatars/              (público, lectura anónima)
```

**URLs firmadas (Shared Access Signature):**
- Duración máxima: 1 hora.
- El API genera la URL firmada y la devuelve al cliente. El cliente descarga directamente de Azure, sin que el archivo pase por el servidor del API.
- Esto reduce el consumo de ancho de banda del backend significativamente.

---

## 10. Sistema de Diseño UI

El sistema de diseño define el lenguaje visual completo de la aplicación. Se implementa en WPF mediante `ResourceDictionaries` que se fusionan en `App.xaml`, garantizando que cualquier control del proyecto pueda acceder a los tokens de diseño por clave.

### 10.1 Paleta Cromática

| Token | Valor HEX | Uso |
|---|---|---|
| `PrimaryColor` | `#2C5530` | Barra lateral, headers estructurales |
| `PrimaryLightColor` | `#3D7A45` | Hover sobre elementos primarios |
| `PrimaryDarkColor` | `#1E3B22` | Active state en navegación |
| `AccentColor` | `#D4AF37` | CTAs: Comprar, Pagar, Guardar, Publicar |
| `AccentHoverColor` | `#C49B20` | Hover sobre botones de acento |
| `BackgroundColor` | `#F5F5F5` | Lienzo base de la aplicación |
| `SurfaceColor` | `#FFFFFF` | Tarjetas, paneles de contenido |
| `TextPrimaryColor` | `#1A1A1A` | Cuerpo de texto, títulos |
| `TextSecondaryColor` | `#6B6B6B` | Metadatos, labels secundarios |
| `DividerColor` | `#E0E0E0` | Bordes de inputs, separadores |
| `ErrorColor` | `#D32F2F` | Mensajes de error, validaciones |
| `SuccessColor` | `#388E3C` | Confirmaciones, estados OK |
| `WarningColor` | `#F57C00` | Estados de advertencia |
| `OverlayColor` | `#80000000` | Fondos de modales y overlays |

**Implementación en Colors.xaml:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Color x:Key="PrimaryColor">#2C5530</Color>
    <Color x:Key="AccentColor">#D4AF37</Color>
    <Color x:Key="BackgroundColor">#F5F5F5</Color>
    <!-- ... resto de colores ... -->

    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <!-- ... resto de brushes ... -->

</ResourceDictionary>
```

### 10.2 Tipografía

| Token | Fuente | Tamaño | Peso | Uso |
|---|---|---|---|---|
| `H1Style` | Segoe UI | 28px | SemiBold (600) | Títulos de página principal |
| `H2Style` | Segoe UI | 22px | SemiBold (600) | Títulos de sección |
| `H3Style` | Segoe UI | 18px | Medium (500) | Sub-secciones, títulos de card |
| `BodyStyle` | Segoe UI | 14px | Regular (400) | Cuerpo de texto general |
| `CaptionStyle` | Segoe UI | 12px | Regular (400) | Metadatos, labels, fechas |
| `ButtonStyle` | Segoe UI | 14px | Medium (500) | Texto de botones |
| `LabelStyle` | Segoe UI | 13px | Medium (500) | Labels de formularios |

La fuente base es **Segoe UI**, disponible nativamente en Windows. No requiere instalación adicional.

### 10.3 Sistema de Espaciado

| Token | Valor | Uso |
|---|---|---|
| `SpaceXS` | 4px | Padding interno mínimo, gap entre ícono y texto |
| `SpaceSM` | 8px | Padding de elementos pequeños |
| `SpaceMD` | 16px | Padding estándar de paneles y tarjetas |
| `SpaceLG` | 24px | Separación entre secciones |
| `SpaceXL` | 32px | Márgenes de página |
| `SpaceXXL` | 48px | Separación entre bloques mayores |

### 10.4 Bordes y Elevación

| Token | Valor | Uso |
|---|---|---|
| `RadiusSM` | 4px | Inputs, botones pequeños |
| `RadiusMD` | 8px | Tarjetas de producto, contenedores |
| `RadiusLG` | 12px | Tarjetas grandes, paneles |
| `RadiusXL` | 16px | Modales |
| `RadiusFull` | 9999px | Avatares, badges, chips |

Las sombras se implementan con `DropShadowEffect` en WPF:
- **Elevación 1** (sutil): `ShadowDepth=1, BlurRadius=4, Opacity=0.08` — para cards inactivas
- **Elevación 2** (card): `ShadowDepth=2, BlurRadius=8, Opacity=0.12` — para cards con contenido
- **Elevación 3** (modal): `ShadowDepth=4, BlurRadius=16, Opacity=0.16` — para overlays

### 10.5 Componentes Base

**Botón Primario (CTA):**
- Fondo: `AccentBrush` (#D4AF37)
- Texto: `#1A1A1A`
- Hover: `AccentHoverBrush` (#C49B20)
- Border Radius: `RadiusSM` (4px)
- Padding: 12px 24px
- Font: `ButtonStyle`
- Usos: Comprar Ahora, Proceder al Pago, Guardar Cambios, Publicar

**Botón Secundario (Outline):**
- Fondo: Transparente
- Borde: 1.5px `PrimaryBrush`
- Texto: `PrimaryBrush`
- Hover: fondo `#102C5530` (primary con 10% alpha)
- Usos: Añadir al Carrito, Cancelar, Volver

**Botón Ghost:**
- Sin fondo, sin borde visible
- Texto: `TextSecondaryColor`
- Hover: fondo `#0A000000`
- Usos: acciones terciarias, links de navegación

**Input de Texto:**
- Borde: 1px `DividerColor`
- Fondo: `SurfaceColor`
- Border Radius: `RadiusSM`
- Padding: 10px 12px
- Focus: borde `PrimaryBrush` 2px
- Error: borde `ErrorColor` 2px + label de error en rojo

**Tarjeta de Libro (BookCard):**
- Fondo: `SurfaceColor`
- Border Radius: `RadiusMD`
- Elevación 2
- Contiene: imagen de portada (ratio 3:4), título, autor, precio, badge de formato
- Hover: elevación 3 + ligera escala (ScaleTransform 1.02)

### 10.6 Navegación

La estructura de navegación sigue el patrón Shell de WPF: una ventana principal (`MainWindow`) que contiene un área de contenido central donde se cargan `UserControls` dinámicamente según la selección en la barra lateral.

**Barra lateral (NavigationRail):**
- Ancho: 220px (expandido) / 64px (colapsado)
- Fondo: `PrimaryBrush`
- Items: ícono + label
- Estado activo: borde izquierdo 3px `AccentBrush` + fondo `PrimaryDarkBrush`
- Módulos: Mi Biblioteca · Marketplace · Grupos de Estudio · Para Vendedores · Perfil y Ajustes

**Iconografía:**
- Set: Material Design Icons (disponible como fuente de iconos o como recursos SVG embebidos).
- Estilo: lineales, monocromáticos.
- Color inactivo: `#80FFFFFF` (blanco 50% alpha sobre fondo verde).
- Color activo: `#FFFFFFFF` (blanco sólido).

---

## 11. Seguridad y Autenticación

### JWT Configuration

```json
{
  "JwtSettings": {
    "Secret": "VARIABLE_DE_ENTORNO_NO_AQUI",
    "Issuer": "intelectia-api",
    "Audience": "intelectia-client",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  }
}
```

- El `Secret` nunca va en `appsettings.json` en el repositorio. Se gestiona como variable de entorno o con `dotnet user-secrets` en desarrollo.
- El `RefreshToken` se almacena en base de datos con hash. Al utilizarse, se invalida y se emite uno nuevo (rotación).

### Prácticas de seguridad implementadas

| Riesgo | Mitigación |
|---|---|
| SQL Injection | EF Core con LINQ parametrizado. Cero SQL raw sin parámetros. |
| Contraseñas en texto plano | BCrypt con salt (BCrypt.Net-Next). |
| Tokens expuestos | JWT con expiración corta + refresh token rotativo. |
| Datos de pago | Stripe maneja los datos de tarjeta directamente. La API solo recibe PaymentMethodId. |
| CORS | Configurado explícitamente para aceptar solo el origen del cliente WPF en desarrollo. |
| Rate Limiting | Middleware de rate limiting en endpoints de auth para mitigar fuerza bruta. |
| Secrets en código | Todas las API keys en variables de entorno / user-secrets. |

---

## 12. Roadmap de Construcción por Fases

El proyecto se divide en 8 fases. Cada fase produce funcionalidad vertical completa y demostrable.

### Fase 1 — Fundación e Infraestructura (Semanas 1-2)

**Backend:**
- Crear solución con los 4 proyectos de Clean Architecture.
- Configurar EF Core + SQL Server Express + conexión.
- Crear todas las entidades del dominio.
- Generar primera migración y verificar schema en SQL Server.
- Configurar JWT (generación + validación).
- Configurar Google OAuth handler.
- Configurar middleware de excepciones globales.
- Configurar CORS.

**Cliente WPF:**
- Crear proyecto WPF .NET 8.
- Configurar estructura MVVM: `BaseViewModel`, `RelayCommand`, `AsyncRelayCommand`.
- Configurar `NavigationService`.
- Crear todos los `ResourceDictionaries` del sistema de diseño (`Colors.xaml`, `Typography.xaml`, `Buttons.xaml`, `Inputs.xaml`, `Cards.xaml`).
- Montar `MainWindow` con `NavigationRail` y área de contenido central.
- Configurar `ApiClient` (HttpClient wrapper con Polly para reintentos).

**Entregable:** Solución compilando, schema de base de datos creado, shell visual de la app con navegación funcional entre páginas vacías.

---

### Fase 2 — Módulo de Autenticación (Semana 3)

**Backend:**
- Endpoints: register, login, google, refresh, logout, forgot-password, reset-password.
- Servicio de email (SMTP con MailKit) para verificación y recuperación.
- Validaciones con FluentValidation.
- Bloqueo tras 3 intentos fallidos (campo `LockoutEnd` en User).

**Cliente WPF:**
- `LoginView` + `LoginViewModel`.
- `RegisterView` + `RegisterViewModel`.
- `RecoverPasswordView` + `RecoverPasswordViewModel`.
- `AuthService`: almacena JWT en memoria segura, gestiona refresh automático.
- Flujo de Google OAuth: abrir navegador, capturar callback.
- Redirección automática a Dashboard tras autenticación exitosa.
- Persistencia de sesión: si hay refresh token válido en almacenamiento local seguro (`Windows Credential Manager`), hacer login automático al abrir la app.

**Entregable:** Flujo de autenticación completo y funcional incluyendo Google OAuth.

---

### Fase 3 — Catálogo del Marketplace (Semanas 4-5)

**Backend:**
- Endpoint `GET /books` con paginación, búsqueda full-text y filtros.
- Endpoint `GET /books/{id}` con detalle completo.
- Endpoint `GET /books/categories`.
- Endpoint `GET /books/{id}/reviews` y `POST /books/{id}/reviews`.
- Seeder con datos de prueba (20+ libros en distintas categorías).

**Cliente WPF:**
- `MarketplaceView` + `MarketplaceViewModel`:
  - Grid de `BookCard` con carga paginada.
  - SearchBox con debounce (300ms).
  - Panel de filtros lateral (categoría, formato, precio, estado).
- `BookDetailView` + `BookDetailViewModel`:
  - Detalle completo, selector de formato, botones CTA.
  - Sección de reseñas.
  - Breadcrumb de navegación.

**Entregable:** Exploración completa del catálogo con búsqueda y filtros.

---

### Fase 4 — Comercio y Pedidos (Semanas 6-7)

**Backend:**
- Carrito: lógica en memoria del servidor (o en base de datos como entidad `Cart` + `CartItem`).
- Endpoints de órdenes y pagos.
- Integración Stripe: crear PaymentIntent, confirmar, webhook `payment_intent.succeeded`.
- Al confirmar pago digital: registrar libros en `UserBook` automáticamente.
- Ciclo de estados de pedido.
- Generación de recibo (PDF simple con iTextSharp o QuestPDF).

**Cliente WPF:**
- `CartView` + `CartViewModel`:
  - Lista de ítems, cantidades editables, subtotal dinámico, campo de código de descuento.
- `CheckoutView` + `CheckoutViewModel`:
  - Paso 1: Selección de dirección.
  - Paso 2: Selección de método de pago.
  - Paso 3: Resumen y confirmación.
  - Manejo de estados de carga durante el pago.
- `OrderHistoryView` + `OrderHistoryViewModel`.
- `OrderDetailView` con línea de tiempo del estado del pedido.
- `AddressesView` y `PaymentMethodsView` en el perfil.

**Entregable:** Flujo de compra completo extremo a extremo con Stripe Sandbox.

---

### Fase 5 — Biblioteca Personal y Herramientas de Estudio (Semanas 8-9)

**Backend:**
- Endpoint de biblioteca personal.
- Generación de URLs firmadas de Azure Blob.
- Endpoints de documentos (upload, download-url, delete).
- Integración Azure Blob Storage (subida de archivos desde el API).
- Endpoints de notas y citas.
- Endpoint de traducción (proxy a DeepL, nunca exponer API key al cliente).
- Generación de exportación de bibliografía en texto formateado.

**Cliente WPF:**
- `LibraryView` + `LibraryViewModel`:
  - Tabs: Libros Adquiridos / Mis Documentos.
  - Cards de libros con barra de progreso.
  - Botón de descarga con indicador de estado (no descargado / descargando / disponible offline).
- `BookViewerView` + `BookViewerViewModel`:
  - Renderizado de PDF con `PdfiumViewer` o `Docotic.Pdf`.
  - Panel lateral: Notas · Citas · Traductor.
  - Función de resaltado de texto.
  - Tradución contextual: seleccionar texto → botón flotante → panel de traducción.
  - `OfflineService` integrado: abrir desde disco si está descargado.
- `NotesView` y `CitationsView` como paneles dentro del visor.
- `TranslationView` embebida en el visor.

**Entregable:** Biblioteca funcional con visor, herramientas de estudio y capacidad offline.

---

### Fase 6 — Panel de Vendedor (Semana 10)

**Backend:**
- Endpoint `POST /users/me/become-vendor` (crea `VendorProfile`).
- Endpoints CRUD de libros para el vendedor.
- Subida de portada y archivo del libro a Azure Blob.
- Endpoints de estadísticas y ventas.

**Cliente WPF:**
- Control de acceso: el módulo "Para Vendedores" solo es visible si el usuario tiene `VendorProfile`.
- Si no lo tiene: pantalla de onboarding para activar el rol.
- `VendorDashboardView`: tarjetas de resumen (libros publicados, ventas del mes, ingresos, rating).
- `PublishBookView` + `PublishBookViewModel`:
  - Formulario completo con selector de archivo para portada y libro digital.
  - Subida con barra de progreso.
- `InventoryView`: tabla de libros con acciones de editar/eliminar.
- `SalesView`: historial de ventas con filtros por fecha.

**Entregable:** Panel de vendedor completamente funcional.

---

### Fase 7 — Grupos de Estudio y Chat en Tiempo Real (Semanas 11-12)

**Backend:**
- Endpoints CRUD de grupos.
- SignalR `ChatHub` con manejo de grupos, mensajes, typing indicators.
- Historial de mensajes paginado desde la base de datos.
- Persistencia de mensajes en base de datos al recibirlos en el Hub.

**Cliente WPF:**
- `GroupsView` + `GroupsViewModel`:
  - Tab "Mis Grupos" y Tab "Explorar".
  - `GroupCard` con nombre, descripción, contador de miembros.
- `GroupDetailView` + `ChatViewModel`:
  - Lista de mensajes con scroll infinito hacia arriba (carga historial).
  - Input de mensaje con soporte de Enter para enviar.
  - Indicador de typing en tiempo real.
  - `SignalRService`: gestiona la `HubConnection`, reconexión automática, eventos.
  - Notificación visual de nuevos mensajes en el ícono del módulo.
- `CreateGroupView`.

**Entregable:** Chat en tiempo real funcional con historial persistente.

---

### Fase 8 — Perfil, Polish y Estabilización (Semanas 13-14)

**Perfil completo:**
- `ProfileView`: foto de perfil (subida a Azure Blob), datos personales, perfil de estudiante.
- `SecurityView`: cambio de contraseña, sesiones activas.
- `AddressesView`: CRUD de direcciones con marcado de predeterminada.
- `PaymentMethodsView`: CRUD de métodos de pago vía Stripe.

**Polish global:**
- Estados de carga (`SkeletonLoaders`) en todas las vistas con datos remotos.
- Manejo de errores global en el cliente: interceptor en `ApiClient` que captura errores 401 (refresh), 403, 500 y muestra notificaciones no intrusivas (toast).
- Detección de conexión: banner informativo cuando no hay internet.
- Animaciones de transición entre vistas (Storyboard en WPF).
- Revisión de accesibilidad: contraste de colores, tamaños mínimos de targets.
- Pruebas de integración en los casos de uso críticos del backend.
- Revisión de seguridad: confirmar que ninguna API key está expuesta en el cliente.

**Entregable:** Aplicación completa, estable y lista para presentación en portafolio.

---

## Apéndice — Variables de Entorno y Configuración

Ninguna de las siguientes claves va en el repositorio. Se gestionan con `dotnet user-secrets` en desarrollo:

```bash
dotnet user-secrets set "JwtSettings:Secret" "tu-secret-aqui"
dotnet user-secrets set "ConnectionStrings:Default" "Server=...;Database=DB_Intelectia;..."
dotnet user-secrets set "ExternalServices:DeepL:ApiKey" "tu-key-aqui"
dotnet user-secrets set "ExternalServices:Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "ExternalServices:Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "ExternalServices:Google:ClientId" "..."
dotnet user-secrets set "ExternalServices:Google:ClientSecret" "..."
dotnet user-secrets set "ExternalServices:Azure:BlobConnectionString" "..."
dotnet user-secrets set "ExternalServices:Email:SmtpPassword" "..."
```

El archivo `.gitignore` debe incluir:
```
**/appsettings.Development.json
**/secrets.json
**/.env
```

---

*Intelectia Technical Documentation v2.0 — Última revisión: Abril 2026*
