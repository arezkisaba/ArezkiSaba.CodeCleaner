using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class MemberDeclarationExtensions
{
    public static string GetName(
        this MemberDeclarationSyntax member)
    {
        switch (member)
        {
            case EventFieldDeclarationSyntax eventField:
                return eventField.Declaration.Variables.First().Identifier.Text;
            case FieldDeclarationSyntax field:
                return field.Declaration.Variables.First().Identifier.Text;
            case PropertyDeclarationSyntax property:
                return property.Identifier.Text;
            case ConstructorDeclarationSyntax constructor:
                return constructor.Identifier.Text;
            case MethodDeclarationSyntax method:
                return method.Identifier.Text;
            default:
                return string.Empty;
        }
    }
}