using System;

namespace Isolaatti.Utils;

public static class Extensions
{
    public static int ToInt(this string input)
    {
        return Convert.ToInt32(input);
    }
    
    public static T Apply<T>(this T obj, Action<T> block)
    {
        block(obj);
        return obj;
    }
}