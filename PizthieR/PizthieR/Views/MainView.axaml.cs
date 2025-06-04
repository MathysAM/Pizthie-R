using Avalonia.Controls;
using PizthieR.Controller;

namespace PizthieR.Views;

public partial class MainView : UserControl
{
   MqttController _mqttController;
    public MainView()
    {
        InitializeComponent();
        _mqttController = new MqttController();
    }
    private void OnConnectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string name = "MathysChef";
        string mdp = "Flth2112";
        _mqttController.Connection(name, mdp);
    }
}