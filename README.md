# Isolaatti-BackendWebApp
Sitio web de Isolaatti

[![.NET](https://github.com/Isolaatti-Software/IsolaattiWebsite/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Isolaatti-Software/IsolaattiWebsite/actions/workflows/dotnet.yml)

Bienvenid@

## Plataformas compatibles
El proyecto puede funcionar en Windows, Linux o Mac.

## Cosas a instalar
Este proyecto es la aplicación web principal, aquí es donde el usuario final accede. Pon especial
atención a los requerimientos que debes satisfacer antes de poder correr la aplicación
en tu estación de trabajo.

### Microsoft .NET 7 SDK 
Este componente es el más importante, es necesario para compilar el código de este proyecto. La forma de instalarlo depende
del sistema operativo que estés usando. https://dotnet.microsoft.com/en-us/download/dotnet/7.0

### Instancia de PostgreSQL
https://www.postgresql.org/download/  
Es el RDBMS que se utiliza en este proyecto. Puedes instalarlo a mano como un servicio en
tu propia estación de trabajo o utilizar Docker, tema que más adelante se aborda.

### MongoDB
Servidor Community https://www.mongodb.com/try/download/community  
Cluster en la nube https://www.mongodb.com/try  
Es una base de datos de documentos, se utiliza en algunas partes de este proyecto. Puedes instalarlo en tu propia maquina utilizando
el Community Server, utilizar un cluster gratuito de Atlas o con Docker.

### RabbitMQ
https://www.rabbitmq.com/download.html  
Es el mensajero de eventos que se utiliza para comunicarse con otros servicios del sistema. Puedes instalarlo directamente
o con Docker. 




## appsettings.Development.json
Este archivo debe encontrarse en tu entorno local, pero no se incluye en el repositorio. Revisa el archivo y crealo. Cuida de no hacer commit a repositorios publicos,
ya que podrías exponer secretos.
https://gist.github.com/erik-everardo/9adcee90e7fbbb5094d380201e1ee907

Revisemos las entradas que .



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
```shell
dotnet ef migrations add InitialMigration
dotnet ef database update
```
Al ejecutar el comando `dotnet ef database update` se deberían crear las tablas en la base de datos.

Alternativamente, puedes generar el código SQL que crea todas las tablas, sin realizar migraciones. Para ello haz lo siguiente.
```shell
dotnet ef dbcontext script -o miArchivoSql.sql --context DbContextApp
```
Lo anterior generará un archivo SQL, que puedes correr en tu instalación de postgreSQL. Por ejemplo, en la imagen se está utilizando pgAdmin para
correr el script.
![pgadmin](https://user-images.githubusercontent.com/43968631/193508789-68e7bae5-8ca1-4314-b488-e9e0bcd65395.png)


Información más a detalle acerca del CLI de Entity Framework: https://learn.microsoft.com/en-us/ef/core/cli/dotnet

5. Ejecuta con el botón de tu IDE, o si estás en linea de comandos, ejecuta `dotnet run`.

Si hay algún problema. erik10cavazos@gmail.com
