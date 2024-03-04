using System;

namespace Isolaatti.Utils;

public static class Extensions
{
    public static int ToInt(this string input)
    {
        return Convert.ToInt32(input);
    }
}