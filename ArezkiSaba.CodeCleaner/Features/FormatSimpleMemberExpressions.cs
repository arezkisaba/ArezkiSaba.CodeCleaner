using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatSimpleMemberExpressions
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        bool isUpdated;
        DocumentEditor documentEditor;

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            var invocationExpressions = documentEditor.OriginalRoot
                .ChildNodes<InvocationExpressionSyntax>(recursive: true)
                .Where(obj => obj.Parent is MemberAccessExpressionSyntax)
                .ToList();
            foreach (var invocationExpression in invocationExpressions)
            {
                var newInvocationExpression = invocationExpression.WithEndOfLineTrivia<InvocationExpressionSyntax>();
                if (!invocationExpression.IsEqualTo(newInvocationExpression))
                {
                    documentEditor.ReplaceNode(invocationExpression, newInvocationExpression);
                    document = documentEditor.GetChangedDocument();
                    isUpdated = true;
                    break;
                }

                ////var itemAfter = invocationExpression.FirstChildToken();
                ////if (!itemAfter.IsKind(SyntaxKind.DotToken))
                ////{
                ////    continue;
                ////}

                ////var imbricationLevel = 0;
                ////var parentExpression = invocationExpression.Ancestors()
                ////    .Where(obj => obj.IsKind(SyntaxKind.ExpressionStatement) || obj.IsKind(SyntaxKind.Argument))
                ////    .FirstOrDefault();
                ////SyntaxTrivia? baseLeadingTrivia = null;
                ////if (parentExpression != null)
                ////{
                ////    baseLeadingTrivia = parentExpression.FindFirstLeadingTrivia();
                ////}

                ////if (baseLeadingTrivia == null)
                ////{
                ////    continue;
                ////}

                ////var leadingTrivias = SyntaxTriviaHelper.GetArgumentLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                ////var newInvocationExpression = invocationExpression.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
                ////var newDotToken = itemAfter.WithLeadingTrivia(leadingTrivias);

                ////var needLineBreak = true;
                ////if (needLineBreak)
                ////{
                ////    if (!invocationExpression.IsEqualTo(newInvocationExpression))
                ////    {
                ////        documentEditor.ReplaceNode(invocationExpression, newInvocationExpression);
                ////        document = documentEditor.GetChangedDocument();
                ////        isUpdated = true;
                ////    }

                ////    ////var nodeParent = itemAfter.Parent;
                ////    ////if (nodeParent != null)
                ////    ////{
                ////    ////    if (!itemAfter.IsEqualTo(newDotToken))
                ////    ////    {
                ////    ////        var newNodeParent = nodeParent.ReplaceToken(itemAfter, newDotToken);
                ////    ////        documentEditor.ReplaceNode(nodeParent, newNodeParent);
                ////    ////        document = documentEditor.GetChangedDocument();
                ////    ////        isUpdated = true;
                ////    ////    }
                ////    ////}

                ////    if (isUpdated)
                ////    {
                ////        break;
                ////    }
                ////}
            }
        } while (isUpdated);

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static int GetImbricationLevel(
        ExpressionSyntax expression)
    {
        var imbricationLevel = 0;
        var ancestors = expression.Ancestors().ToList();
        foreach (var ancestor in ancestors)
        {
            if (ancestor.IsKind(SyntaxKind.InvocationExpression) || ancestor.IsKind(SyntaxKind.Argument))
            {
                imbricationLevel++;
            }
            else if (ancestor is StatementSyntax)
            {
                break;
            }
        }

        return imbricationLevel;
    }
}