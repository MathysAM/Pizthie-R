using Avalonia.Controls;
using PizthieR.Services;

namespace PizthieR.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Example usage of the MQTT service on startup
        var mqttService = new MqttService();
        _ = mqttService.ConnectAsync(
            "wss://4d1f194df18748a393eeabb274d5e439.s1.eu.hivemq.cloud:8884/mqtt",
            "MathysChef",
            "Flth2112");
    }
}
