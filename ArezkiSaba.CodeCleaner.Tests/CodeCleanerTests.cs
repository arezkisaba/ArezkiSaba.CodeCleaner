using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System.Reflection;
using System.Text;

namespace ArezkiSaba.CodeCleaner.Tests;

[TestFixture]
public sealed class CodeCleanerTests
{
    private string _resourcesFolderPath;
    private string _projectName;
    private string _fileName;

    [SetUp]
    public void Setup()
    {
        _resourcesFolderPath = $"{Assembly.GetExecutingAssembly().GetName().Name}/Resources";
        _projectName = "TestProject";
        _fileName = "TestClass.cs";
    }

    [Test]
    public async Task CleanAsync_ContentAfterRefactoring_EqualsTo_ExpectedContent()
    {
        var sourceContent = await ReadEmbeddedResourceAsync($"{_resourcesFolderPath}/Source.txt");

        var filesToAdd = new List<(string fileName, string fileContent)>();
        filesToAdd.Add((_fileName, sourceContent));

        var workspace = CreateInMemoryWorkspace(
            _projectName,
            filesToAdd
        );
        workspace = await workspace.CleanAndRefactorAsync();

        var contentAfterRefactoring = await ReadDocumentContentAsync(workspace, _projectName, _fileName);
        var expectedContent = await ReadEmbeddedResourceAsync($"{_resourcesFolderPath}/Expected.txt");

        Assert.That(contentAfterRefactoring, Is.EqualTo(expectedContent));
    }

    ////[Test]
    ////public async Task OpenAI_Test()
    ////{
    ////    var api = new OpenAIAPI(APIAuthentication.LoadFromEnv());
    ////    var chat = api.Chat.CreateConversation();
    ////    chat.Model = Model.ChatGPTTurbo;
    ////    chat.RequestParameters.Temperature = 0;

    ////    /// give instruction as System
    ////    chat.AppendSystemMessage("You are a teacher who helps children understand if things are animals or not.  If the user tells you an animal, you say \"yes\".  If the user tells you something that is not an animal, you say \"no\".  You only ever respond with \"yes\" or \"no\".  You do not say anything else.");

    ////    // give a few examples as user and assistant
    ////    chat.AppendUserInput("Is this an animal? Cat");
    ////    chat.AppendExampleChatbotOutput("Yes");
    ////    chat.AppendUserInput("Is this an animal? House");
    ////    chat.AppendExampleChatbotOutput("No");

    ////    // now let's ask it a question
    ////    chat.AppendUserInput("Is this an animal? Dog");
    ////    // and get the response
    ////    var response = await chat.GetResponseFromChatbotAsync();
    ////    Console.WriteLine(response); // "Yes"

    ////    // and continue the conversation by asking another
    ////    chat.AppendUserInput("Is this an animal? Chair");
    ////    // and get another response
    ////    response = await chat.GetResponseFromChatbotAsync();
    ////    Console.WriteLine(response); // "No"

    ////    // the entire chat history is available in chat.Messages
    ////    foreach (var msg in chat.Messages)
    ////    {
    ////        Console.WriteLine($"{msg.Role}: {msg.Content}");
    ////    }
    ////}

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

    public static async  Task<string> ReadDocumentContentAsync(
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

        return (await document.GetTextAsync()).ToString();
    }

    private static async Task<string> ReadEmbeddedResourceAsync(
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
                return await reader.ReadToEndAsync();
            }
        }
    }

    #endregion
}