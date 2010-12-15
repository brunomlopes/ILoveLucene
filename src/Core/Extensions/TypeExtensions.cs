using System;

namespace Core.Extensions
{
    public static class TypeExtensions
    {
        public static string FriendlyTypeName(this Type type)
        {
            var name = type.Name;
            return System.Text.RegularExpressions.Regex.Replace(name, "(?<l>[A-Z])", " ${l}").Trim();
        }
        
        public static string FriendlyTypeName(this object type)
        {
            return type.GetType().FriendlyTypeName();
        }
    }
}