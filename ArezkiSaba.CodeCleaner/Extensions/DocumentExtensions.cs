using ArezkiSaba.CodeCleaner.Models;
using ArezkiSaba.CodeCleaner.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Newtonsoft.Json.Linq;
using System.Drawing.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class DocumentExtensions
{
    private record ToReplaceEntry(SyntaxNode Where, SyntaxToken From, SyntaxToken To);

    public static async Task<RefactorOperationResult> StartRegionRemoverAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new RegionRemover(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartReadonlyModifierFieldRewriterAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new ReadonlyModifierFieldRewriter(
                document.Project.Solution,
                semanticModel
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartSealedModifierClassRewriterAsync(
        this Document document,
        Solution solution)
    {
        var allTypeDeclarations = await GetAllTypeDeclarations(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new SealedModifierClassRewriter(
                document.Project.Solution,
                semanticModel,
                allTypeDeclarations
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> ReorderClassMembersAsync(
        this Document document,
        Solution solution)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var root = documentEditor.GetDocumentEditorRoot();

        // Take interfaces, classes, structs
        var typeDeclarations = root.ChildNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var newTypeDeclaration = await GetSortedTypeDeclaration(documentEditor, typeDeclaration);
            documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> ReorderFieldsWithPropfullPropertiesAsync(
        this Document document,
        Solution solution)
    {
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
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartTypeInferenceRewriterAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new TypeInferenceRewriter(
                semanticModel
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartUsingDirectiveSorterAsync(
        this Document document,
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

    public static async Task<RefactorOperationResult> StartDuplicatedUsingDirectiveRemoverAsync(
        this Document document,
        Solution solution)
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

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartDuplicatedEmptyLinesRemoverAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        document = document.WithSyntaxRoot(
            new DuplicatedEmptyLinesRemover(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartEmptyLinesBracesRemoverAsync(
        this Document document,
        Solution solution)
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
                    var leadingTrivias = new List<SyntaxTrivia>();
                    for (var j = 0; j < targetToken.LeadingTrivia.Count; j++)
                    {
                        var leadingTrivia = targetToken.LeadingTrivia[j];
                        if (!leadingTrivia.IsKind(SyntaxKind.EndOfLineTrivia) &&
                            !leadingTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) &&
                            !leadingTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                        {
                            leadingTrivias.Add(leadingTrivia);
                        }
                    }

                    var targetTokenUpdated = targetToken.WithLeadingTrivia(leadingTrivias);
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (targetToken.LeadingTrivia.Count != targetTokenUpdated.LeadingTrivia.Count)
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
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
                    var leadingTrivias = new List<SyntaxTrivia>();
                    for (var j = 0; j < targetToken.LeadingTrivia.Count; j++)
                    {
                        var leadingTrivia = targetToken.LeadingTrivia[j];
                        if (!leadingTrivia.IsKind(SyntaxKind.EndOfLineTrivia) &&
                            !leadingTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) &&
                            !leadingTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                        {
                            leadingTrivias.Add(leadingTrivia);
                        }
                    }

                    var targetTokenUpdated = targetToken.WithLeadingTrivia(leadingTrivias);
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (targetToken.LeadingTrivia.Count != targetTokenUpdated.LeadingTrivia.Count)
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
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

    public static async Task<RefactorOperationResult> StartRegionInserterAsync(
        this Document document,
        Solution solution)
    {
        if (document.SkipProgramEntryPoint())
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                document.Project.Solution
            );
        }

        var documentEditor = await DocumentEditor.CreateAsync(document);
        var declarations = documentEditor.OriginalRoot.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(obj => obj.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            .ToList();

        if (!declarations.Any())
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                document.Project.Solution
            );
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

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartMethodDeclarationParameterLineBreakerAsync(
        this Document document,
        Solution solution)
    {
        var excludedTypes = new List<Type>
        {
            typeof(LocalFunctionStatementSyntax),
            typeof(SimpleLambdaExpressionSyntax),
            typeof(ParenthesizedLambdaExpressionSyntax)
        };

        var documentEditor = await DocumentEditor.CreateAsync(document);
        var declarations = documentEditor.GetDocumentEditorRoot().FindAllConstructorAndMethodDeclarations();
        foreach (var declaration in declarations)
        {
            var baseLeadingTrivia = declaration.FindFirstLeadingTrivia();
            if (baseLeadingTrivia == null)
            {
                continue;
            }

            var hasExcludedType = declaration.DescendantNodes()
                .Any(node => excludedTypes.Any(excludedType => node.GetType() == excludedType));
            if (hasExcludedType)
            {
                continue;
            }

            var needLineBreak = true;
            var parametersList = declaration.ParameterList;
            var newParameters = new List<ParameterSyntax>();
            for (var i = 0; i < parametersList.Parameters.Count; i++)
            {
                var parameter = parametersList.Parameters[i];
                if (needLineBreak)
                {
                    parameter = parameter.WithLeadingTrivia(
                        SyntaxTriviaHelper.GetEndOfLine(),
                        baseLeadingTrivia.Value,
                        SyntaxTriviaHelper.GetTab()
                    );
                }

                newParameters.Add(parameter);
            }

            var newParametersList = parametersList.WithParameters(
                SyntaxFactory.SeparatedList(newParameters)
            );
            var newMethodDeclaration = declaration.WithParameterList(newParametersList);
            documentEditor.ReplaceNode(declaration, newMethodDeclaration);
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    public static async Task<RefactorOperationResult> StartExpressionArgumentLineBreakerAsync(
        this Document document,
        Solution solution)
    {
        var excludedTypes = new List<Type>
        {
            typeof(LocalFunctionStatementSyntax),
            typeof(SimpleLambdaExpressionSyntax),
            typeof(ParenthesizedLambdaExpressionSyntax)
        };


        bool isUpdated;
        DocumentEditor documentEditor;

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            var expressions = documentEditor.OriginalRoot.FindAllInvocationAndCreationExpressions();

            foreach (var expression in expressions)
            {
                SyntaxTrivia? baseLeadingTrivia = null;

                var imbricationLevel = 0;
                var parentExpression = expression.Ancestors().Where(obj => obj.IsInvocationOrCreationExpression()).FirstOrDefault();
                if (parentExpression != null)
                {
                    var ancestors = expression.Ancestors().ToList();
                    foreach (var ancestor in ancestors)
                    {
                        if (ancestor.IsInvocationOrCreationExpression())
                        {
                            imbricationLevel++;
                        }
                        else if (ancestor is StatementSyntax)
                        {
                            break;
                        }
                    }
                }

                var parentStatement = expression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (parentStatement != null)
                {
                    baseLeadingTrivia = parentStatement.FindFirstLeadingTrivia();
                }

                if (baseLeadingTrivia != null)
                {
                    var hasExcludedType = expression.DescendantNodes()
                        .Any(node => excludedTypes.Any(excludedType => node.GetType() == excludedType));
                    if (hasExcludedType)
                    {
                        continue;
                    }

                    var needLineBreak = expression.GetExpressionLength() > 100;
                    var argumentList = expression.GetArgumentList();
                    if (argumentList == null || !argumentList.Arguments.Any())
                    {
                        continue;
                    }

                    var newArguments = new List<ArgumentSyntax>();
                    for (var i = 0; i < argumentList.Arguments.Count; i++)
                    {
                        var argument = argumentList.Arguments[i];
                        if (needLineBreak)
                        {
                            var leadingTrivias = new List<SyntaxTrivia>();
                            leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
                            if (baseLeadingTrivia != null)
                            {
                                leadingTrivias.Add(baseLeadingTrivia.Value);
                            }

                            for (var j = 0; j < imbricationLevel + 1; j++)
                            {
                                leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
                            }

                            argument = argument.WithLeadingTrivia(leadingTrivias);

                            if (i == argumentList.Arguments.Count - 1)
                            {
                                var trailingTrivias = new List<SyntaxTrivia>();
                                trailingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
                                if (baseLeadingTrivia != null)
                                {
                                    trailingTrivias.Add(baseLeadingTrivia.Value);
                                }

                                for (var j = 0; j < imbricationLevel; j++)
                                {
                                    trailingTrivias.Add(SyntaxTriviaHelper.GetTab());
                                }

                                argument = argument.WithTrailingTrivia(trailingTrivias);
                            }
                        }

                        newArguments.Add(argument);
                    }

                    var newArgumentList = argumentList.WithArguments(
                        SyntaxFactory.SeparatedList(newArguments)
                    );
                    var newExpression = expression.WithArgumentList(newArgumentList);
                    if (expression.FullSpan.Length != newExpression.FullSpan.Length)
                    {
                        documentEditor.ReplaceNode(expression, newExpression);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
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

    public static async Task<RefactorOperationResult> StartFieldRenamerAsync(
        this Document document,
        Solution solution)
    {
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
                }
            }
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartEventFieldRenamerAsync(
        this Document document,
        Solution solution)
    {
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
            }
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartPropertyRenamerAsync(
        this Document document,
        Solution solution)
    {
        if (document.Name.Contains("ViewModel"))
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                solution
            );
        }

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
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartMethodRenamerAsync(
        this Document document,
        Solution solution)
    {
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
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartLocalVariableRenamerAsync(
        this Document document,
        Solution solution)
    {
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
            }
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartParameterRenamerAsync(
        this Document document,
        Solution solution)
    {
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
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartUnusedMethodParameterRenamerAsync(
        this Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.FindAllConstructorAndMethodDeclarations();
        foreach (var declaration in declarations)
        {
            var parameters = declaration.DescendantNodes().OfType<ParameterSyntax>().ToList();
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
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> StartAsyncMethodRenamerAsync(
        this Document document,
        Solution solution)
    {
        if (document.SkipProgramEntryPoint())
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                solution
            );
        }

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

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }

    public static async Task<RefactorOperationResult> FormatAsync(
        this Document document,
        Solution solution)
    {
        document = await Formatter.FormatAsync(document);
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
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

    private static SyntaxNode GetDocumentEditorRoot(
        this DocumentEditor documentEditor)
    {
        var root = documentEditor.OriginalRoot.ChildNodes().FirstOrDefault(
            obj => obj.IsKind(SyntaxKind.NamespaceDeclaration) || obj.IsKind(SyntaxKind.FileScopedNamespaceDeclaration)
        );
        root ??= documentEditor.OriginalRoot;
        return root;
    }

    private static bool SkipProgramEntryPoint(
        this Document document)
    {
        return document.Name == "Program.cs";
    }

    private static int GetUsingDirectiveRank(
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

    private static async Task<IList<TypeDeclarationSyntax>> GetAllTypeDeclarations(
        Document document)
    {
        var typeDeclarations = new List<TypeDeclarationSyntax>();

        foreach (var project in document.Project.Solution.Projects)
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
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
        };
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

        var baseLeadingTrivia = typeDeclarationRoot.FindFirstLeadingTrivia();
        var orderedMemberDeclarations = new List<MemberDeclarationSyntax>();
        foreach (var declarationToExtract in declarationsToExtract)
        {
            orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarationsToAdd, declarationToExtract, baseLeadingTrivia));
        }

        return typeDeclarationRoot
            .WithMembers(new SyntaxList<MemberDeclarationSyntax>(orderedMemberDeclarations));
    }

    private static List<MemberDeclarationSyntax> GetMemberDeclarations(
        List<MemberDeclarationSyntax> memberDeclarations,
        SyntaxKind syntaxKind,
        SyntaxTrivia? baseLeadingTrivia)
    {
        var sortedemberDeclarations = memberDeclarations
            .Where(obj => obj.IsKind(syntaxKind))
            .OrderBy(obj => GetMemberDeclarationModifierRank(obj, syntaxKind))
            .ThenBy(obj => obj.GetName())
            .Select((obj, i) =>
            {
                var leadingTrivias = new List<SyntaxTrivia>();

                ////var existingLeadingTriviasToInclude = obj.GetLeadingTrivia().Where(
                ////    obj => obj.IsKind(SyntaxKind.RegionDirectiveTrivia) || obj.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                ////).ToList();
                ////if (existingLeadingTriviasToInclude.Any())
                ////{
                ////    existingLeadingTriviasToInclude.Insert(0, SyntaxTriviaHelper.GetEndOfLine());
                ////    existingLeadingTriviasToInclude.Insert(1, baseLeadingTrivia);
                ////    leadingTrivias.AddRange(existingLeadingTriviasToInclude);
                ////}

                if (i == 0 || (syntaxKind != SyntaxKind.FieldDeclaration && syntaxKind != SyntaxKind.EventFieldDeclaration))
                {
                    leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
                }

                if (baseLeadingTrivia != null)
                {
                    leadingTrivias.Add(baseLeadingTrivia.Value);
                }

                leadingTrivias.Add(SyntaxTriviaHelper.GetTab());

                return obj
                    .FormatByDefault()
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(leadingTrivias)
                    )
                    .WithTrailingTrivia(
                        SyntaxTriviaHelper.GetEndOfLine()
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
            if (modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return 1;
            }
            else if (modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return 2;
            }
            else if (modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        if (syntaxKind == SyntaxKind.ConstructorDeclaration)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)memberDeclaration;
            return constructorDeclaration.ParameterList.Parameters.Count;
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
