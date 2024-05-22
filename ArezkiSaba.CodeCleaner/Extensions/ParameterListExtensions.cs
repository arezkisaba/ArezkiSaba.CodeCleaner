using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ParameterListExtensions
{
    public static ParameterListSyntax WithEndOfLineTriviaAfterCloseParen(
        this ParameterListSyntax parameterList)
    {
        return parameterList.WithCloseParenToken(
            parameterList.CloseParenToken.WithEndOfLineTrivia()
        );
    }
}
