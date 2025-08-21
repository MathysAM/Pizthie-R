using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PizthieR.Controller;

namespace PizthieR;

public partial class Connection : UserControl
{
    MqttController mqttController;
    public Connection(MqttController mqttController)
    {
        this.mqttController = mqttController;
        InitializeComponent();
    }

    private void Connection_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ConnectionMqtt();
    }
    private void DeConnection_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DeConnectionMqtt();
    }

    private async void ConnectionMqtt()
    {
        if (ID.Text != null && MDP.Text != null)
        {
            Status.Text = await mqttController.ConnectAsync(ID.Text, MDP.Text);

        }
        else
        {
            Status.Text = "Erreur de connexion";
        }
    }
    private async void DeConnectionMqtt()
    {
       
        await mqttController.DisposeAsync();
        Status.Text = "Deconnecter";


    }
}