using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class BaseMethodDeclarationExtensions
{
    public static ParameterListSyntax Format(
        this ParameterListSyntax parameterList,
        SyntaxNode parentNode)
    {
        var needLineBreak = parameterList.Parameters.Any();

        if (needLineBreak)
        {
            parameterList = parameterList.WithOpenParenToken(parameterList.OpenParenToken.WithEndOfLineTrivia());
            parameterList = parameterList.ReplaceNodes(parameterList.Parameters, (parameter, __) => parameter.WithIndentationTrivia(parentNode));
            parameterList = parameterList.ReplaceTokens(parameterList.Parameters.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        }

        return parameterList.WithCloseParenToken(parameterList.CloseParenToken.WithoutLeadingTrivia());
    }
}
