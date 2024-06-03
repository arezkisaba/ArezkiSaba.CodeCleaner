using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatEqualsValueClause : RefactorOperationBase
{
    public override string Name => nameof(FormatEqualsValueClause);

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

            var equalsValueClauses = documentEditor.OriginalRoot.DescendantNodes().OfType<EqualsValueClauseSyntax>().ToList();
            foreach (var equalsValueClause in equalsValueClauses)
            {
                if (equalsValueClause.Parent is not PropertyDeclarationSyntax parent)
                {
                    continue;
                }

                var imbricationLevel = equalsValueClause.GetIndentCountByImbrication();
                var newExpression = equalsValueClause.Format(parent, imbricationLevel);
                if (!equalsValueClause.IsEqualTo(newExpression))
                {
                    documentEditor.ReplaceNode(equalsValueClause, newExpression);
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