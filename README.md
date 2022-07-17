# Isolaatti-BackendWebApp
Sitio web de Isolaatti

Bienvenid@

## Plataformas compatibles
El proyecto puede funcionar en Windows, Linux o Mac.

## Cosas a instalar
* Microsoft .NET 6 https://dotnet.microsoft.com/en-us/download/dotnet/6.0
* PostgreSQL
* MongoDB

## Cosas que debes tener disponibles
* Una cuenta de SendGrid. Esto se usa para enviar correos. https://sendgrid.com/free/
* Cuenta de Google Firebase (puede ser la gratuita). Esto crea una cuenta de Google Cloud también.

## appsettings.Development.json
Este archivo debe encontrarse en tu entorno local, pero no se incluye en el repositorio. Revisa el archivo y crealo. Cuida de no hacer commit a repositorios publicos,
ya que podrías exponer secretos.
https://gist.github.com/erik-everardo/9adcee90e7fbbb5094d380201e1ee907

## isolaatti-firebase-adminsdk.json
Este archivo no existe en el repositorio, pero deberás utilizar el proporcionado por Google Cloud. 
Tienes que crear una clave de acceso administrador. Esto se usa para poder, desde el servidor, validar las sesiones de cuentas
de Google, Microsoft y Facebook. También se necesita para acceder al Bucket de storage.

Debes usar este mismo nombre, o de lo contrario tendrías que modificar el código donde se use el archivo, y cambiar el nombre del archivo.

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
