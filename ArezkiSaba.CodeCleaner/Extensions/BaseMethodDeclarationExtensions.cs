using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class BaseMethodDeclarationExtensions
{
    public static ParameterListSyntax FormatParameterList(
        this ParameterListSyntax parameterList,
        SyntaxNode parentNode)
    {
        parameterList = parameterList.WithOpenParenToken(parameterList.OpenParenToken.WithEndOfLineTrivia());
        parameterList = parameterList.ReplaceNodes(parameterList.Parameters, (parameter, __) => parameter.WithIndentationTrivia(parentNode));
        parameterList = parameterList.ReplaceTokens(parameterList.Parameters.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        parameterList = parameterList.WithCloseParenToken(parameterList.CloseParenToken.WithoutLeadingTrivia());
        return parameterList;
    }
}
