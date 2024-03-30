namespace ArezkiSaba.CodeCleaner;

public static class Program
{
    private static async Task Main(
        string[] args)
    {
        string sourceCodeLocation;
        if (args.Any())
        {
            sourceCodeLocation = args[0];
        }
        else
        {
#if DEBUG
            sourceCodeLocation = @"C:\git2";
#else
            Console.Write("Please enter target folder : ");
            sourceCodeLocation = Console.ReadLine();
#endif
        }

        var codeCleaner = new CodeCleanerService(sourceCodeLocation);
        await codeCleaner.StartAsync();

        Console.WriteLine("Press enter to continue");
        Console.ReadLine();
    }
}