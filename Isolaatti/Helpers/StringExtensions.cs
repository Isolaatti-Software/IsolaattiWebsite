namespace Isolaatti.Helpers;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);
    public static string IfNullOrWhiteSpace(this string text, string defaultValue) => !string.IsNullOrWhiteSpace(text) ? text : defaultValue;
}