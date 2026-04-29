using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PizthieR;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Starting Avalonia Browser...");

            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync("out");

            Console.WriteLine("Avalonia Browser started OK.");
        }
        catch (Exception ex)
        {
            // IMPORTANT: ToString() contient InnerException + stacktrace
            Console.Error.WriteLine("=== AVALONIA START FAILED ===");
            Console.Error.WriteLine(ex.ToString());
            Console.Error.WriteLine("=== END ===");
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}