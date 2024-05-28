using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatLambdaExpression : RefactorOperationBase
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

            var expressions = documentEditor.OriginalRoot.DescendantNodes().OfType<LambdaExpressionSyntax>().ToList();
            foreach (var expression in expressions)
            {
                var newExpression = expression.Format();
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