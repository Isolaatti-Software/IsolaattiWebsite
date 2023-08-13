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
        public const string PasswordRecoveryEmail = @"<html><body>
            <h1>Hola {1}. Restablezcamos tu contraseña.</h1>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta de Isolaatti</p>
            <p>Para que puedas restablecer tu contraseña, solo necesitas hacer clic en el enlace. Solo es válido una vez.</p>
            <p><a href='{0}'>{0}</a></p>";

        public const string WelcomeEmail = @"<html><body>
            <h1>¡Hola, {0}!</h1>
            <p>Me complace darte la bienvenida a Isolaatti, gracias por crear una cuenta.</p>
            <p>Saludos.</p>
            <p>Erik Cavazos</p>
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
    }
}