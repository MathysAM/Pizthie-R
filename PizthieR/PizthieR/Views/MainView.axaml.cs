using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using MQTTnet; // seul using côté MQTT

namespace PizthieR.Views
{

    public partial class MainView : UserControl, IAsyncDisposable
    {
        private readonly Button _btnConnect;
        private readonly Button _btnSub;
        private readonly Button _btnPub;
        private readonly TextBox _topicBox;
        private readonly TextBox _payloadBox;
        private readonly TextBlock _status;

        private MQTTnet.Client.IMqttClient _client;
        private MQTTnet.Client.MqttClientOptions _opts;   // <<< PAS IMqttClientOptions

        private const string WssUrl = "wss://4d1f194df18748a393eeabb274d5e439.s1.eu.hivemq.cloud:8884/mqtt";
        private const string User = "MathysChef";
        private const string Pass = "Flth2112";

        public MainView()
        {
            InitializeComponent();

            _status = new TextBlock { Text = "MQTT: déconnecté" };
            _btnConnect = new Button { Content = "Se connecter (WSS)" };
            _btnSub = new Button { Content = "Subscribe", IsEnabled = false };
            _btnPub = new Button { Content = "Publish", IsEnabled = false };
            _topicBox = new TextBox { Text = "/Sup/Stop" };
            _payloadBox = new TextBox { Text = "hello wasm" };

            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 8,
                Children =
                {
                    _btnConnect,
                    new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8, Children = { _topicBox, _btnSub } },
                    new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8, Children = { _payloadBox, _btnPub } },
                    _status
                }
            };

            _btnConnect.Click += async (_, __) => await ConnectAsync();

            _btnPub.Click += async (_, __) => await PublishAsync(_topicBox.Text ?? "/Sup/Stop", _payloadBox.Text ?? "");
        }

        private async Task ConnectAsync()
        {
            try
            {
                _btnConnect.IsEnabled = false;
                SetStatus("Connexion…");

                var builder = new MQTTnet.Client.MqttClientOptionsBuilder()
                    .WithWebSocketServer(WssUrl) // WASM => WebSocket obligatoire
                    .WithClientId($"wasm_{Guid.NewGuid():N}".Substring(0, 16))
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

                if (!string.IsNullOrWhiteSpace(User))
                    builder = builder.WithCredentials(User, Pass);

                _opts = builder.Build(); // <-- type MqttClientOptions

                _client = new MQTTnet.MqttFactory().CreateMqttClient();

                _client.ApplicationMessageReceivedAsync += e =>
                {
                    SetStatus($"MSG [{e.ApplicationMessage.Topic}] {e.ApplicationMessage.ConvertPayloadToString()}");
                    return Task.CompletedTask;
                };

                _client.DisconnectedAsync += async _ =>
                {
                    SetStatus("Déconnecté. Reconnexion dans 2s…");
                    await Task.Delay(2000);
                    try { await _client.ConnectAsync(_opts); } catch { }
                };

                await _client.ConnectAsync(_opts);

                _btnSub.IsEnabled = _btnPub.IsEnabled = true;
                SetStatus("? Connecté (WSS)");
            }
            catch (Exception ex)
            {
                _btnConnect.IsEnabled = true;
                SetStatus("? " + ex.Message);
            }
        }


        private async Task PublishAsync(string topic, string payload)
        {
            if (_client?.IsConnected != true) { SetStatus("Non connecté"); return; }

            var msg = new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload ?? string.Empty)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _client.PublishAsync(msg);
            SetStatus($"PUB ? {topic} : {payload}");
        }

        private void SetStatus(string s) => _status.Text = $"[{DateTime.Now:HH:mm:ss}] {s}";

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_client != null)
                {
                    var disc = new MQTTnet.Client.MqttClientDisconnectOptions();
                    await _client.DisconnectAsync(disc);
                }
            }
            catch { }
            _client?.Dispose();
        }
    }

}
