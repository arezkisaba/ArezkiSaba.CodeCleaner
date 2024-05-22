using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ConstructorInitializerExtensions
{
    ////public static ConstructorInitializerSyntax FormatParameters(
    ////    this BaseMethodDeclarationSyntax baseMethodDeclaration,
    ////    ParameterListSyntax parameterList,
    ////    SyntaxNode parentNode)
    ////{
    ////    var newParametersList = baseMethodDeclaration.ParameterList.WithCloseParenToken(
    ////        baseMethodDeclaration.ParameterList.CloseParenToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine())
    ////    );
    ////    var newBaseMethodDeclaration = baseMethodDeclaration.WithParameterList(newParametersList);
    ////    if (!baseMethodDeclaration.IsEqualTo(newBaseMethodDeclaration))
    ////    {
    ////        documentEditor.ReplaceNode(baseMethodDeclaration, newBaseMethodDeclaration);
    ////        return (documentEditor.GetChangedDocument(), true);
    ////    }

    ////    var childToken = childNode.FirstChildToken(recursive: true);
    ////    var newChildNode = childNode.ReplaceToken(
    ////        childToken,
    ////        childToken.WithLeadingTrivia(
    ////            SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode, indentCount: 1)
    ////        )
    ////    );
    ////}
}
