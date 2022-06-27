# Isolaatti-BackendWebApp
Backend y frontend de Isolaatti

Bienvenid@

## Plataformas compatibles
El proyecto puede funcionar en Windows, Linux o Mac.

## Cosas a instalar
* Microsoft .NET 6 https://dotnet.microsoft.com/en-us/download/dotnet/6.0
* PostgreSQL
* (Opcional pero recomendado) Instalar un IDE compatible con .NET. Puede ser Visual Studio 2022, JetBrains Rider o
  cualquier editor de código compatible, como VS Code.
* (Opcional) Si quieres hacer queries directamente a tu base de datos, puedes instalar Azure Data Studio o alguno de tu preferencia.

## Cosas que debes tener disponibles
* Una cuenta de SendGrid. Esto se usa para enviar correos. https://sendgrid.com/free/
* Cuenta de Google Firebase (puede ser la gratuita). Esto crea una cuenta de Google Cloud también.

## appsettings.Development.json
Una vez que tengas tu cuenta y api key, deberías sustituir lo siguiente del 
archivo [appsettings.Development.json](appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Database": "**aqui va el connection string de la base de datos de desarrollo"
  },
  "ApiKeys": {
    "SendGrid": "**aqui va el apikey de sendgrid**"
  }
}
```

## isolaatti-firebase-adminsdk.json
Este archivo no existe en el repositorio, pero deberás utilizar el proporcionado por Google Cloud. 
Tienes que crear una clave de acceso administrador. Esto se usa para poder, desde el servidor, validar las sesiones de cuentas
de Google, Microsoft y Facebook. También se necesita para acceder al Bucket de storage.

Lee lo siguiente:
* Admin: https://firebase.google.com/docs/admin/setup?authuser=0&%3Bhl=es&hl=es
* Cuentas de servicio Google Cloud: https://cloud.google.com/iam/docs/service-accounts y https://cloud.google.com/docs/authentication/production
## Instrucciones para correr en tu estación de trabajo
1. Clona el repositorio
2. Abre la solución con tu IDE. Si no usas un IDE, entonces desplazate hasta la carpeta del proyecto con tu línea de comandos.
3. Si estás en un IDE, los paquetes nuget deberían empezar a reestablecerse. Si lo correras con la línea de comandos, entonces haz lo siguiente.
```
cd /carpeta/del/proyecto
dotnet run
```

4. Realiza la migración de la base de datos. Se da por hecho que ya la tienes instalada y configurada correctamente. También, ya debiste haber 
proporcionado el connection string donde se menciona más arriba. Para ejecutar una migración, se recomienda eliminar todos los archivos del 
directorio [Migrations](/Migrations) y después ejecutar el siguiente comando.
```
dotnet ef migrations add InitialMigration
dotnet ef database update
```
Al ejecutar el comando `dotnet ef database update` se deberían crear las tablas en la base de datos.

5. Ejecuta con el botón de tu IDE, o si estás en linea de comandos, ejecuta `dotnet run`.

Si hay algún problema. erik10cavazos@gmail.com
