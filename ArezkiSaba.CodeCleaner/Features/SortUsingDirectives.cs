using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class SortUsingDirectives : RefactorOperationBase
{
    public override string Name => nameof(SortUsingDirectives);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var compilationUnit = root as CompilationUnitSyntax;
        var sortedUsingDirectives = SyntaxFactory.List(compilationUnit.Usings
            .OrderBy(x => GetUsingDirectiveRank(x))
            .ThenBy(x => x.Name.ToString().GetAlphaNumerics())
        );
        compilationUnit = compilationUnit.WithUsings(sortedUsingDirectives);
        document = document.WithSyntaxRoot(compilationUnit);
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    #region Private use

    private int GetUsingDirectiveRank(
        UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias == null)
        {
            if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
            {
                return 7;
            }
            else if (usingDirective.Name.ToString().StartsWith("System"))
            {
                return 1;
            }
            else if (usingDirective.Name.ToString().StartsWith("Microsoft"))
            {
                return 2;
            }
            else if (usingDirective.Name.ToString().StartsWith("Windows"))
            {
                return 3;
            }
            else if (usingDirective.Name.ToString().StartsWith("Prevoir.Toolkit"))
            {
                return 5;
            }
            else if (usingDirective.Name.ToString().StartsWith("Prevoir."))
            {
                return 6;
            }
            else
            {
                return 4;
            }
        }
        else
        {
            return 8;
        }
    }

    #endregion
}