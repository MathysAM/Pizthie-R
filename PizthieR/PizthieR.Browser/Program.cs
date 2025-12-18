using System;
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
            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync("out");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("=== Avalonia startup failed ===");
            Console.Error.WriteLine(ex.ToString()); // IMPORTANT: imprime inner exceptions
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
