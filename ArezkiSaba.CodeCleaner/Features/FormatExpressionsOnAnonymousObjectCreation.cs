using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatExpressionsOnAnonymousObjectCreation
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
            var anonymousObjectCreationExpressions = documentEditor.OriginalRoot.ChildNodes<AnonymousObjectCreationExpressionSyntax>(recursive: true).ToList();

            foreach (var anonymousObjectCreationExpression in anonymousObjectCreationExpressions)
            {
                var imbricationLevel = SyntaxTriviaHelper.GetImbricationLevel(anonymousObjectCreationExpression);

                SyntaxTrivia? baseLeadingTrivia = null;
                var baseStatement = anonymousObjectCreationExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (baseStatement != null)
                {
                    baseLeadingTrivia = baseStatement.FindFirstLeadingTrivia();
                }

                if (baseLeadingTrivia == null)
                {
                    continue;
                }

                var needLineBreak = true;
                var newAnonymousObjectCreationExpression = anonymousObjectCreationExpression.ReplaceNodes(anonymousObjectCreationExpression.Initializers, (childinitializer, __) =>
                {
                    if (needLineBreak)
                    {
                        var leadingTrivias = SyntaxTriviaHelper.GetArgumentLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                        childinitializer = childinitializer.WithLeadingTrivia(leadingTrivias).WithoutTrailingTrivia();
                    }

                    return childinitializer;
                });

                if (needLineBreak)
                {
                    var bracesLeadingTrivia = SyntaxTriviaHelper.GetCloseBraceOrParenLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                    newAnonymousObjectCreationExpression = newAnonymousObjectCreationExpression.WithEndOfLines(bracesLeadingTrivia);

                    var firstBraceToken = newAnonymousObjectCreationExpression.ChildTokens().FirstOrDefault(obj => obj.IsKind(SyntaxKind.OpenBraceToken));
                    var itemBefore = newAnonymousObjectCreationExpression.ItemBefore(firstBraceToken);
                    if (itemBefore.IsNode)
                    {
                        var targetNode = itemBefore.AsNode();
                        var targetToken = targetNode.LastChildToken<SyntaxToken>(recursive: true);
                        newAnonymousObjectCreationExpression = newAnonymousObjectCreationExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
                    }
                    else if (itemBefore.IsToken)
                    {
                        var targetToken = itemBefore.AsToken();
                        newAnonymousObjectCreationExpression = newAnonymousObjectCreationExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
                    }

                    if (!anonymousObjectCreationExpression.IsEqualTo(newAnonymousObjectCreationExpression))
                    {
                        documentEditor.ReplaceNode(anonymousObjectCreationExpression, newAnonymousObjectCreationExpression);
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