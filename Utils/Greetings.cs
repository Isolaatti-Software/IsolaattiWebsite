using System;

namespace isolaatti_API.Utils;

public class Greetings
{
    private static string[] _greetingsList = new[]
    {
        "Tu estancia aquí es sumamente importante para Isolaatti",
        "Adelante, tome su café sin azúcar y disfrute",
        "No lo sé, quizá deberías entrar de nuevo",
        "Te diría el numero de visitante que eres del día, pero no lo sé",
        "Sería un mentiroso si dijera que no me agrada tu visita",
        "No, no te ama, pero Isolaatti sí",
        "Ahhhhh, se te echó de menos",
        "¿Cómo te ha tratado la vida?",
        "¿Qué sería la vida si no hubiera desilucion?",
        "¡El día era normalito hasta que decidiste dar clic ahí!",
        "Vaya, vaya, ¿y si ingresas tu contraseña ahora?",
        "Ufff, mis ojos se iluminan al ver que llegas..."
    };

    public static string GetRandomGreeting()
    {
        var random = new Random();
        var index = random.Next(0, _greetingsList.Length - 1);
        return _greetingsList[index];
    }
}