using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PizthieR.Controller;
using System.Text;

namespace PizthieR;

public partial class Control : UserControl
{
    MqttController _MqttController;
    public Control(MqttController _MqttController)
    {
        this._MqttController = _MqttController;
        InitializeComponent();
        StartFour.Click += StartFour_Click;
        StopFour.Click += StopFour_Click;
        PublierTemperature.Click += PublierTemperature_Click;
        PublierTemperatureMax.Click += PublierTemperatureMax_Click;
    }
    public Control()
    {
        

    }
    private void StartFour_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Start();    
            
    }
    private void PublierTemperature_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
        if (ConsigneTemperature.Text != null)
        {
            _MqttController.PublishConsigneTemperature(ConsigneTemperature.Text);
        }
    }
    private void PublierTemperatureMax_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TempératureMax.Text != null)
        {
            _MqttController.PublishTemperatureMax(TempératureMax.Text);
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
}