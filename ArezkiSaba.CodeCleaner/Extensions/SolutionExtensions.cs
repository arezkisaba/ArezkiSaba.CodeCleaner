using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SolutionExtensions
{
    public static async Task<IList<TypeDeclarationSyntax>> GetAllTypeDeclarationsAsync(
        this Solution solution)
    {
        var typeDeclarations = new List<TypeDeclarationSyntax>();

        foreach (var project in solution.Projects)
        {
            foreach (var currentDocument in project.Documents)
            {
                var syntaxTree = await currentDocument.GetSyntaxTreeAsync();
                if (syntaxTree != null && syntaxTree.Options.Kind == SourceCodeKind.Regular)
                {
                    var currentRoot = await syntaxTree.GetRootAsync();
                    var typeDeclarationsInDocument = currentRoot.DescendantNodes().OfType<TypeDeclarationSyntax>();
                    typeDeclarations.AddRange(typeDeclarationsInDocument);
                }
            }
        }

        return typeDeclarations;
    }

    public static async Task<Solution> RenameSymbolAsync(
        this Solution solution,
        ISymbol symbol,
        string name,
        string newName)
    {
        try
        {
            solution = await Renamer.RenameSymbolAsync(
                solution,
                symbol,
                new SymbolRenameOptions(),
                newName
            );
            return solution;
        }
        catch (Exception)
        {
            Console.WriteLine($"Failed to rename '{name}' to '{newName}'", ConsoleColor.Red);
            return solution;
        }
    }

    public static async Task<Solution> RenameParameterNameIfUnreferencedAsync(
        this Solution solution,
        SemanticModel semanticModel,
        ParameterSyntax senderParameter,
        string discard)
    {
        var newSolution = solution;
        var symbol = semanticModel.GetDeclaredSymbol(senderParameter);
        var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
        foreach (var reference in references)
        {
            if (reference.Locations.Any())
            {
                continue;
            }

            newSolution = await RenameSymbolAsync(
                newSolution,
                symbol,
                symbol.Name,
                discard
            );
        }

        return newSolution;
    }
}
