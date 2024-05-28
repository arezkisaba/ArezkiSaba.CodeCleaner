using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatArgumentList : RefactorOperationBase
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

            var expressions = documentEditor.OriginalRoot.DescendantNodes().OfType<ExpressionSyntax>().ToList();
            foreach (var expression in expressions)
            {
                var argumentList = expression.FirstChildNode<ArgumentListSyntax>();
                ////var memberAccessExpression = expression.FirstChildNode<MemberAccessExpressionSyntax>();
                if (argumentList == null)
                {
                    continue;
                }

                var parentStatement = expression.FirstParentNode<StatementSyntax>();
                var imbricationLevel = expression.GetIndentCountbyImbrication();
                var newExpression = expression.Format(argumentList, parentStatement, imbricationLevel);
                if (!expression.IsEqualTo(newExpression))
                {
                    documentEditor.ReplaceNode(expression, newExpression);
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