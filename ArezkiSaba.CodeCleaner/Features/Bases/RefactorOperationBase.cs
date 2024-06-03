using Microsoft.CodeAnalysis;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public abstract class RefactorOperationBase
{
    public abstract string Name { get; }

    public abstract Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution);
}