# Guía de Despliegue Gratuito

Esta guía explica cómo desplegar el backend de Intelectia en servicios free-tier.

> **Nota:** El cliente WPF es una aplicación de escritorio Windows y se distribuye por separado. Este despliegue es exclusivamente para el backend API.

---

## Opción A: Render.com (Recomendada)

### Paso: Preparar la Base de Datos

- Crea una cuenta gratuita en [Render.com](https://render.com).
- En el dashboard, selecciona **New +** → **PostgreSQL**.
- Configura:
   - Name: `intelectia-db`
   - Region: La más cercana a tus usuarios
   - Plan: **Free** (1 GB storage, 90 días de inactividad antes de suspensión)
- Anota la **Internal Database URL** (se usa dentro de Render) y la **External Database URL** (para conexión local de debugging).

### Paso: Desplegar el Web Service (API)

- En Render, selecciona **New +** → **Web Service**.
- Conecta tu repositorio de GitHub `Intelectia`.
- Configura:
   - Name: `intelectia-api`
   - Region: Misma que la base de datos
   - Branch: `main` (o `develop` para staging)
   - Build Command: `dotnet publish Intelectia.API/Intelectia.API.csproj -c Release -o /app/publish`
   - Start Command: `dotnet /app/publish/Intelectia.API.dll`
   - Plan: **Free**

### Paso: Configurar Variables de Entorno

En la sección **Environment** del Web Service, agrega:

| Variable | Valor Ejemplo | Descripción |
|----------|--------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Entorno de ejecución |
| `ASPNETCORE_HTTP_PORTS` | `8080` | Puerto del contenedor |
| `ConnectionStrings__Default` | `Host=...;Database=...;Username=...;Password=...` | Usa la **Internal Database URL** de Render Postgres |
| `JwtSettings__Secret` | *(genera una cadena aleatoria de 32+ caracteres)* | Clave para firmar JWT |
| `JwtSettings__Issuer` | `Intelectia` | Emisor del token |
| `JwtSettings__Audience` | `IntelectiaUsers` | Audiencia del token |
| `AllowedOrigins__0` | `https://tu-dominio.onrender.com` | Origen permitido para CORS |
| `ExternalServices__Stripe__SecretKey` | `sk_test_...` | Stripe Sandbox (opcional) |
| `ExternalServices__DeepL__ApiKey` | `...` | DeepL API Key (opcional) |

> **Importante:** Los valores sensibles nunca deben estar en el código. Render los inyecta como variables de entorno en runtime.

### Paso: Desplegar y Verificar

- Haz clic en **Create Web Service**.
- Render construirá la imagen Docker y desplegará la API.
- Una vez listo, visita `https://intelectia-api.onrender.com/health` para confirmar que responde "Healthy".

---

## Opción B: Railway.app

Railway ofrece un flujo similar con PostgreSQL nativo:

- Crea un proyecto nuevo y añade un servicio **PostgreSQL**.
- Añade un segundo servicio conectando tu repositorio de GitHub.
- Railway detectará automáticamente el `Dockerfile` y configurará las variables `DATABASE_URL`.
- Mapea `DATABASE_URL` a `ConnectionStrings__Default` en las variables de entorno de la API.

---

## Ejecutar Migraciones en Producción

La primera vez que despliegues, debes aplicar las migraciones a la base de datos remota:

### Método A: Desde tu máquina local (recomendado para inicial)

- Obtén la **External Database URL** de Render/Railway.
- Configúrala temporalmente en tus user-secrets locales:
   ```bash
   cd Intelectia.API
   dotnet user-secrets set "ConnectionStrings:Default" "<external-url>"
   ```
- Ejecuta las migraciones:
   ```bash
   cd ../Intelectia.Infrastructure
   dotnet ef database update --startup-project ../Intelectia.API
   ```
- Remueve el secreto temporal después:
   ```bash
   dotnet user-secrets remove "ConnectionStrings:Default"
   ```

### Método B: Script de arranque (avanzado)

Para automatizar migraciones en cada despliegue, puedes envolver el `Entry Point` del Dockerfile con un script que ejecute `dotnet ef database update` antes de `dotnet Intelectia.API.dll`. Sin embargo, para proyectos personales, el método A es suficiente y más seguro.

---

## Consideraciones del Free Tier

| Limitación | Impacto | Mitigación |
|------------|---------|------------|
| **Inactividad (Render)** | El servicio se suspende tras 90 días sin tráfico | Usa un ping externo gratuito cada 15 min para mantenerlo activo |
| **RAM limitada** | PostgreSQL Free Tier tiene ~256MB RAM | Optimiza consultas, evita cargas masivas en memoria |
| **Sin backups automáticos** | Render Free no incluye backups programados | Exporta manualmente la BD periódicamente o usa un servicio externo |
| **Dominio compartido** | URL del tipo `*.onrender.com` | Adquiere un dominio personalizado si necesitas branding profesional |

---

## Solución de Problemas Comunes

### La API no inicia / Error de conexión a BD
- Verifica que `ConnectionStrings__Default` use la **Internal Database URL** (no la externa).
- Confirma que las variables de entorno estén escritas exactamente como se muestran (respetan mayúsculas/minúsculas).

### Error de CORS al conectar desde WPF
- Asegúrate de que `AllowedOrigins__0` incluya el dominio desde el que se conecta el cliente WPF (o `*` solo para desarrollo).

### Migraciones fallan en producción
- Confirma que el usuario de la BD remota tenga permisos para crear tablas.
- Verifica que la versión de PostgreSQL en producción sea compatible con las migraciones generadas localmente (PostgreSQL 16 recomendado).

---

## Seguridad en Producción

- **JWT Secret:** Debe ser una cadena aleatoria de al menos 32 caracteres. Genera una con:
  ```bash
  openssl rand -base64 32
  ```
- **Rate Limiting:** Ya configurado en la API (5 intentos/minuto en endpoints de auth).
- **HTTPS:** Render/Railway proveen HTTPS automático. Asegúrate de que tu cliente WPF use `https://` en la URL base.
- **Logs:** Revisa los logs en el panel de Render/Railway para debugging en producción.
