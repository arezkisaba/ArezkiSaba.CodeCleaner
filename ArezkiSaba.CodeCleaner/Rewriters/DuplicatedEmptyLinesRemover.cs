using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class DuplicatedEmptyLinesRemover : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia
    {
        get { return true; }
    }

    public override SyntaxTriviaList VisitList(
        SyntaxTriviaList triviaList)
    {
        var newTriviaList = new List<SyntaxTrivia>();
        var lastWasEndOfLine = false;

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (!lastWasEndOfLine)
                {
                    newTriviaList.Add(trivia);
                    lastWasEndOfLine = true;
                }
            }
            else
            {
                newTriviaList.Add(trivia);
                lastWasEndOfLine = false;
            }
        }

        return SyntaxFactory.TriviaList(newTriviaList);
    }
}
