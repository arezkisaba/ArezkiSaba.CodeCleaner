using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatAssignmentExpression : RefactorOperationBase
{
    public override string Name => nameof(FormatAssignmentExpression);

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

            var assignmentExpressions = documentEditor.OriginalRoot.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToList();
            foreach (var assignmentExpression in assignmentExpressions)
            {
                var newExpression = assignmentExpression.Format();
                if (!assignmentExpression.IsEqualTo(newExpression))
                {
                    documentEditor.ReplaceNode(assignmentExpression, newExpression);
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