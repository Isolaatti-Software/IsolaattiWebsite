using System.Collections.Generic;

namespace Isolaatti.isolaatti_lib
{
    public class Languages
    {
        public static string SignIn = "Sign in";
        public static string SignOut = "Sign out";
        public static string SignUp = "SignUp";
        public static string Username = "Username";
        public static string Password = "Password";
        public static string Email = "Email";


        public static Dictionary<string, string> Spanish = new Dictionary<string, string>
        {
            { "Sign in", "Iniciar sesión" },
            { "Sign out", "Cerrar sesión" },
            { "Sign up", "Registrate" },
            { "Username", "Nombre de usuario" },
            { "Password", "Contraseña" },
            { "Email", "Correo electrónico" }
        };

        public static Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "Sign in", "Sign in" },
            { "Sign out", "Sign out" },
            { "Sign up", "Sign up" },
            { "Username", "Username" },
            { "Password", "Password" },
            { "Email", "Email" }
        };

        public static Dictionary<string, string> French = new Dictionary<string, string>
        {
            { "Sign in", "S'identifier" },
            { "Sign out", "Se déconnecter" },
            { "Sign up", "S'inscrire" },
            { "Username", "Nom d'utilisateur" },
            { "Password", "Le mot de passe" },
            { "Email", "Email" }
        };
    }
}