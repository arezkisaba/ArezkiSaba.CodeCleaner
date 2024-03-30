using ArezkiSaba.CodeCleaner.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class DocumentExtensions
{
    public static async Task<Document> StartTypeInferenceRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(new TypeInferenceRewriter(semanticModel).Visit(root));
    }

    public static async Task<Document> StartReadonlyModifierFieldRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(new ReadonlyModifierFieldRewriter(document.Project.Solution, semanticModel).Visit(root));
    }

    public static async Task<Document> StartUnusedMethodParameterDiscarderAsync(
        this Document document,
        Solution solution)
    {
        var semanticModel = await document.GetSemanticModelAsync();
        var documentEditor = await DocumentEditor.CreateAsync(document);

        var methodDeclarations = documentEditor.OriginalRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
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
            await RenameParameterNameIfUnreferencedAsync(solution, documentEditor, semanticModel, senderParameter, "_");
            var argsParameter = parameters[1];
            await RenameParameterNameIfUnreferencedAsync(solution, documentEditor, semanticModel, argsParameter, "__");
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartMethodDeclarationParameterLineBreakerAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new MethodDeclarationParameterLineBreaker().Visit(root));
    }

    public static async Task<Document> StartInvocationExpressionArgumentLineBreakerAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new InvocationExpressionArgumentLineBreaker().Visit(root));
    }

    public static async Task<Document> StartUsingDirectiveSorterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var compilationUnit = root as CompilationUnitSyntax;
        var sortedUsingDirectives = SyntaxFactory.List(compilationUnit.Usings
            .OrderBy(x => x.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) ? 1 : x.Alias == null ? 0 : 2)
            .ThenBy(x => x.Alias?.ToString())
            .ThenByDescending(x => x.Name.ToString().StartsWith(nameof(System) + "."))
            .ThenBy(x => x.Name.ToString())
        );
        compilationUnit = compilationUnit.WithUsings(sortedUsingDirectives);
        document = document.WithSyntaxRoot(compilationUnit);
        return document;
    }

    public static async Task<Document> StartFieldDeclarationSorterAsync(
        this Document document)
    {
        var semanticModel = await document.GetSemanticModelAsync();
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var forbiddenTypes = new List<SyntaxKind>()
        {
            SyntaxKind.BaseList,
            SyntaxKind.AttributeList,
            SyntaxKind.TypeParameterList,
            SyntaxKind.TypeParameterConstraintClause
        };

        var classDeclarationsToAdd = new List<(string className, List<FieldDeclarationSyntax> fieldDeclarations)>();
        foreach (var classDeclaration in documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var fieldDeclarationsToAdd = new List<FieldDeclarationSyntax>();
            foreach (var fieldDeclaration in classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                documentEditor.RemoveNode(fieldDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
                fieldDeclarationsToAdd.Add(fieldDeclaration);
            }

            classDeclarationsToAdd.Add((classDeclaration.Identifier.ValueText, fieldDeclarationsToAdd));
        }

        documentEditor = await DocumentEditor.CreateAsync(documentEditor.GetChangedDocument());

        var classDeclarations = documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        foreach (var (className, fieldDeclarations) in classDeclarationsToAdd)
        {
            var match = classDeclarations.FirstOrDefault(classDeclaration =>
            {
                return className == classDeclaration.Identifier.Text;
            });

            if (match != null)
            {
                var nodeToPlaceBefore = match.ChildNodes().FirstOrDefault(obj => forbiddenTypes.All(ft => !obj.IsKind(ft)));
                documentEditor.InsertBefore(
                    nodeToPlaceBefore,
                    fieldDeclarations
                        .OrderByDescending(obj => obj.Modifiers.Any(obj => obj.IsKind(SyntaxKind.StaticKeyword)))
                        .ThenByDescending(obj => obj.Modifiers.Any(obj => obj.IsKind(SyntaxKind.ReadOnlyKeyword)))
                        .Select(obj => SyntaxFactory.FieldDeclaration(
                            obj.AttributeLists,
                            obj.Modifiers,
                            obj.Declaration
                        )
                    )
                );
            }
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartDuplicatedUsingDirectiveRemoverAsync(
        this Document document)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var usingDirectives = documentEditor.OriginalRoot.ChildNodes().OfType<UsingDirectiveSyntax>().ToList();
        var usingDirectivesToRemove = new List<UsingDirectiveSyntax>();

        for (var i = 0; i < usingDirectives.Count; i++)
        {
            var hasSameUsingDirectives = usingDirectives.Skip(i + 1).Any(obj => obj.Name.ToString() == usingDirectives[i].Name.ToString());
            if (hasSameUsingDirectives)
            {
                usingDirectivesToRemove.Add(usingDirectives[i]);
            }
        }

        foreach (var usingDirectiveToRemove in usingDirectivesToRemove)
        {
            documentEditor.RemoveNode(usingDirectiveToRemove);
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartDuplicatedEmptyLinesRemoverAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new DuplicatedEmptyLinesRemover().Visit(root));
    }

    public static bool IsAutoGenerated(
        this Document document)
    {
        var fileName = Path.GetFileNameWithoutExtension(document.FilePath.AsSpan());
        return
            document.FilePath.Contains(".nuget", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(".AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".gi", StringComparison.OrdinalIgnoreCase);
    }

    #region Private use

    private static async Task RenameParameterNameIfUnreferencedAsync(
        Solution solution,
        DocumentEditor documentEditor,
        SemanticModel semanticModel,
        ParameterSyntax senderParameter,
        string discard)
    {
        var parameterSymbol = semanticModel.GetDeclaredSymbol(senderParameter);
        var parameterReferences = await SymbolFinder.FindReferencesAsync(parameterSymbol, solution);
        foreach (var parameterReference in parameterReferences)
        {
            var locations = parameterReference.Locations.ToList();
            if (!locations.Any())
            {
                var updatedParameter = SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(discard)
                ).WithType(senderParameter.Type);

                documentEditor.ReplaceNode(senderParameter, updatedParameter);
            }
        }
    }

    #endregion
}
