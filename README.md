# Intelectia

Ecosistema académico de escritorio para estudiantes universitarios: marketplace de libros, biblioteca personal, herramientas de estudio y colaboración en tiempo real.

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat&logo=postgresql)](https://www.postgresql.org)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![CI](https://github.com/Ochoa-Stack/Intelectia/actions/workflows/ci.yml/badge.svg)](https://github.com/Ochoa-Stack/Intelectia/actions/workflows/ci.yml)

> Deploy API: `https://intelectia-api.onrender.com/health`\
> **Status:** Producción gratuita

---

## Resumen

Intelectia es un ecosistema académico unificado que opera bajo dos roles integrados: un portal de estudio para estudiantes (biblioteca personal, traducción contextual, gestor de citas) y un panel de venta para proveedores (marketplace, inventario, estadísticas). Ambos contextos coexisten en una única sesión de usuario, eliminando la fragmentación típica de herramientas académicas dispersas y reduciendo la carga cognitiva asociada al cambio constante entre aplicaciones.

El sistema se construyo aplicando arquitectura pura con inversión de dependencias estricta, donde la capa de dominio no conoce infraestructura externa y cada caso de uso se ejecuta mediante un pipeline CQRS validado combinada con contenerización Docker y despliegue en servicios free-tier, garantiza que el proyecto sea práctico sin comprometer la seguridad ni la calidad del código.

---

## Arquitectura

Intelectia sigue **Clean Architecture** con inversión de dependencias estricta en cuatro capas desacopladas:

```
Intelectia/
├── Domain/             # Entidades, reglas de negocio puras (sin dependencias externas)
├── Application/        # Casos de uso (CQRS con MediatR), validación (FluentValidation)
├── Infrastructure/     # EF Core, repositorios, servicios externos (Stripe, DeepL, Azure)
├── API/                # Controllers, SignalR, middleware, configuración de arranque
├── WPF/                # Cliente de escritorio (zero referencias a UI en ViewModels)
└── Shared/             # DTOs y enums compartidos entre API y cliente
```

### Decisiones Abordadas
- **Patrón Repositorio Específico:** Desacoplamos EF Core de la capa de Application para garantizar testabilidad y mantenibilidad.
- **Pipeline CQRS con Validación:** Cada petición pasa por `ValidationBehavior<TRequest, TResponse>` antes de ejecutar su handler, centralizando la validación de entrada.
- **Soft Delete Global:** Todas las entidades heredan de `BaseEntity` y aplican un filtro global en EF Core para borrado lógico transparente.
- **Seguridad de Autenticación:** JWT stateless con rotación de refresh tokens, almacenamiento seguro vía Windows Credential Manager, y protección CSRF en OAuth.

---

## Calidad y Testing

### Ejecutar Validaciones Locales

```bash
# Compilar solución
dotnet build

# Ejecutar pruebas unitarias (cuando se implementen)
dotnet test

# Analizar código con Roslyn Analyzers (configurado por defecto en .NET)
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Pipeline de Integración Continua

Cada Pull Request a `develop` o `main` ejecuta automáticamente:
- Build de todos los proyectos
- Restauración de dependencias
- Validación de estilo de código
- Pruebas unitarias (cuando estén disponibles)

---

## Despliegue Gratuito

Intelectia está preparado para desplegarse en servicios free-tier como **Render.com** o **Railway.app** usando Docker.

Consulta la guía completa en [deploy_guide.md](deploy_guide.md) para instrucciones paso a paso.

### Requisitos Previos para Producción
- Base de datos PostgreSQL gestionada (Render Postgres Free Tier, Neon, o Supabase)
- Variables de entorno configuradas para secretos (JWT, Stripe, DeepL, etc.)
- Dominio personalizado (opcional, para evitar waking del servicio free-tier)

---

## Desarrollo Local con Docker

Levanta todo el ecosistema (API y PostgreSQL) con un solo comando:

```bash
docker-compose up --build
```

La API estará disponible en `http://localhost:8080` y el health check en `/health`.

---

## Gestión de Secretos

- **Desarrollo:** Usa `dotnet user-secrets` para credenciales locales.
- **Producción:** Inyecta variables de entorno en el panel de tu proveedor de nube.
- **Nunca** commitees credenciales reales al repositorio.

---

## Estado del Proyecto

Este proyecto fue desarrollado como parte de mi formación en **TSU Desarrollo de Software Multiplataforma**. Representa la aplicación práctica de:

- Clean Architecture y principios SOLID
- Patrón CQRS con MediatR y validación centralizada
- Autenticación JWT con rotación de refresh tokens
- Despliegue en la nube con Docker y PostgreSQL (Free Tier)
- Integración de cliente WPF con API REST en producción

> **Importante**: El contenido del marketplace (libros, grupos, etc.) no está poblado en la base de datos de producción. Para fines de demostración, el flujo de autenticación y la arquitectura están completamente funcionales.

---

## Licencia

Distribuido bajo la Licencia MIT. Ver `LICENSE` para más detalles.
