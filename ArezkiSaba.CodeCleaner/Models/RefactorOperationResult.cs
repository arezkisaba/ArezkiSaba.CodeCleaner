using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Models;

public sealed record RefactorOperationResult(
    Document Document,
    Project Project,
    Solution Solution
);
