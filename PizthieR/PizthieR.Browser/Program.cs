using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PizthieR;

internal sealed partial class Program
{
    private static Task Main(string[] args) =>
        BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
