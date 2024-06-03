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
        bool recursive = false) where T : SyntaxNode
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

    public static T FirstParentNode<T>(
        this SyntaxNode root,
        bool recursive = false) where T : SyntaxNode
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxNode> nodes = null;
        if (recursive)
        {
            nodes = root.Ancestors();
        }
        else
        {
            nodes = root.Ancestors();
        }

        return nodes.OfType<T>().FirstOrDefault();
    }

    public static T FirstChildNode<T>(
        this SyntaxNode root,
        bool recursive = false) where T : SyntaxNode
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

    public static bool HasChildNode<T>(
        this SyntaxNode root,
        bool recursive = false) where T : SyntaxNode
    {
        return root.FirstChildNode<T>(recursive) != null;
    }

    public static T LastChildNode<T>(
        this SyntaxNode root,
        bool recursive = false) where T : SyntaxNode
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

    public static IEnumerable<SyntaxToken> ChildTokens(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return Enumerable.Empty<SyntaxToken>();
        }

        IEnumerable<SyntaxToken> tokens = null;
        if (recursive)
        {
            tokens = root.DescendantTokens();
        }
        else
        {
            tokens = root.ChildTokens();
        }

        return tokens;
    }

    public static SyntaxToken FirstChildToken(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxToken> tokens = null;
        if (recursive)
        {
            tokens = root.DescendantTokens();
        }
        else
        {
            tokens = root.ChildTokens();
        }

        return tokens.FirstOrDefault();
    }

    public static SyntaxToken LastChildToken(
        this SyntaxNode root,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxToken> tokens = null;
        if (recursive)
        {
            tokens = root.DescendantTokens();
        }
        else
        {
            tokens = root.ChildTokens();
        }

        return tokens.LastOrDefault();
    }

    public static SyntaxNodeOrToken ItemAfter(
        this SyntaxNode root,
        SyntaxNodeOrToken syntaxItem,
        bool recursive = false)
    {
        if (root == null)
        {
            return null;
        }

        IEnumerable<SyntaxNodeOrToken> items = null;
        if (recursive)
        {
            items = root.DescendantNodesAndTokens();
        }
        else
        {
            items = root.ChildNodesAndTokens();
        }

        for (var i = 0; i < items.Count(); i++)
        {
            var item = items.ElementAt(i);
            if (item.IsEquivalentTo(syntaxItem))
            {
                if (i + 1 < items.Count() - 1)
                {
                    return items.ElementAt(i + 1);
                }
            }
        }

        return null;
    }

    public static SyntaxToken SyntaxTokenAfter(
        this SyntaxNode root,
        SyntaxNodeOrToken syntaxItem,
        bool recursive = false)
    {
        if (root == null)
        {
            return default;
        }

        IEnumerable<SyntaxNodeOrToken> items = null;
        if (recursive)
        {
            items = root.DescendantNodesAndTokens();
        }
        else
        {
            items = root.ChildNodesAndTokens();
        }

        for (var i = 0; i < items.Count(); i++)
        {
            var item = items.ElementAt(i);
            if (item.IsEquivalentTo(syntaxItem))
            {
                if (i + 1 < items.Count())
                {
                    return items.ElementAt(i + 1).AsToken();
                }
            }
        }

        return default;
    }

    public static SyntaxNodeOrToken ItemBefore(
        this SyntaxNode root,
        SyntaxNodeOrToken syntaxItem,
        bool recursive = false)
    {
        if (root == null)
        {
            return null;
        }

        IEnumerable<SyntaxNodeOrToken> items = null;
        if (recursive)
        {
            items = root.DescendantNodesAndTokens();
        }
        else
        {
            items = root.ChildNodesAndTokens();
        }

        for (var i = 0; i < items.Count(); i++)
        {
            var item = items.ElementAt(i);
            if (item.IsStrictlyEqualTo(syntaxItem))
            {
                return items.ElementAt(i - 1);
            }
        }

        return null;
    }

    public static T WithEndOfLineTrivia<T>(
        this SyntaxNode node) where T : class
    {
        return node.WithEndOfLineTrivia() as T;
    }

    public static SyntaxNode WithEndOfLineTrivia(
        this SyntaxNode node)
    {
        return (SyntaxNode)((SyntaxNodeOrToken)node).WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static T WithIndentationTrivia<T>(
        this SyntaxNode node,
        SyntaxNode parent,
        bool keepOtherTrivias = false,
        int indentCount = 1) where T : SyntaxNode
    {
        return node.WithIndentationTrivia(parent, indentCount, keepOtherTrivias) as T;
    }

    public static SyntaxNode WithIndentationTrivia(
        this SyntaxNode node,
        SyntaxNode relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        return node.WithIndentationTrivia((SyntaxNodeOrToken)relativeTo, indentCount, keepOtherTrivias, mustAddLineBreakBefore);
    }

    public static T WithIndentationTrivia<T>(
        this SyntaxNode node,
        SyntaxToken parent,
        bool keepOtherTrivias = false,
        int indentCount = 1) where T : SyntaxNode
    {
        return node.WithIndentationTrivia(parent, indentCount, keepOtherTrivias) as T;
    }

    public static SyntaxNode WithIndentationTrivia(
        this SyntaxNode node,
        SyntaxToken relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        return node.WithIndentationTrivia((SyntaxNodeOrToken)relativeTo, indentCount, keepOtherTrivias, mustAddLineBreakBefore);
    }

    public static int GetIndentationLevel(
        this SyntaxNode item,
        int indentCount = 0)
    {
        return ((SyntaxNodeOrToken)item).GetIndentationLevel(indentCount);
    }

    public static int GetIndentationLength(
        this SyntaxNode item,
        int indentCount = 0)
    {
        return ((SyntaxNodeOrToken)item).GetIndentationLength(indentCount);
    }

    public static int GetIndentCountByImbrication(
        this SyntaxNode node)
    {
        var imbricationLevel = 0;
        var countToSubstract = node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) ? 1 : 0;
        var ancestors = node.Ancestors().ToList();
        foreach (var ancestor in ancestors)
        {
            if (ancestor.IsImbricationExpression())
            {
                imbricationLevel++;
            }
            else if (ancestor is StatementSyntax)
            {
                break;
            }
        }

        return imbricationLevel - countToSubstract;
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
        var result = root.IsInvocationOrCreationExpression() ||
            root is AnonymousObjectCreationExpressionSyntax ||
            root is ImplicitObjectCreationExpressionSyntax ||
            root is ArrayCreationExpressionSyntax ||
            root is ImplicitArrayCreationExpressionSyntax ||
            root is StackAllocArrayCreationExpressionSyntax ||
            root is ImplicitStackAllocArrayCreationExpressionSyntax ||
            root is CollectionExpressionSyntax ||
            root is ObjectCreationExpressionSyntax;
        ////if (root.IsInvocationOrCreationExpression())
        ////{
        ////    result = result && !root.HasChildNode<MemberAccessExpressionSyntax>();
        ////}
        return result;
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

    public static int GetLength(
        this SyntaxNode node)
    {
        if (node == null)
        {
            return 0;
        }

        var text = node.GetText().ToString().Replace(Environment.NewLine, string.Empty);
        return text.Length;
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

    #region Private use

    private static SyntaxNode WithIndentationTrivia(
        this SyntaxNode node,
        SyntaxNodeOrToken relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivia = relativeTo.GetIndentation(indentCount);

        if (keepOtherTrivias)
        {
            leadingTrivias.AddRange(node.GetLeadingTrivia().Where(obj => !obj.IsKind(SyntaxKind.WhitespaceTrivia)));
        }

        if (mustAddLineBreakBefore)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
        }

        leadingTrivias.AddRange(indentationTrivia);

        var newTriviaList = new List<SyntaxTrivia>();
        foreach (var trivia in leadingTrivias)
        {
            if (keepOtherTrivias && (trivia.IsRegionTrivia() || trivia.IsCommentTrivia()))
            {
                newTriviaList.AddRange(indentationTrivia);
            }

            newTriviaList.Add(trivia);
        }

        return node.WithLeadingTrivia(newTriviaList);
    }

    private static void DisplayNodeAsTree(
        int indent,
        SyntaxNode node)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("{0}{1} {2}", new string('\u00A0', 4 * indent), node.Kind(), node.Span);
        Console.ResetColor();
    }

    private static void DisplayTokenAsTree(
        int indent,
        SyntaxToken token)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("{0}{1} {2}", new string('\u00A0', 4 * indent), token.Kind(), token.Span);
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
        Console.WriteLine("{0}{1} {2}", new string('\u00A0', 4 * indent), trivia.Kind(), trivia.Span);
        Console.ResetColor();
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

    #endregion
}
