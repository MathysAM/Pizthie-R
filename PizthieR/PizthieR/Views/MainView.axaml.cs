using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
            SetActiveNavButton(Bconnection);

            // État initial : boutons hors connexion masqués
            BControl.IsVisible = false;
            BProgrammation.IsVisible = false;

            // Surveillance santé MQTT
            _MqttController.pingHealthyChanged += pingHealthyChanged;
        }

        private void pingHealthyChanged(object sender, bool newValue)
        {
            if (!newValue)
            {
                _Connection.DeConnectionMqtt();
                Frame.Content = _pages[0];
                SetActiveNavButton(Bconnection);
            }
        }

        /// <summary>
        /// Appelée par 'Connection' quand l'état change.
        /// </summary>
        public async void IsConnected(bool value)
        {
            if (value)
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

                Frame.Content = _pages[0];
                SetActiveNavButton(Bconnection);
            }
        }

        // Handlers de navigation
        private void ViewConnection_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[0];
            SetActiveNavButton(Bconnection);
        }

        private void ViewControl_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[1];
            SetActiveNavButton(BControl);
        }

        private void ViewProgrammation_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[2];
            SetActiveNavButton(BProgrammation);
        }

        /// <summary>
        /// Retire la classe "active" de tous les boutons de nav
        /// et l'applique uniquement sur le bouton sélectionné.
        /// </summary>
        private void SetActiveNavButton(Button active)
        {
            foreach (var btn in new[] { Bconnection, BControl, BProgrammation })
            {
                btn.Classes.Remove("active");
            }
            active.Classes.Add("active");
        }
    }
}