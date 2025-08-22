using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PizthieR.Controller;
using System.Text;

namespace PizthieR;

public partial class Control : UserControl
{
    public Control()
    {
        InitializeComponent();

    }
    MqttController _MqttController;
  
    public Control(MqttController _MqttController)
    {
        InitializeComponent();
        this._MqttController = _MqttController;
       
        StartFour.Click += StartFour_Click;
        StopFour.Click += StopFour_Click;
        PublierTemperature.Click += PublierTemperature_Click;
        PublierTemperatureMax.Click += PublierTemperatureMax_Click;
    }
   
    private void StartFour_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Start();

    }
    private async void PublierTemperature_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        if (ConsigneTemperature.Text != null)
        {
            await _MqttController.PublishConsigneTemperature(ConsigneTemperature.Text);
        }
    }
    private async void PublierTemperatureMax_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TemperatureMax.Text != null)
        {
            await _MqttController.PublishTemperatureMax(TemperatureMax.Text);
        }

    }


    private void StopFour_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Stop();
    }
    private async void Start()
    {
        await _MqttController.PublishMomentaryAsync("/Sup/Start");
    }
    private async void Stop()
    {
        await _MqttController.PublishMomentaryAsync("/Sup/Stop");
    }
    public async void Abonnement()
    {
        await _MqttController.SubscribeAsync("/Sup/ConsigneTemperture", CallBackConsigneTemperature, 0);
        await _MqttController.SubscribeAsync("/Sup/TemperatureMax", CallBackTemperatureMax, 0);

        await _MqttController.SubscribeAsync("/automate/ReguleEtat", CallBackEtatRegule, 0);
        await _MqttController.SubscribeAsync("/automate/EtatFour", CallBackEtatFour, 0);
        await _MqttController.SubscribeAsync("/automate/temperature", CallBackTemperature, 0);
        await _MqttController.SubscribeAsync("/automate/ModeRL", CallBackModeFonctionnement, 0);




    }
    public async void DesAbonnement()
    {
        await _MqttController.UnsubscribeAsync("/Sup/ConsigneTemperture");
        await _MqttController.UnsubscribeAsync("/Sup/TemperatureMax");

        await _MqttController.UnsubscribeAsync("/automate/ReguleEtat");
        await _MqttController.UnsubscribeAsync("/automate/EtatFour");
        await _MqttController.UnsubscribeAsync("/automate/temperature");
        await _MqttController.UnsubscribeAsync("/automate/ModeRL");
    }
    private void CallBackConsigneTemperature(string payload, int index)
    {

        ConsigneTemperature.Text = payload;
        
    }
    private void CallBackTemperatureMax(string payload, int index)
    {

        TemperatureMax.Text = payload;

    }
    private void CallBackEtatRegule(string payload, int index)
    {

       
        if (payload == "true")
        {
            EtatRegule.Text = "Regule en marche";
        }
        else if (payload == "false")
        {
            EtatRegule.Text = "Regule arreter";
        }

    }
    private void CallBackEtatFour(string payload, int index)
    {
        if (payload == "true")
        {
            Etatfour.Text = "Four en marche";
        }
        else if(payload == "false") 
        {
            Etatfour.Text = "Four arreter";
        }

            

    }
    private void CallBackTemperature(string payload, int index)
    {

        TemperatureActuelle.Text = payload;

    }
    private void CallBackModeFonctionnement(string payload, int index)
    {


        if (payload == "true")
        {
            ModeFonctionnement.Text = "Mode Local";
        }
        else if (payload == "false")
        {
            ModeFonctionnement.Text = "Mode Distant";
        }


    }
}