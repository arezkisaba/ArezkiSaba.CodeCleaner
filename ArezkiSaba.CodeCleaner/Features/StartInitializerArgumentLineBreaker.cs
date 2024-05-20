using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class StartInitializerArgumentLineBreaker
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
            var initializerExpressions = documentEditor.OriginalRoot.ChildNodes<InitializerExpressionSyntax>(recursive: true).ToList();

            foreach (var initializerExpression in initializerExpressions)
            {
                var parentExpression = (ExpressionSyntax)initializerExpression.Parent;
                var imbricationLevel = SyntaxTriviaHelper.GetImbricationLevel(parentExpression);

                SyntaxTrivia? baseLeadingTrivia = null;
                var baseStatement = initializerExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (baseStatement != null)
                {
                    baseLeadingTrivia = baseStatement.FindFirstLeadingTrivia();
                }

                if (baseLeadingTrivia == null)
                {
                    continue;
                }

                var needLineBreak = true;
                var newInitializerExpression = initializerExpression.ReplaceNodes(initializerExpression.Expressions, (childExpression, __) =>
                {
                    if (needLineBreak)
                    {
                        var leadingTrivias = SyntaxTriviaHelper.GetArgumentLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                        childExpression = childExpression.WithLeadingTrivia(leadingTrivias).WithoutTrailingTrivia();
                    }

                    return childExpression;
                });

                if (needLineBreak)
                {
                    var bracesLeadingTrivia = SyntaxTriviaHelper.GetCloseBraceOrParenLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                    newInitializerExpression = newInitializerExpression.WithEndOfLines(bracesLeadingTrivia);
                    var newParentExpression = parentExpression.WithInitializer(newInitializerExpression);
                    var itemBefore = newParentExpression.ItemBefore(newInitializerExpression);
                    if (itemBefore.IsNode)
                    {
                        var targetNode = itemBefore.AsNode();
                        var targetToken = targetNode.LastChildToken<SyntaxToken>(recursive: false);
                        newParentExpression = newParentExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
                    }
                    else if (itemBefore.IsToken)
                    {
                        var targetToken = itemBefore.AsToken();
                        newParentExpression = newParentExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
                    }

                    if (!parentExpression.IsEqualTo(newParentExpression))
                    {
                        documentEditor.ReplaceNode(parentExpression, newParentExpression);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
                }
            }
        } while (isUpdated);

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}