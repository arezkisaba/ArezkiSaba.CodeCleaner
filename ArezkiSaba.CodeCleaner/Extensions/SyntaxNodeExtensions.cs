using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxNodeExtensions
{
    public static bool IsEqualTo(
        this SyntaxNode root,
        SyntaxNode compareTo)
    {
        return root.FullSpan.Length == compareTo.FullSpan.Length;
    }

    public static IEnumerable<T> ChildNodes<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return Enumerable.Empty<T>();
        }

        IEnumerable<SyntaxNode> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantNodes();
        }
        else
        {
            nodes = root.ChildNodes();
        }

        return nodes.OfType<T>();
    }

    public static T FirstChildNode<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxNode> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantNodes();
        }
        else
        {
            nodes = root.ChildNodes();
        }

        return nodes.OfType<T>().FirstOrDefault();
    }

    public static T LastChildNode<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxNode> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantNodes();
        }
        else
        {
            nodes = root.ChildNodes();
        }

        return nodes.OfType<T>().LastOrDefault();
    }

    public static IEnumerable<T> ChildTokens<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return Enumerable.Empty<T>();
        }

        IEnumerable<SyntaxToken> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantTokens();
        }
        else
        {
            nodes = root.ChildTokens();
        }

        return nodes.OfType<T>();
    }

    public static T FirstChildToken<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxToken> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantTokens();
        }
        else
        {
            nodes = root.ChildTokens();
        }

        return nodes.OfType<T>().FirstOrDefault();
    }

    public static T LastChildToken<T>(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxToken> nodes = null;
        if (recursive)
        {
            nodes = root.DescendantTokens();
        }
        else
        {
            nodes = root.ChildTokens();
        }

        return nodes.OfType<T>().LastOrDefault();
    }

    public static SyntaxNodeOrToken ItemAfter(
        this SyntaxNode root,
        SyntaxNodeOrToken syntaxItem)
    {
        if (root == null)
        {
            return null;
        }

        var items = root.ChildNodesAndTokens().ToList();
        for (var i = 0; i < items.Count(); i++)
        {
            var item = items[i];
            if (item.IsEquivalentTo(syntaxItem))
            {
                if (i + 1 < items.Count() - 1)
                {
                    return items[i + 1];
                }
            }
        }

        return null;
    }

    public static SyntaxNodeOrToken ItemBefore(
        this SyntaxNode root,
        SyntaxNodeOrToken syntaxItem)
    {
        if (root == null)
        {
            return null;
        }

        var items = root.ChildNodesAndTokens().ToList();
        for (var i = 0; i < items.Count(); i++)
        {
            var item = items[i];
            if (item.IsEquivalentTo(syntaxItem))
            {
                return items[i - 1];
            }
        }

        return null;
    }

    public static bool IsInvocationOrCreationExpression(
        this SyntaxNode root)
    {
        return root.IsKind(SyntaxKind.InvocationExpression)
            || root.IsKind(SyntaxKind.ObjectCreationExpression);
    }

    public static bool IsImbricationExpression(
        this SyntaxNode root)
    {
        return root.IsInvocationOrCreationExpression() ||
            root is ArrayCreationExpressionSyntax ||
            root is ImplicitArrayCreationExpressionSyntax ||
            root is StackAllocArrayCreationExpressionSyntax ||
            root is ImplicitStackAllocArrayCreationExpressionSyntax ||
            root is CollectionExpressionSyntax ||
            root is ObjectCreationExpressionSyntax;
    }

    public static bool HasBaseOrThisInitializer(
        this SyntaxNode root)
    {
        return root.DescendantNodes().OfType<ConstructorInitializerSyntax>().Any();
    }

    public static bool HasDeclaration(
        this SyntaxNode root)
    {
        return root.DescendantNodes().OfType<BlockSyntax>().Any();
    }

    public static SyntaxTrivia? FindFirstLeadingTrivia(
        this SyntaxNode root)
    {
        var baseLeadingTrivia = root.GetLeadingTrivia().FirstOrDefault(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        if (baseLeadingTrivia.IsKind(SyntaxKind.None))
        {
            return null;
        }

        return baseLeadingTrivia;
    }

    public static bool BeginsWithAutoGeneratedComment(
        this SyntaxNode root)
    {
        if (root.HasLeadingTrivia)
        {
            var commentTrivia = root.GetLeadingTrivia().Where(
                t => t.IsKind(SyntaxKind.MultiLineCommentTrivia) || t.IsKind(SyntaxKind.SingleLineCommentTrivia)
            );

            foreach (var trivia in commentTrivia)
            {
                var text = trivia.ToString().AsSpan();
                if (text.Contains("autogenerated", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("auto-generated", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static void DisplayCodeAsTree(
        this SyntaxNode syntaxNode,
        int indent = 0)
    {
        var children = syntaxNode.ChildNodesAndTokens().ToList();

        if (children.Count() == 0)
        {
            Console.WriteLine();
        }
        else
        {
            foreach (var child in children)
            {
                if (child.IsToken)
                {
                    DisplayTokenAsTree(indent, child.AsToken());

                    foreach (var trivia in child.AsToken().LeadingTrivia)
                    {
                        DisplayTriviaAsTree(indent + 1, trivia, ConsoleColor.Red);
                    }

                    foreach (var trivia in child.AsToken().TrailingTrivia)
                    {
                        DisplayTriviaAsTree(indent + 1, trivia, ConsoleColor.DarkRed);
                    }
                }

                if (child.IsNode)
                {
                    DisplayNodeAsTree(indent, child.AsNode());
                    child.AsNode().DisplayCodeAsTree(indent + 1);
                }
            }
        }
    }

    private static void DisplayNodeAsTree(
        int indent,
        SyntaxNode node)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("{0}{1} {2}", new string('\u00A0', 4 * (indent)), node.Kind(), node.Span);
        Console.ResetColor();
    }

    private static void DisplayTokenAsTree(
        int indent,
        SyntaxToken token)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("{0}{1} {2}", new string('\u00A0', 4 * (indent)), token.Kind(), token.Span);
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" {0}", token);
        Console.ResetColor();
    }

    private static void DisplayTriviaAsTree(
        int indent,
        SyntaxTrivia trivia,
        ConsoleColor consoleColor)
    {
        Console.ForegroundColor = consoleColor;
        Console.WriteLine("{0}{1} {2}", new string('\u00A0', 4 * (indent)), trivia.Kind(), trivia.Span);
        Console.ResetColor();
    }

    public static void DisplayCodeAsRaw(
        this SyntaxNode node)
    {
        var children = node.ChildNodesAndTokens().ToList();

        if (children.Count() == 0)
        {
            Console.WriteLine();
        }
        else
        {
            foreach (var child in children)
            {

                if (child.IsToken)
                {
                    foreach (var trivia in child.AsToken().LeadingTrivia)
                    {
                        DisplayTriviaAsRaw(trivia);
                    }

                    DisplayTokenAsRaw(child.AsToken());

                    foreach (var trivia in child.AsToken().TrailingTrivia)
                    {
                        DisplayTriviaAsRaw(trivia);
                    }
                }

                if (child.IsNode)
                {
                    child.AsNode().DisplayCodeAsRaw();
                }
            }
        }
    }

    private static void DisplayTokenAsRaw(
        SyntaxToken token)
    {
        Console.ResetColor();
        Console.Write("{0}", token);
    }

    private static void DisplayTriviaAsRaw(
        SyntaxTrivia trivia)
    {
        Console.ResetColor();
        Console.Write("{0}", trivia);
    }
}
