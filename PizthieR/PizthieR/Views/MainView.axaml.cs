using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using MQTTnet;
using PizthieR.Controller; // seul using côté MQTT

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

            // État initial : seules les pages nécessitant la connexion sont masquées
            BControl.IsVisible = false;
            BProgrammation.IsVisible = false;

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
                // Replie le menu si overlay pour un comportement propre
                FermerPaneSiOverlay();
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

                // Retour page Connection si on se déconnecte
                Frame.Content = _pages[0];
            }
        }

        // Handlers de navigation (réutilisent tes pages existantes)
        private void ViewConnection_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[0];
            FermerPaneSiOverlay();
        }

        private void ViewControl_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[1];
            FermerPaneSiOverlay();
        }

        private void ViewProgrammation_Click(object? sender, RoutedEventArgs e)
        {
            Frame.Content = _pages[2];
            FermerPaneSiOverlay();
        }

        // Ferme le volet et décoche le hamburger en mode Overlay
        private void FermerPaneSiOverlay()
        {
            if (AppSplitView is null) return;

            if (AppSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                AppSplitView.IsPaneOpen = false;

                if (this.FindControl<ToggleButton>("HamburgerBtn") is { } btn)
                    btn.IsChecked = false;
            }
        }
    }
}
