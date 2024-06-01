using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class StringHelper
{
    public static string GenerateCharacterOccurences(
        char character,
        int count)
    {
        return new string(character, count);
    }
}