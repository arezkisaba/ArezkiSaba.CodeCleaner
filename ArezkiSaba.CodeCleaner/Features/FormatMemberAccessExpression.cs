using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatMemberAccessExpression : RefactorOperationBase
{
    public override string Name => nameof(FormatArgumentList);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        bool isUpdated;
        DocumentEditor documentEditor;

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);

            var expressions = documentEditor.OriginalRoot.DescendantNodes().OfType<MemberAccessExpressionSyntax>().ToList();
            foreach (var expression in expressions)
            {
                var parentStatement = expression.FirstParentNode<StatementSyntax>();
                var parentExpression = expression.Parent as InvocationExpressionSyntax;
                var dotToken = expression.FirstChildToken();
                if (parentExpression == null || !dotToken.IsKind(SyntaxKind.DotToken))
                {
                    continue;
                }

                var imbricationLevel = expression.GetIndentCountByImbrication();
                var newParentExpression = parentExpression.Format(expression, parentStatement, imbricationLevel);
                if (!parentExpression.IsEqualTo(newParentExpression))
                {
                    documentEditor.ReplaceNode(parentExpression, newParentExpression);
                    document = documentEditor.GetChangedDocument();
                    isUpdated = true;
                    break;
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