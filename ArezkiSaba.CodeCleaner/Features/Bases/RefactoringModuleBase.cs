using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features.Bases;

public abstract class RefactoringModuleBase
{
    protected readonly Solution _solution;

    protected RefactoringModuleBase(
        Solution solution)
    {
        _solution = solution;
    }
}
