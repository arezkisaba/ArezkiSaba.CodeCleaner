using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SolutionExtensions
{
    public static async Task<(Document, Solution)> StartFieldRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            if (declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword) || m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var name = declaration.GetName();
                if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
                {
                    continue;
                }

                foreach (var variable in declaration.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable);
                    var newName = symbol.Name.ToPascalCase();
                    solution = await RenameSymbolAsync(
                        solution,
                        symbol,
                        name,
                        newName
                    );
                    document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
                }
            }
            else
            {
                var name = declaration.GetName();
                if (string.IsNullOrWhiteSpace(name) || name.StartsWith('_'))
                {
                    continue;
                }

                foreach (var variable in declaration.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable);
                    var newName = $"_{name}";
                    solution = await RenameSymbolAsync(
                        solution,
                        symbol,
                        name,
                        newName
                    );
                    document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
                }
            }
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartEventFieldRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<EventFieldDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
            {
                continue;
            }

            foreach (var variable in declaration.Declaration.Variables)
            {
                var symbol = semanticModel.GetDeclaredSymbol(variable);
                var newName = symbol.Name.ToPascalCase();
                solution = await RenameSymbolAsync(
                    solution,
                    symbol,
                    name,
                    newName
                );
                document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
            }
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartPropertyRenamerAsync(
        this Solution solution,
        Document document)
    {
        if (document.Name.Contains("ViewModel"))
        {
            return (document, solution);
        }

        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = symbol.Name.ToPascalCase();
            solution = await RenameSymbolAsync(
                solution,
                symbol,
                name,
                newName
            );
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartMethodRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = symbol.Name.ToPascalCase();
            solution = await RenameSymbolAsync(
                solution,
                symbol,
                name,
                newName
            );
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartLocalVariableRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().ToList();
        foreach (var localDeclaration in declarations)
        {
            foreach (var declaration in localDeclaration.Declaration.Variables)
            {
                var symbol = semanticModel.GetDeclaredSymbol(declaration);
                if (char.IsLower(symbol.Name[0]))
                {
                    continue;
                }

                var newName = symbol.Name.ToCamelCase();
                solution = await RenameSymbolAsync(
                    solution,
                    symbol,
                    symbol.Name,
                    newName
                );
                document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
            }
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartParameterRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<ParameterSyntax>()
            .Where(obj => !obj.Ancestors().Any(obj => obj.IsKind(SyntaxKind.RecordDeclaration)))
            .ToList();
        foreach (var declaration in declarations)
        {
            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            if (char.IsLower(symbol.Name[0]))
            {
                continue;
            }

            var newName = symbol.Name.ToCamelCase();
            solution = await RenameSymbolAsync(
                solution,
                symbol,
                symbol.Name,
                newName
            );
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartUnusedMethodParameterRenamerAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var methodDeclaration in methodDeclarations)
        {
            var parameters = methodDeclaration.DescendantNodes().OfType<ParameterSyntax>().ToList();
            if (parameters.Count != 2)
            {
                continue;
            }

            var senderParameterToken = parameters[0].DescendantTokens().OfType<SyntaxToken>()
                .LastOrDefault(obj => obj.ValueText == "sender");
            var argsParameterToken = parameters[1].DescendantTokens().OfType<SyntaxToken>()
                .FirstOrDefault(obj => obj.ValueText.EndsWith("Args"));

            var isCallbackMethod =
                !string.IsNullOrWhiteSpace(senderParameterToken.ValueText) &&
                !string.IsNullOrWhiteSpace(argsParameterToken.ValueText);

            if (!isCallbackMethod)
            {
                continue;
            }

            var senderParameter = parameters[0];
            solution = await RenameParameterNameIfUnreferencedAsync(solution, semanticModel, senderParameter, "_");
            var argsParameter = parameters[1];
            solution = await RenameParameterNameIfUnreferencedAsync(solution, semanticModel, argsParameter, "__");
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> StartAsyncMethodRenamerAsync(
        this Solution solution,
        Document document)
    {
        if (document.SkipProgramEntryPoint())
        {
            return (document, solution);
        }

        ////document = await Formatter.FormatAsync(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var asyncSuffix = "Async";
        var declarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.Identifier.ValueText;
            var hasAsyncSuffix = name.EndsWith(asyncSuffix);
            var hasAsyncKeyword = declaration.ChildTokens().Any(obj => obj.IsKind(SyntaxKind.AsyncKeyword));
            var hasTaskReturnTypeKeyword = declaration.ChildNodes().Any(node =>
            {
                return node.ChildTokens().Any(token => token.ValueText == "Task");
            });
            if (hasAsyncSuffix || (!hasAsyncKeyword && !hasTaskReturnTypeKeyword))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = $"{declaration.Identifier.ValueText}{asyncSuffix}";
            solution = await RenameSymbolAsync(
                solution,
                symbol,
                name,
                newName
            );
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return (document, solution);
    }

    public static async Task<(Document, Solution)> ReorderFieldsWithPropfullPropertiesAsync(
        this Solution solution,
        Document document)
    {
        ////document = await Formatter.FormatAsync(document);
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var root = documentEditor.GetDocumentEditorRoot();
        var semanticModel = await document.GetSemanticModelAsync();

        var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var fieldDeclarations = typeDeclaration.ChildNodes().OfType<FieldDeclarationSyntax>()
                .Reverse()
                .ToList();
            foreach (var fieldDeclaration in fieldDeclarations)
            {
                var variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                var symbol = semanticModel.GetDeclaredSymbol(variableDeclarator);
                var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
                foreach (var reference in references)
                {
                    foreach (var location in reference.Locations)
                    { 
                        var documentRoot = await location.Document.GetSyntaxRootAsync();
                        var referencedNode = documentRoot.FindNode(location.Location.SourceSpan);
                        var propertyDeclaration = referencedNode.Ancestors()
                            .OfType<PropertyDeclarationSyntax>()
                            .FirstOrDefault();

                        var fieldName = fieldDeclaration.GetName();
                        var propertyName = propertyDeclaration.GetName();

                        if (fieldName.ToPascalCase() != propertyName)
                        {
                            continue;
                        }

                        var isReferencedFromProperty = propertyDeclaration != null;
                        if (isReferencedFromProperty)
                        {
                            var indentationTrivias = propertyDeclaration.GetLeadingTrivia()
                                .Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia))
                                .ToList();
                            documentEditor.InsertBefore(
                                propertyDeclaration,
                                fieldDeclaration
                                    .WithLeadingTrivia(
                                        SyntaxFactory.TriviaList()
                                            .Add(SyntaxTriviaHelper.GetEndOfLine())
                                            .AddRange(indentationTrivias)
                                    )
                                    .WithoutTrailingTrivia()
                            );
                            documentEditor.RemoveNode(fieldDeclaration);
                            break;
                        }
                    }
                }
            }
        }

        document = documentEditor.GetChangedDocument();
        solution = document.Project.Solution;

        return (document, solution);
    }

    #region Private use

    private static async Task<Solution> RenameParameterNameIfUnreferencedAsync(
        Solution solution,
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

    private static async Task<Solution> RenameSymbolAsync(
        Solution newSolution,
        ISymbol symbol,
        string name,
        string newName)
    {
        try
        {
            newSolution = await Renamer.RenameSymbolAsync(
                newSolution,
                symbol,
                new SymbolRenameOptions(),
                newName
            );
            return newSolution;
        }
        catch (Exception)
        {
            Console.WriteLine($"Failed to rename '{name}' to '{newName}'", ConsoleColor.Red);
            return newSolution;
        }
    }

    #endregion
}
