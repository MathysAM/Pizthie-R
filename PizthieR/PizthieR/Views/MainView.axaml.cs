using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MQTTnet;
using PizthieR.Controller; // seul using côté MQTT

namespace PizthieR.Views
{

    public partial class MainView : UserControl
    {

        

        
        

        List<UserControl> _pages = new();
        Connection _Connection;
        Control _Control;
        Programmation _Programmation;
        MqttController _MqttController;

        public MainView()
        {
            InitializeComponent();
            _MqttController = new MqttController();
            _Connection = new Connection(_MqttController, this);
            _Control = new Control(_MqttController);
            _Programmation = new Programmation(_MqttController);

            _pages.Add(_Connection);
            _pages.Add(_Control);
            _pages.Add(_Programmation);

            Frame.Content = _pages[0]; // page par défaut

            BControl.IsVisible = false;
            BProgrammation.IsVisible = false;

            _MqttController.pingHealthyChanged += pingHealthyChanged;

        }
        private void pingHealthyChanged(object sender, bool newValue)
        {
            if (!newValue) 
            {
                _Connection.DeConnectionMqtt();
                Frame.Content = _pages[0]; // page par défaut
            }

            
        }
        public async void IsConnected(bool value)
        {
            if(value) 
            {
                BControl.IsVisible = true;
                BProgrammation.IsVisible = true;
                _Control.Abonnement();
                await _Programmation.SubscribeAllAsync();
            }
            else
            {
                BControl.IsVisible = false;
                BProgrammation.IsVisible = false;
                _Control.DesAbonnement();
                await _Programmation.UnsubscribeAllAsync();
            }

        }

        private void ViewProgrammation_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Frame.Content = _pages[2];
        }
        private void ViewConnection_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Frame.Content = _pages[0];
        }
        private void ViewControl_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Frame.Content = _pages[1];
        }
    }

}
