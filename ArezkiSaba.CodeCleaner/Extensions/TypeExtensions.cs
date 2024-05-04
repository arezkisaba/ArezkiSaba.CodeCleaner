using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class TypeExtensions
{
    public static string GetName(
        this TypeSyntax type)
    {
        switch (type)
        {
            case GenericNameSyntax genericNameSyntax:
                return genericNameSyntax.Identifier.Text;
            case IdentifierNameSyntax identifierNameSyntax:
                return identifierNameSyntax.Identifier.Text;
            default:
                return string.Empty;
        }
    }
}