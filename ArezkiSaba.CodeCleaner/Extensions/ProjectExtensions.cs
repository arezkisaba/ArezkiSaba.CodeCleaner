using Microsoft.Build.Evaluation;
using MSBuildProject = Microsoft.Build.Evaluation.Project;
using RoslynProject = Microsoft.CodeAnalysis.Project;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ProjectExtensions
{
    public static async Task<bool> IsNonNugetProjectAsync(
        this RoslynProject project)
    {
        var msbuildProject = project.ToMSBuildProject();
        if(msbuildProject == null)
        {
            return true;
        }

        bool.TryParse(msbuildProject.GetPropertyValue("IsPackable"), out var isPackable);
        return !isPackable;
    }

    #region Private use

    private static MSBuildProject ToMSBuildProject(
        this RoslynProject roslynProject)
    {
        if (roslynProject == null)
        {
            throw new ArgumentNullException(nameof(roslynProject));
        }

        var projectFilePath = roslynProject.FilePath;
        if (string.IsNullOrEmpty(projectFilePath))
        {
            return null;
        }

        var projectCollection = new ProjectCollection();
        var msbuildProject = projectCollection.LoadProject(projectFilePath);
        return msbuildProject;
    }

    #endregion
}
