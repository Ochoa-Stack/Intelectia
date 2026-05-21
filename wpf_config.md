# Guía de Configuración del Cliente WPF

Intelectia.WPF está diseñado para conectarse dinámicamente a la API, permitiéndote alternar fácilmente entre tu entorno de desarrollo local y el entorno de producción en la nube.

## Cómo cambiar la URL de la API

La URL base a la que apunta el cliente WPF se encuentra definida en el archivo `Intelectia.WPF/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:8080"
  }
}
```

> Nota: El archivo `appsettings.json` debe tener siempre configurada la propiedad **`Copy to Output Directory: Copy if newer`** (Copiar si es posterior) en Visual Studio o en el archivo `.csproj`. Esto asegura que el binario de la aplicación siempre tenga acceso a la configuración más reciente.

### Entorno de Desarrollo (Localhost)
Para desarrollo, asegúrate de que el valor sea tu URL local (generalmente la que expone Docker Compose o Kestrel):
- `BaseUrl`: `"https://localhost:8080"` (o el puerto que utilices)

### Entorno de Producción (Render / Railway)
Para apuntar el cliente a la API productiva en la nube, cambia el valor por tu URL pública:
- `BaseUrl`: `"https://intelectia-api.onrender.com"` (reemplaza por tu URL real)

## Compilar una Versión de Release (Producción)

Si vas a distribuir la aplicación a los usuarios finales, compila una versión en modo Release asegurándote de que apunta a producción:

- Abre `appsettings.json` y cambia `BaseUrl` a tu URL de producción en la nube.
- Abre una terminal en la raíz de la solución o en el proyecto WPF.
- Ejecuta el comando de compilación o publicación, por ejemplo:
   ```bash
   dotnet publish Intelectia.WPF/Intelectia.WPF.csproj -c Release -r win-x64 --self-contained
   ```
- Distribuye el contenido de la carpeta generada (incluyendo el `appsettings.json` configurado) a tus usuarios.

> **Tip:** No es necesario recompilar todo el proyecto si solo modificas este JSON en la carpeta de binarios ya generada (`bin/Release/...`). El cliente WPF lo leerá dinámicamente al iniciarse cada vez.

## ¿Qué pasa con CORS?

Los clientes nativos de escritorio (como WPF) no siempre envían la cabecera `Origin` de la misma manera que lo hacen los navegadores web tradicionales. 

Para que la API acepte las peticiones del cliente WPF, la política de CORS en `Intelectia.API` está configurada para aceptar `AllowAnyOrigin()` cuando `AllowedOrigins` está establecido en `*`. En desarrollo esto es el comportamiento por defecto, y en producción es seguro mantener `*` en una aplicación dirigida a clientes móviles/escritorio.
