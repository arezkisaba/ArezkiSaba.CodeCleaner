using System.Text.RegularExpressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class StringExtensions
{
    public static string ToCamelCase(
        this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var newName = Regex.Replace(input, @"[^0-9a-zA-Z]", string.Empty);
        newName = char.ToLowerInvariant(newName[0]) + newName[1..];
        return Regex.Replace(newName, @"(\s\w)", m => m.Value.ToUpper());
    }

    public static string ToPascalCase(
        this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var newName = Regex.Replace(input, @"[^0-9a-zA-Z]", string.Empty);
        return Regex.Replace(newName, @"(^\w)|(\s\w)|(\b\w)", m => m.Value.ToUpper());
    }
}