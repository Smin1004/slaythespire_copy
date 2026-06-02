using System;
using UnityEngine;

public static class Utility
{
    public static T EnumParse<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value);
    }
}
