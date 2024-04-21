using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class MemberDeclarationExtensions
{
    public static string GetName(
        this MemberDeclarationSyntax declaration)
    {
        switch (declaration)
        {
            case EventFieldDeclarationSyntax eventField:
                return eventField.Declaration.Variables.FirstOrDefault() != null ? eventField.Declaration.Variables.First().Identifier.Text : null;
            case FieldDeclarationSyntax field:
                return field.Declaration.Variables.FirstOrDefault() != null ? field.Declaration.Variables.First().Identifier.Text : null;
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

    public static MemberDeclarationSyntax RemoveAllTriviasFromParametersAndArguments(
        this MemberDeclarationSyntax declaration)
    {
        var parameterList = declaration.DescendantNodes().OfType<ParameterListSyntax>().FirstOrDefault();
        if (parameterList != null)
        {
            var newParameters = parameterList.Parameters.Select(p => p.WithoutTrivia());
            var newParameterList = parameterList.WithParameters(SyntaxFactory.SeparatedList(newParameters));
            newParameterList = newParameterList.WithOpenParenToken(newParameterList.OpenParenToken.WithoutTrivia());
            declaration = declaration.ReplaceNode(parameterList, newParameterList);
        }

        var argumentList = declaration.DescendantNodes().OfType<ArgumentListSyntax>().FirstOrDefault();
        if (argumentList != null)
        {
            var newParameters = argumentList.Arguments.Select(p => p.WithoutTrivia());
            var newArgumentList = argumentList.WithArguments(SyntaxFactory.SeparatedList(newParameters));
            newArgumentList = newArgumentList.WithOpenParenToken(newArgumentList.OpenParenToken.WithoutTrivia());
            declaration = declaration.ReplaceNode(argumentList, newArgumentList);
        }

        return declaration;
    }
}