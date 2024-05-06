using System.Text;
using System.Text.RegularExpressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class StringExtensions
{
    public static string GetAlphaNumerics(
        this string input)
    {
        var cleanedString = new StringBuilder();
        foreach (var c in input)
        {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            {
                cleanedString.Append(c);
            }
        }

        return cleanedString.ToString();
    }

    public static bool IsAlphaNumerics(
        this string input)
    {
        foreach (var c in input)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    public static string ToCamelCase(
        this string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.IsAlphaNumerics())
        {
            return input;
        }

        var newName = Regex.Replace(input, @"[^0-9a-zA-Z]", string.Empty);
        if (newName.All(char.IsUpper))
        {
            return newName.ToLowerInvariant();
        }

        newName = char.ToLowerInvariant(newName[0]) + newName[1..];
        return Regex.Replace(newName, @"(\s\w)", m => m.Value.ToUpper());
    }

    public static string ToPascalCase(
        this string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.IsAlphaNumerics())
        {
            return input;
        }

        var newName = Regex.Replace(input, @"[^0-9a-zA-Z]", string.Empty);
        return Regex.Replace(newName, @"(^\w)|(\s\w)|(\b\w)", m => m.Value.ToUpper());
    }
}