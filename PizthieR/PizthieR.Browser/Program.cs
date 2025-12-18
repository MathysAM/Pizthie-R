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
        // 1) Remonte les exceptions non gérées (souvent masquées en WASM)
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.Error.WriteLine("=== UnhandledException ===");
            Console.Error.WriteLine(e.ExceptionObject?.ToString() ?? "<null>");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.Error.WriteLine("=== UnobservedTaskException ===");
            Console.Error.WriteLine(e.Exception?.ToString() ?? "<null>");
            e.SetObserved();
        };

        // 2) Pour attraper le crash au tout début (ex: static ctor SkiaSharp)
        try
        {
            await BuildAvaloniaApp()
                .WithInterFont()
                // garde ton "out" si c'est ton folder de debug, sinon mets "" (vide)
                .StartBrowserAppAsync("out");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("=== Fatal exception during startup ===");
            Console.Error.WriteLine(ex.ToString());

            // Déroule les InnerException pour ne rien rater
            var inner = ex.InnerException;
            var depth = 0;
            while (inner != null && depth < 10)
            {
                Console.Error.WriteLine($"=== InnerException level {depth + 1} ===");
                Console.Error.WriteLine(inner.ToString());
                inner = inner.InnerException;
                depth++;
            }

            throw; // laisse le runtime afficher l'erreur aussi
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}