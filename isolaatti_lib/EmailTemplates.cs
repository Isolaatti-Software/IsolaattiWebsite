/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

namespace isolaatti_API.isolaatti_lib
{
    public class EmailTemplates
    {
        public const string SongReady = @"<html><body>
        <h1>Song {0} has been processed!</h1>
        <p>Open this link <a href='{5}'>{5}</a></p>
        <p>Thr 4 base tracks are now available on the app. Also, here you have direct links to the files:</p>
        <ul>
            <li>Bass: {1}</li>
            <li>Drums: {2}</li>
            <li>Vocals: {3}</li>
            <li>Other: {4}</li>
        </ul>
        
        </body></html>";

        public const string SongStartedProcessing = @"<html><body>
        <h1>Your song is being processing now</h1>
        <p>Your song {0} is now processing. Please wait, you will receive a message when it's finished</p>
        </body></html>";

        public const string PasswordRecoveryEmail = @"<html><body>
            <h1>Hola {1}. Restablezcamos tu contraseña.</h1>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta de Isolaatti</p>
            <p>Para que puedas restablecer tu contraseña, solo necesitas hacer clic en el enlace. Solo es válido una vez.</p>
            <p><a href='{0}'>{0}</a></p>";

        public const string WelcomeEmail = @"<html><body>
            <h1>¡Hola, {0}!</h1>
            <p>Me complace darte la bienvenida a Isolaatti, gracias por crear una cuenta</p>
            <p>Erik (erikswdev)</p>
        </body></html>";
    }
}