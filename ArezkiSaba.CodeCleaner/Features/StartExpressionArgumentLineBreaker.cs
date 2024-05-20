using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class StartExpressionArgumentLineBreaker
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
            var expressions = documentEditor.OriginalRoot.ChildNodes<ExpressionSyntax>(recursive: true).Where(obj => obj.IsInvocationOrCreationExpression()).ToList();
            foreach (var expression in expressions)
            {
                var imbricationLevel = SyntaxTriviaHelper.GetImbricationLevel(expression);
                var parentExpression = expression.Ancestors().Where(obj => obj.IsImbricationExpression()).FirstOrDefault();

                SyntaxTrivia? baseLeadingTrivia = null;
                var parentStatement = expression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (parentStatement != null)
                {
                    baseLeadingTrivia = parentStatement.FindFirstLeadingTrivia();
                }

                if (baseLeadingTrivia == null)
                {
                    continue;
                }

                var argumentList = expression.GetArgumentList();
                var arguments = argumentList?.Arguments ?? Enumerable.Empty<ArgumentSyntax>();
                if (arguments != null && arguments.Any())
                {
                    var needLineBreak = expression.GetLength() > 100;
                    var newArgumentList = argumentList.ReplaceNodes(argumentList.Arguments, (argument, __) =>
                    {
                        if (needLineBreak)
                        {
                            var leadingTrivias = SyntaxTriviaHelper.GetArgumentLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                            argument = argument.WithLeadingTrivia(leadingTrivias).WithoutTrailingTrivia();
                        }

                        return argument;
                    });

                    if (needLineBreak)
                    {
                        var closeParenLeadingTrivia = SyntaxTriviaHelper.GetCloseBraceOrParenLeadingTrivia(baseLeadingTrivia, imbricationLevel);
                        var newExpression = expression.WithArgumentList(newArgumentList.WithEndOfLines(closeParenLeadingTrivia));
                        if (!expression.IsEqualTo(newExpression))
                        {
                            documentEditor.ReplaceNode(expression, newExpression);
                            document = documentEditor.GetChangedDocument();
                            isUpdated = true;
                            break;
                        }
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