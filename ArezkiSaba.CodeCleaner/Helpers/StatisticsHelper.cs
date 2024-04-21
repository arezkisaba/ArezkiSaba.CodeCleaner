using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class StatisticsHelper
{
    public static void Print()
    {
        var lineCount = 0;
        var folder = @"C:\git";
        var codeExtensions = new[] { ".cs", ".xaml", ".html", ".js", ".css", ".ps1" };
        var allFiles = Directory.GetFiles(folder, searchPattern: "*.*", searchOption: SearchOption.AllDirectories)
            .Where(file => codeExtensions.Any(file.ToLower().EndsWith))
            .ToList();
        var i = 0;
        foreach (var file in allFiles)
        {
            lineCount += File.ReadAllLines(file).Length;
            Console.WriteLine($"Total code lines : {lineCount.ToString("N0").Replace(",", " ")}   -   {i / (double)allFiles.Count * 100:F2} %");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            i++;
        }
    }
}