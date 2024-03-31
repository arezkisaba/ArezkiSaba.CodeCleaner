using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System.Reflection;
using System.Text;

namespace ArezkiSaba.CodeCleaner.Tests;

[TestFixture]
public sealed class CodeCleanerServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task RefactorAsync_Test()
    {
        var resourcesFolderPath = $"ArezkiSaba.CodeCleaner.Tests/Resources";
        var sourceContent = ReadEmbeddedResource($"{resourcesFolderPath}/Source.txt");

        var projectName = "TestProject";
        var fileName = "TestClass.cs";

        var filesToAdd = new List<(string fileName, string fileContent)>();
        filesToAdd.Add((fileName, sourceContent));

        var workspace = CreateInMemoryWorkspace(
            projectName,
            filesToAdd
        );
        workspace = await workspace.RefactorAsync();

        var contentAfterRefactoring = ReadDocumentContent(workspace, projectName, fileName);
        var expectedContent = ReadEmbeddedResource($"{resourcesFolderPath}/Expected.txt");

        Assert.That(contentAfterRefactoring, Is.EqualTo(expectedContent));
    }

    #region Private use

    private static Workspace CreateInMemoryWorkspace(
        string projectName,
        IEnumerable<(string fileName, string fileContent)> filesWithContent)
    {
        var workspace = new AdhocWorkspace();
        var solution = workspace.CurrentSolution;
        var projectId = ProjectId.CreateNewId();
        var versionStamp = VersionStamp.Create();
        var projectInfo = ProjectInfo.Create(projectId, versionStamp, projectName, projectName, LanguageNames.CSharp);
        solution = solution.AddProject(projectInfo);

        foreach (var (fileName, fileContent) in filesWithContent)
        {
            var documentId = DocumentId.CreateNewId(projectId);
            var documentInfo = DocumentInfo.Create(documentId, fileName, null, SourceCodeKind.Regular);
            solution = solution.AddDocument(documentInfo);
            var project = solution.GetProject(projectId);
            var document = solution.GetDocument(documentId);
            document = document.WithText(SourceText.From(fileContent, Encoding.UTF8));
            project = document.Project;
            solution = project.Solution;
        }

        if (!ReferenceEquals(solution, workspace.CurrentSolution) &&
            workspace.TryApplyChanges(solution))
        {
            return workspace;
        }

        throw new InvalidOperationException($"Failed to create in memory workspace with project '{projectName}'.");
    }

    public static string ReadDocumentContent(
        Workspace workspace,
        string projectName,
        string fileName)
    {
        var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
        {
            throw new ArgumentException($"Project '{projectName}' not found in the solution.");
        }

        var document = project.Documents.FirstOrDefault(d => d.Name == fileName);
        if (document == null)
        {
            throw new ArgumentException($"Document '{fileName}' not found in project '{projectName}'.");
        }

        var content = document.GetTextAsync().Result.ToString();
        return content;
    }

    private static string ReadEmbeddedResource(
        string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(resourcePath.Replace("/", ".")));
        if (resourceName == null)
        {
            throw new ArgumentException($"Embedded resource file '{resourcePath}' not found.");
        }

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException($"Failed to read embedded resource file '{resourceName}'.");
            }

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    #endregion
}
