using ArezkiSaba.CodeCleaner.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;

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

    public static async Task<Document> StartEmptyLinesBracesRemoverAsync(
        this Document document)
    {
        bool isUpdated;
        DocumentEditor documentEditor;
        List<SyntaxToken> tokens = [];

        // After open brace

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            tokens = documentEditor.OriginalRoot.DescendantTokens().ToList();

            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                if (token.IsKind(SyntaxKind.OpenBraceToken))
                {
                    var targetToken = tokens[i + 1];
                    if (targetToken.HasLeadingTrivia)
                    {
                        var targetTokenUpdated = targetToken.WithLeadingTrivia();
                        var node = targetToken.Parent;
                        var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                        if (targetToken.HasLeadingTrivia != targetTokenUpdated.HasLeadingTrivia)
                        {
                            documentEditor.ReplaceNode(node, nodeUpdated);
                            document = documentEditor.GetChangedDocument();
                            isUpdated = true;
                            break;
                        }
                    }
                }
            }
        } while (isUpdated);

        // Before close brace

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            tokens = documentEditor.OriginalRoot.DescendantTokens().ToList();

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.IsKind(SyntaxKind.CloseBraceToken))
                {
                    var targetToken = token;
                    var targetTokenUpdated = targetToken.WithLeadingTrivia();
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (targetToken.HasLeadingTrivia != targetTokenUpdated.HasLeadingTrivia)
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
                }
            }
        } while (isUpdated);

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartDuplicatedEmptyLinesRemoverAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new DuplicatedEmptyLinesRemover().Visit(root));
    }

    public static async Task<Document> ReorderClassMembersAsync(
        this Document document)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);

        // Take interfaces, classes, structs
        var typeDeclarations = documentEditor.OriginalRoot.ChildNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var newTypeDeclaration = await GetSortedTypeDeclaration(documentEditor, typeDeclaration);
            documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);

            ////documentEditor = await DocumentEditor.CreateAsync(documentEditor.GetChangedDocument());

            ////var items1 = documentEditor.GetChangedRoot().DescendantNodes().Where(obj => ReferenceEquals(obj, newTypeDeclaration)).ToList();

            ////var newNewTypeDeclaration = documentEditor.GetTypeDeclarationWithTrivias(newTypeDeclaration);
            ////documentEditor.ReplaceNode(newTypeDeclaration, newNewTypeDeclaration);
        }

        return documentEditor.GetChangedDocument();
    }

    private static async Task<TypeDeclarationSyntax> GetSortedTypeDeclaration(
        DocumentEditor documentEditor,
        TypeDeclarationSyntax typeDeclarationRoot)
    {
        var declarationsToExtract = new List<SyntaxKind>()
        {
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
        };
        var indentationTrivia = typeDeclarationRoot.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        var memberDeclarationsReversed = typeDeclarationRoot.ChildNodes().OfType<MemberDeclarationSyntax>().Reverse().ToList();

        var memberDeclarationsToAdd = new List<MemberDeclarationSyntax>();
        foreach (var memberDeclaration in memberDeclarationsReversed)
        {
            var newMemberDeclaration = memberDeclaration;

            if (!declarationsToExtract.Any(newMemberDeclaration.IsKind))
            {
                continue;
            }

            if (newMemberDeclaration is TypeDeclarationSyntax typeDeclaration)
            {
                var newTypeDeclaration = await GetSortedTypeDeclaration(documentEditor, typeDeclaration);
                documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
                newMemberDeclaration = newTypeDeclaration;
            }

            memberDeclarationsToAdd.Add(newMemberDeclaration);
        }

        var orderedMemberDeclarations = new List<MemberDeclarationSyntax>();
        foreach (var declarationToExtract in declarationsToExtract)
        {
            orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarationsToAdd, declarationToExtract, indentationTrivia));
        }

        var newTypeDeclarationRoot = typeDeclarationRoot.WithMembers(new SyntaxList<MemberDeclarationSyntax>(orderedMemberDeclarations));
        ////newTypeDeclarationRoot = documentEditor.GetTypeDeclarationWithTrivias(newTypeDeclarationRoot);
        return newTypeDeclarationRoot;
    }

    public static async Task<Document> StartRegionInserterAsync(
        this Document document)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var declarations = documentEditor.OriginalRoot.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(obj => obj.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            .ToList();

        if (!declarations.Any())
        {
            return document;
        }

        var firstDeclaration = declarations.First();
        var lastDeclaration = declarations.Last();

        var leadingTrivia = firstDeclaration.GetLeadingTrivia()
            .Add(SyntaxFactory.Trivia(SyntaxFactory.RegionDirectiveTrivia(true)
                .WithTrailingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.Whitespace(" "),
                        SyntaxFactory.PreprocessingMessage("\"Private use\"")
                    )
                )
            ))
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetTab());
        var trailingTrivia = lastDeclaration.GetTrailingTrivia()
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetTab())
            .Add(SyntaxFactory.Trivia(SyntaxFactory.EndRegionDirectiveTrivia(true)))
            .Add(SyntaxTriviaHelper.GetEndOfLine());

        if (ReferenceEquals(firstDeclaration, lastDeclaration))
        {
            documentEditor.ReplaceNode(firstDeclaration, firstDeclaration
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia)
            );
        }
        else
        {
            documentEditor.ReplaceNode(
                firstDeclaration,
                firstDeclaration.WithLeadingTrivia(leadingTrivia)
            );
            documentEditor.ReplaceNode(
                lastDeclaration,
                lastDeclaration.WithTrailingTrivia(trailingTrivia)
            );
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

    public static async Task<Solution> StartUnusedMethodParameterRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

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
            newSolution = await RenameSymbolAsync(
                newSolution,
                symbol,
                name,
                newName
            );
        }

        return newSolution;
    }

    public static async Task<Solution> StartFieldRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

        var declarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>()
            .ToList();
        foreach (var declaration in declarations)
        {
            if (declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword) || m.IsKind(SyntaxKind.StaticKeyword)))
            {
                var name = declaration.GetName();
                if (char.IsUpper(name[0]))
                {
                    continue;
                }

                foreach (var variable in declaration.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable);
                    var newName = symbol.Name.ToPascalCase();
                    newSolution = await RenameSymbolAsync(
                        newSolution,
                        symbol,
                        name,
                        newName
                    );
                }
            }
            else
            {
                var name = declaration.GetName();
                if (name.StartsWith('_'))
                {
                    continue;
                }

                foreach (var variable in declaration.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable);
                    var newName = $"_{name}";
                    newSolution = await RenameSymbolAsync(
                        newSolution,
                        symbol,
                        name,
                        newName
                    );
                }
            }
        }

        return newSolution;
    }

    public static async Task<Solution> StartEventFieldRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

        var declarations = root.DescendantNodes().OfType<EventFieldDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (char.IsUpper(name[0]))
            {
                continue;
            }

            foreach (var variable in declaration.Declaration.Variables)
            {
                var symbol = semanticModel.GetDeclaredSymbol(variable);
                var newName = symbol.Name.ToPascalCase();
                newSolution = await RenameSymbolAsync(
                    newSolution,
                    symbol,
                    name,
                    newName
                );
            }
        }

        return newSolution;
    }

    public static async Task<Solution> StartPropertyRenamerAsync(
        this Document document,
        Solution solution)
    {
        if (document.Name.Contains("ViewModel"))
        {
            return solution;
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

        var declarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (char.IsUpper(name[0]))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = symbol.Name.ToPascalCase();
            newSolution = await RenameSymbolAsync(
                newSolution,
                symbol,
                name,
                newName
            );
        }

        return newSolution;
    }

    public static async Task<Solution> StartMethodRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

        var declarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (char.IsUpper(name[0]))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = symbol.Name.ToPascalCase();
            newSolution = await RenameSymbolAsync(
                newSolution,
                symbol,
                name,
                newName
            );
        }

        return newSolution;
    }

    public static async Task<Solution> StartLocalVariableRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

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
                newSolution = await Renamer.RenameSymbolAsync(
                    newSolution,
                    symbol,
                    new SymbolRenameOptions(),
                    newName
                );
            }
        }

        return newSolution;
    }

    public static async Task<Solution> StartParameterRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        var newSolution = solution;

        var declarations = root.DescendantNodes().OfType<ParameterSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            if (char.IsLower(symbol.Name[0]))
            {
                continue;
            }

            var newName = symbol.Name.ToCamelCase();
            newSolution = await Renamer.RenameSymbolAsync(
                newSolution,
                symbol,
                new SymbolRenameOptions(),
                newName
            );
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
        var symbol = semanticModel.GetDeclaredSymbol(senderParameter);
        var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
        foreach (var reference in references)
        {
            if (reference.Locations.Any())
            {
                continue;
            }

            newSolution = await Renamer.RenameSymbolAsync(
                newSolution,
                symbol,
                new SymbolRenameOptions(),
                discard
            );
        }

        return newSolution;
    }

    private static TypeDeclarationSyntax GetTypeDeclarationWithTrivias(
        this DocumentEditor documentEditor,
        TypeDeclarationSyntax typeDeclaration)
    {
        var memberDeclarations = typeDeclaration.ChildNodes().OfType<MemberDeclarationSyntax>().ToList();
        var memberDeclarationsReversed = memberDeclarations.ToList();
        memberDeclarationsReversed.Reverse();

        var i = 0;
        var canAddLeadingTrivia = false;
        var newTypeDeclaration = typeDeclaration;
        var declarationTrivia = newTypeDeclaration.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));

        var finalMemberDeclarations = new List<MemberDeclarationSyntax>();

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
                            SyntaxTriviaHelper.GetEndOfLine(),
                            declarationTrivia,
                            SyntaxTriviaHelper.GetTab()
                        )
                    );

                    var memberDeclarationWithTrivia = memberDeclaration.ReplaceToken(originalLeadingToken, leadingToken);
                    if (!memberDeclarationWithTrivia.IsEquivalentTo(memberDeclaration))
                    {
                        finalMemberDeclarations.Add(memberDeclarationWithTrivia);
                        ////documentEditor.ReplaceNode(memberDeclaration, memberDeclarationWithTrivia);
                    }
                    else
                    {
                        finalMemberDeclarations.Add(memberDeclaration);
                    }
                }
                else
                {
                    finalMemberDeclarations.Add(memberDeclaration);
                }
            }
            else
            {
                finalMemberDeclarations.Add(memberDeclaration);
            }

            i++;
        }

        return typeDeclaration.WithMembers(new SyntaxList<MemberDeclarationSyntax>(finalMemberDeclarations));
    }

    private static List<MemberDeclarationSyntax> GetMemberDeclarations(
        List<MemberDeclarationSyntax> memberDeclarations,
        SyntaxKind syntaxKind,
        SyntaxTrivia indentationTrivia)
    {
        var sortedemberDeclarations = memberDeclarations
            .Where(obj => obj.IsKind(syntaxKind))
            .OrderBy(obj => GetMemberDeclarationModifierRank(obj, syntaxKind))
            .ThenBy(obj => obj.GetName())
            .Select((obj, i) =>
            {
                return obj
                    .RemoveAllTriviasFromParametersAndArguments()
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(
                            indentationTrivia,
                            SyntaxTriviaHelper.GetTab()
                        ))
                    .WithTrailingTrivia(
                        SyntaxFactory.TriviaList(
                            SyntaxTriviaHelper.GetEndOfLine()
                        )
                    );
            });
        return sortedemberDeclarations.ToList();
    }

    private static int GetMemberDeclarationModifierRank(
        MemberDeclarationSyntax memberDeclaration,
        SyntaxKind syntaxKind)
    {
        var modifiers = memberDeclaration.Modifiers;

        if (syntaxKind == SyntaxKind.FieldDeclaration)
        {
            if (modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return 1;
            }
            else if (modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        if (modifiers.Any(SyntaxKind.PublicKeyword) &&
            modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return 1;
        }
        else if (modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return 2;
        }
        else if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return 3;
        }
        else if (modifiers.Any(SyntaxKind.InternalKeyword))
        {
            return 4;
        }
        else if (modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            return 5;
        }
        else if (modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return 6;
        }

        return 7;
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
