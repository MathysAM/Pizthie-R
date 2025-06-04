using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PizthieR;
using PizthieR.Browser.Services; // ⬅️ pour accéder à MqttControllerWeb

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
        .WithInterFont()
        .AfterSetup(_ =>
        {
            App.MqttController = new MqttControllerWeb(); // ⬅️ lien avec le core
        })
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
