/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

namespace Isolaatti.isolaatti_lib
{
    public class EmailTemplates
    {
        public static string NotificationsAddress = "notificaciones@isolaatti.com";
        public const string PasswordRecoveryEmail = @"<html><body>
            <h1>Hola {1}. Restablezcamos tu contraseña.</h1>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta de Isolaatti</p>
            <p>Para que puedas restablecer tu contraseña, solo necesitas hacer clic en el enlace. Solo es válido una vez.</p>
            <p><a href='{0}'>{0}</a></p>";

        public const string WelcomeEmail = @"<html><body>
            <h1>¡Hola, {0}!</h1>
            <p>Me complace darte la bienvenida a Isolaatti, gracias por crear una cuenta.</p>
            <p>Saludos.</p>
        </body></html>";

        public const string WelcomeEmailExternal = @"<html><body>
            <h1>¡Hola, {0}!</h1>
            <p>Dado que creaste tu cuenta con un medio externo (Google, Facebook o Microsoft), también te hemos creado una contraseña aleatoria.</p>
            <p>Puedes optar por siempre iniciar sesión con el medio que usaste, pero si lo deseas puedes usar las siguientes credenciales</p>
            <p>Email: <strong>{1}</strong> Password: <strong>{2}</strong></p>
            <p>Saludos.</p>
            <p>Erik Cavazos</p>
        </body>";

        public const string LoginEmail = @"<html><body>
            <h1>¡Hola, {0}!</h1>
            <p>De parte de Isolaatti. Solo para informarte que acabas de iniciar sesión. Si no lo hiciste, entonces alguien conoce tu contraseña</p>
            <p>Origen: {1}</p>
            <p>Saludos.</p>
            <p>Erik Cavazos</p>
        </body>";

        public const string PreRegistrationEmail = 
            """
            <html lang="es">
            <body>
                <h1 style="text-align: center;">Isolaatti</h1>
                <p>Ya estás cerca de tener tu cuenta en Isolaatti, solo necesitas el siguiente código:</p>
                <div style="background-color: #3b2d50;color: white;padding: 1em;text-align: center;font-size: larger;">{0}</div>
                <p>Si comenzaste el registro desde el navegador web ve a <a href="https://isolaatti.com/codigo">https://isolaatti.com/codigo</a></p>
                <p>O si lo hiciste desde la app, continua en la pantalla que te solicita el código.</p>
            </body>
            </html>
            """;

        public const string InvitationEmail =
            """
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body>
                <h1 style="text-align: center;">Isolaatti</h1>
                <h2>@{0} te ha invitado a unirte al squad "{1}"</h2>
                <p>Entra a la aplicación para que revises esta invitación.</p>
            </body>
            </html>
            """;
    }
}