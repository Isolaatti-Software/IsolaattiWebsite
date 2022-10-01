namespace Isolaatti.isolaatti_lib
{
    public class QueryNormalization
    {
        public static string ReplaceAccents(string text)
        {
            return text.Replace("á", "a")
                .Replace("à", "a")
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("í", "i")
                .Replace("ì", "i")
                .Replace("ó", "o")
                .Replace("ò", "o")
                .Replace("ú", "u")
                .Replace("ù", "u")
                .Replace("ä", "a")
                .Replace("ë", "e")
                .Replace("ï", "i")
                .Replace("ö", "o")
                .Replace("ü", "u");
        }
    }
}