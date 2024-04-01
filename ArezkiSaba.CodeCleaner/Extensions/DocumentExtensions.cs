using ArezkiSaba.CodeCleaner.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class DocumentExtensions
{
    public static async Task<Document> StartTypeInferenceRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new TypeInferenceRewriter().Visit(root));
    }

    public static async Task<Document> StartReadonlyModifierFieldRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(new ReadonlyModifierFieldRewriter(document.Project.Solution, semanticModel).Visit(root));
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

    public static async Task<Document> StartDuplicatedMethodEmptyLinesRemoverAsync(
        this Document document)
    {
        // After open brace

        var documentEditor = await DocumentEditor.CreateAsync(document);
        var allNodes = documentEditor.OriginalRoot.DescendantNodes().ToList();
        foreach (var node in allNodes)
        {
            if (!node.IsKind(SyntaxKind.ConstructorDeclaration) &&
                !node.IsKind(SyntaxKind.MethodDeclaration))
            {
                continue;
            }

            var block = node.DescendantNodes().Single(obj => obj.IsKind(SyntaxKind.Block));
            var blockTokens = block.DescendantTokens();
            if (blockTokens.Count() == 2) // OpenBrace + CloseBrace
            {
                continue;
            }

            var originalFirstToken = blockTokens.ElementAt(1);
            var updatedFirstToken = originalFirstToken.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                )
            );

            var nodeWithTrivia = node.ReplaceToken(originalFirstToken, updatedFirstToken);
            if (!nodeWithTrivia.IsEquivalentTo(node))
            {
                documentEditor.ReplaceNode(node, nodeWithTrivia);
            }
        }

        // Before close brace

        documentEditor = await DocumentEditor.CreateAsync(documentEditor.GetChangedDocument());
        allNodes = documentEditor.OriginalRoot.DescendantNodes().ToList();
        foreach (var node in allNodes)
        {
            if (!node.IsKind(SyntaxKind.ConstructorDeclaration) &&
                !node.IsKind(SyntaxKind.MethodDeclaration))
            {
                continue;
            }

            var block = node.DescendantNodes().Single(obj => obj.IsKind(SyntaxKind.Block));
            var blockTokens = block.DescendantTokens();
            if (blockTokens.Count() == 2) // OpenBrace + CloseBrace
            {
                continue;
            }

            var originalCloseBrace = node.DescendantTokens().LastOrDefault(obj => obj.IsKind(SyntaxKind.CloseBraceToken));
            var updatedCloseBrace = originalCloseBrace.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                )
            );

            var nodeWithTrivia = node.ReplaceToken(originalCloseBrace, updatedCloseBrace);
            if (!nodeWithTrivia.IsEquivalentTo(node))
            {
                documentEditor.ReplaceNode(node, nodeWithTrivia);
            }
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> ReorderClassMembersAsync(
        this Document document)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var memberDeclarationsWithClassName = documentEditor.RemoveAndExtractMemberDeclarationsFromClass();
        documentEditor = await DocumentEditor.CreateAsync(documentEditor.GetChangedDocument());
        documentEditor.InsertOrderedMemberDeclarationsIntoClass(memberDeclarationsWithClassName);
        documentEditor = await DocumentEditor.CreateAsync(documentEditor.GetChangedDocument());
        documentEditor.AddLeadingTriviaToMemberDeclarations();
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

    public static async Task<Solution> StartUnusedMethodParameterRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;
        var newDocument = document;

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
            newSolution = await RenameParameterNameIfUnreferencedAsync(newSolution, semanticModel, senderParameter, "_");
            var argsParameter = parameters[1];
            newSolution = await RenameParameterNameIfUnreferencedAsync(newSolution, semanticModel, argsParameter, "__");
        }

        return newSolution;
    }

    public static async Task<Solution> StartAsyncMethodRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;
        var newDocument = document;

        var asyncSuffix = "Async";
        var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var methodDeclaration in methodDeclarations)
        {
            var hasAsyncKeyword = methodDeclaration.ChildTokens().Any(obj => obj.IsKind(SyntaxKind.AsyncKeyword));
            var hasAsyncSuffix = methodDeclaration.Identifier.ValueText.EndsWith(asyncSuffix);
            if (!hasAsyncKeyword || hasAsyncSuffix)
            {
                continue;
            }

            var methodDeclarationSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            var methodDeclarationReferences = await SymbolFinder.FindReferencesAsync(methodDeclarationSymbol, solution);
            foreach (var parameterReference in methodDeclarationReferences)
            {
                newSolution = await Renamer.RenameSymbolAsync(
                    newSolution,
                    methodDeclarationSymbol,
                    new SymbolRenameOptions(),
                    $"{methodDeclaration.Identifier.ValueText}{asyncSuffix}"
                );
            }
        }

        return newSolution;
    }

    public static bool IsAutoGenerated(
        this Document document)
    {
        var fileName = Path.GetFileNameWithoutExtension(document.FilePath.AsSpan());
        var isFilePathAutoGenerated =
            document.FilePath?.Contains(".nuget", StringComparison.OrdinalIgnoreCase) ?? false;
        var isFileNameAutoGenerated =
            fileName.Contains(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(".AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".gi", StringComparison.OrdinalIgnoreCase);
        return isFilePathAutoGenerated || isFileNameAutoGenerated;
    }

    #region Private use

    private static async Task<Solution> RenameParameterNameIfUnreferencedAsync(
        Solution solution,
        SemanticModel semanticModel,
        ParameterSyntax senderParameter,
        string discard)
    {
        var newSolution = solution;
        var parameterSymbol = semanticModel.GetDeclaredSymbol(senderParameter);
        var parameterReferences = await SymbolFinder.FindReferencesAsync(parameterSymbol, solution);
        foreach (var parameterReference in parameterReferences)
        {
            var locations = parameterReference.Locations.ToList();
            if (locations.Any())
            {
                continue;
            }

            newSolution = await Renamer.RenameSymbolAsync(newSolution, parameterSymbol, new SymbolRenameOptions(), discard);
        }

        return newSolution;
    }

    private static List<(string className, List<MemberDeclarationSyntax> memberDeclarations)> RemoveAndExtractMemberDeclarationsFromClass(
        this DocumentEditor documentEditor)
    {
        var declarationsToExtract = new List<SyntaxKind>()
        {
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.MethodDeclaration
        };
        var classDeclarationsToAdd = new List<(string className, List<MemberDeclarationSyntax> memberDeclarations)>();
        foreach (var classDeclaration in documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var memberDeclarationsToAdd = new List<MemberDeclarationSyntax>();
            foreach (var node in classDeclaration.DescendantNodes())
            {
                if (!declarationsToExtract.Any(obj => node.IsKind(obj)))
                {
                    continue;
                }

                documentEditor.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
                memberDeclarationsToAdd.Add(node as MemberDeclarationSyntax);
            }

            classDeclarationsToAdd.Add((classDeclaration.Identifier.ValueText, memberDeclarationsToAdd));
        }

        return classDeclarationsToAdd;
    }

    private static void AddLeadingTriviaToMemberDeclarations(
        this DocumentEditor documentEditor)
    {
        var classDeclarations = documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        foreach (var classDeclaration in classDeclarations)
        {
            var memberDeclarations = classDeclaration.DescendantNodes().OfType<MemberDeclarationSyntax>().ToList();
            var memberDeclarationsReversed = memberDeclarations.ToList();
            memberDeclarationsReversed.Reverse();

            var i = 0;
            var canAddLeadingTrivia = false;
            foreach (var memberDeclaration in memberDeclarationsReversed)
            {
                if (i != memberDeclarations.Count - 1)
                {
                    canAddLeadingTrivia = memberDeclaration.CanAddLeadingTrivia(memberDeclarations);
                    if (canAddLeadingTrivia)
                    {
                        var originalLeadingToken = memberDeclaration.GetFirstToken();
                        var leadingToken = originalLeadingToken.WithLeadingTrivia(
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.EndOfLine(Environment.NewLine)
                            )
                        );

                        var memberDeclarationWithTrivia = memberDeclaration.ReplaceToken(originalLeadingToken, leadingToken);
                        if (!memberDeclarationWithTrivia.IsEquivalentTo(memberDeclaration))
                        {
                            documentEditor.ReplaceNode(memberDeclaration, memberDeclarationWithTrivia);
                        }
                    }
                }

                i++;
            }
        }
    }

    public static void InsertOrderedMemberDeclarationsIntoClass(
        this DocumentEditor documentEditor,
        List<(string className, List<MemberDeclarationSyntax> memberDeclarations)> memberDeclarationsWithClassName)
    {
        var classDeclarations = documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        foreach (var (className, memberDeclarations) in memberDeclarationsWithClassName)
        {
            var match = classDeclarations.FirstOrDefault(classDeclaration =>
            {
                return className == classDeclaration.Identifier.Text;
            });
            if (match != null)
            {
                var orderedMemberDeclarations = new List<MemberDeclarationSyntax>();
                orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarations, SyntaxKind.EventFieldDeclaration));
                orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarations, SyntaxKind.FieldDeclaration));
                orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarations, SyntaxKind.PropertyDeclaration));
                orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarations, SyntaxKind.ConstructorDeclaration));
                orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarations, SyntaxKind.MethodDeclaration));
                documentEditor.InsertMembers(match, 0, orderedMemberDeclarations);
            }
        }
    }

    private static List<MemberDeclarationSyntax> GetMemberDeclarations(
        List<MemberDeclarationSyntax> memberDeclarations,
        SyntaxKind syntaxKind)
    {
        var filteredMethodDeclarations = memberDeclarations
            .Where(obj => obj.IsKind(syntaxKind))
            .ToList();
        filteredMethodDeclarations = filteredMethodDeclarations
            .OrderByDescending(obj => obj.Modifiers.Any(obj => obj.IsKind(SyntaxKind.StaticKeyword)))
            .ThenByDescending(obj => obj.Modifiers.Any(obj => obj.IsKind(SyntaxKind.ReadOnlyKeyword)))
            .Select(
                (obj, i) =>
                {
                    var memberDeclaration = obj.WithoutLeadingTrivia();
                    memberDeclaration = memberDeclaration.WithTrailingTrivia(
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.EndOfLine(Environment.NewLine)
                        )
                    );
                    return memberDeclaration;
                }
            )
            .ToList();
        return filteredMethodDeclarations;
    }

    private static bool CanAddLeadingTrivia(
        this MemberDeclarationSyntax memberDeclaration,
        List<MemberDeclarationSyntax> memberDeclarations)
    {
        var canAddLeadingTrivia = false;
        if (memberDeclaration is EventFieldDeclarationSyntax eventFieldDeclaration)
        {
            var firstMemberDeclaration = memberDeclarations.First(obj => obj.IsKind(SyntaxKind.EventFieldDeclaration));
            canAddLeadingTrivia = eventFieldDeclaration.IsEquivalentTo(firstMemberDeclaration);
        }
        else if (memberDeclaration is FieldDeclarationSyntax fieldDeclaration)
        {
            var firstMemberDeclaration = memberDeclarations.First(obj => obj.IsKind(SyntaxKind.FieldDeclaration));
            canAddLeadingTrivia = fieldDeclaration.IsEquivalentTo(firstMemberDeclaration);
        }
        else if (memberDeclaration is PropertyDeclarationSyntax _)
        {
            canAddLeadingTrivia = true;
        }
        else if (memberDeclaration is ConstructorDeclarationSyntax _)
        {
            canAddLeadingTrivia = true;
        }
        else if (memberDeclaration is MethodDeclarationSyntax _)
        {
            canAddLeadingTrivia = true;
        }

        return canAddLeadingTrivia;
    }

    private record class ClassMemberModel(
        string ClassName,
        List<MemberDeclarationSyntax> MemberDeclarations
    );

    #endregion
}