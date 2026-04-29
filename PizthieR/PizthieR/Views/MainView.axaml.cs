using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MQTTnet;
using PizthieR.Controller;

namespace PizthieR.Views
{
    public partial class MainView : UserControl
    {
        // Pages
        private readonly List<UserControl> _pages = new();
        private readonly Connection _Connection;
        private readonly Control _Control;
        private readonly Programmation _Programmation;

        // MQTT
        private readonly MqttController _MqttController;

        public MainView()
        {
            InitializeComponent();

            // Init contrôleur MQTT + vues
            _MqttController = new MqttController();
            _Connection = new Connection(_MqttController, this);
            _Control = new Control(_MqttController);
            _Programmation = new Programmation(_MqttController);

            _pages.Add(_Connection);
            _pages.Add(_Control);
            _pages.Add(_Programmation);

            // Page par défaut
            Frame.Content = _pages[0];

            // Onglets verrouillés jusqu'à la connexion
            BControl.IsEnabled = false;
            BProgrammation.IsEnabled = false;
            SetActiveTab(0);

            // Surveillance santé MQTT
            _MqttController.pingHealthyChanged += pingHealthyChanged;
        }

        private void pingHealthyChanged(object sender, bool newValue)
        {
            // Si perte de santé/ping → déconnexion "sécurisée" + retour page Connection
            if (!newValue)
            {
                _Connection.DeConnectionMqtt();
                Frame.Content = _pages[0];
                SetActiveTab(0);
            }
        }

        /// <summary>
        /// Appelée par 'Connection' quand l’état change.
        /// Gère visibilité des items + abonnements.
        /// </summary>
        public async void IsConnected(bool value)
        {
            if (value)
            {
                BControl.IsEnabled = true;
                BProgrammation.IsEnabled = true;

                _Control.Abonnement();
                await _Programmation.SubscribeAllAsync();
            }
            else
            {
                BControl.IsEnabled = false;
                BProgrammation.IsEnabled = false;

                _Control.DesAbonnement();
                await _Programmation.UnsubscribeAllAsync();

                // Retour page Connection si on se déconnecte
                Frame.Content = _pages[0];
                SetActiveTab(0);
            }
        }

        private void ViewConnection_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[0];
            SetActiveTab(0);
        }

        private void ViewControl_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[1];
            SetActiveTab(1);
        }

        private void ViewProgrammation_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[2];
            SetActiveTab(2);
        }

        private void SetActiveTab(int index)
        {
            var active   = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#CC0000"));
            var inactive = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#8E8E93"));
            Bconnection.Foreground    = index == 0 ? active : inactive;
            BControl.Foreground       = index == 1 ? active : inactive;
            BProgrammation.Foreground = index == 2 ? active : inactive;
        }
    }
}
