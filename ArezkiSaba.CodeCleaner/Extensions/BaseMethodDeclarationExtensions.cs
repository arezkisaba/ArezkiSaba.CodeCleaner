using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class BaseMethodDeclarationExtensions
{
    public static BaseMethodDeclarationSyntax FormatParameters(
        this BaseMethodDeclarationSyntax baseMethodDeclaration,
        ParameterListSyntax parameterList,
        SyntaxNode parentNode)
    {
        var newParameterList = parameterList;
        newParameterList = newParameterList.WithOpenParenToken(newParameterList.OpenParenToken.WithEndOfLineTrivia());
        newParameterList = newParameterList.ReplaceNodes(newParameterList.Parameters, (parameter, __) => parameter.WithIndentationTrivia(parentNode));
        newParameterList = newParameterList.ReplaceTokens(newParameterList.Parameters.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        newParameterList = newParameterList.WithCloseParenToken(newParameterList.CloseParenToken.WithoutLeadingTrivia());
        return baseMethodDeclaration.WithParameterList(newParameterList);
    }
}
