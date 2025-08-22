using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PizthieR.Controller;
using PizthieR.Views;

namespace PizthieR;

public partial class Connection : UserControl
{
  
   
    public Connection()
    {
        InitializeComponent();
        

    }
    bool IsConnected;
    MqttController mqttController;
    MainView mainView;
    // Requis par le previewer Avalonia
   
    public Connection(MqttController mqttController, MainView mainView)
    {
        InitializeComponent();
        this.mqttController = mqttController;
        this.mainView = mainView;
        // branchements d'�v�nements possibles ici si besoin
        BtnConnect.Click += Connection_Click;
        BtnDisconnect.Click += DeConnection_Click;
        
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
            IsConnected  = await mqttController.ConnectAsync(ID.Text, MDP.Text);
         
            if (IsConnected)
            {
                
                mainView.IsConnected(true);
                Status.Text = "Connecter";
            }
            else
            {
                mainView.IsConnected(false);

                Status.Text = "Erreur de connexion";
            }
        }
        else
        {
            Status.Text = "Erreur de connexion";
        }
    }
    private async void DeConnectionMqtt()
    {
       
        await mqttController.DisconnectAsync();
        Status.Text = "Deconnecter";
        mainView.IsConnected(false);

    }
}