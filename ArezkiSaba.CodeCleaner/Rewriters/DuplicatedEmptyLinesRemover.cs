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
        SyntaxTriviaList list)
    {
        list = base.VisitList(list);

        var lineBreaksAtBeginning = list.TakeWhile(t => t.IsKind(SyntaxKind.EndOfLineTrivia)).Count();
        if (lineBreaksAtBeginning > 1)
        {
            list = SyntaxFactory.TriviaList(list.Skip(lineBreaksAtBeginning - 1));
        }

        return list;
    }
}
