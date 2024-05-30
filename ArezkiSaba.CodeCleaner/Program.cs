namespace ArezkiSaba.CodeCleaner;

public static class Program
{
    private static async Task Main(
        string[] args)
    {
        string targetLocation;
        if (args.Any())
        {
            targetLocation = args[0];
        }
        else
        {
#if DEBUG
            ////targetLocation = @"C:\git\Prevoir.SolutionCapitalObseques\Prevoir.SolutionCapitalObseques.sln";
            targetLocation = @"C:\git\Prevoir.Api.GED.ApiClient\Prevoir.Api.GED.ApiClient.sln"; 
            ////targetLocation = @"C:\git\ArezkiSaba.CodeCleaner\ArezkiSaba.CodeCleaner.SampleProject.sln";
#else
            Console.Write("Please enter target folder : ");
            targetLocation = Console.ReadLine();
#endif
        }

        var codeCleaner = new CodeCleanerService(targetLocation);
        await codeCleaner.StartAsync();
    }
}