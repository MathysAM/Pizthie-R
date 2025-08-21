using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PizthieR.Controller
{
    public class MqttController : IAsyncDisposable
    {
        private IMqttClient _client;
        private MqttClientOptions _opts;

        private readonly ConcurrentDictionary<string, (Action<string, int> cb, int index)> _subscriptions =
            new(StringComparer.Ordinal);

        private Timer _pingTimer;
        private volatile bool _shouldReconnect;
        private volatile bool _pingHealthy;
        private readonly TimeSpan _pingPeriod = TimeSpan.FromSeconds(10);

        private const string WssUrl =
            "wss://4d1f194df18748a393eeabb274d5e439.s1.eu.hivemq.cloud:8884/mqtt"; // WebSocket obligatoire en WASM

        public bool IsConnected => _client?.IsConnected == true;
        public bool IsPingHealthy => _pingHealthy;

        // Événements (facultatifs) pour remonter à l’UI
        public event Action Connected;
        public event Action Disconnected;
        public event Action<string> Error;
        public event Action<string, string> Message; // (topic, payload)



        public MqttController() 
        {
        }
        public async Task<string> ConnectAsync(string user, string pass)
        {
            try
            {
                if (_client?.IsConnected == true)
                    return "Déjà connecté";

                var builder = new MqttClientOptionsBuilder()
                    .WithClientId($"wasm_{Guid.NewGuid():N}".Substring(0, 16))
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                    // nouvelle API (évite l’avertissement d’obsolescence)
                    .WithWebSocketServer(o => o.WithUri(WssUrl));

                if (!string.IsNullOrWhiteSpace(user))
                    builder = builder.WithCredentials(user, pass);

                _opts = builder.Build();

                _client = new MqttFactory().CreateMqttClient();

                // Messages entrants
                _client.ApplicationMessageReceivedAsync += e =>
                {
                    var topic = e.ApplicationMessage.Topic ?? string.Empty;
                    var payload = e.ApplicationMessage.PayloadSegment.Count > 0
                        ? Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)
                        : (e.ApplicationMessage.Payload is { Length: > 0 }
                            ? Encoding.UTF8.GetString(e.ApplicationMessage.Payload)
                            : string.Empty);

                    if (_subscriptions.TryGetValue(topic, out var sub))
                        sub.cb?.Invoke(payload, sub.index);

                    Message?.Invoke(topic, payload);
                    return Task.CompletedTask;
                };

                _client.ConnectedAsync += e =>
                {
                    _shouldReconnect = true;
                    Connected?.Invoke();
                    StartPingLoop();
                    return Task.CompletedTask;
                };

                _client.DisconnectedAsync += async e =>
                {
                    Disconnected?.Invoke();
                    StopPingLoop();

                    if (_shouldReconnect && _opts != null)
                    {
                        await Task.Delay(2000);
                        try { await _client!.ConnectAsync(_opts); }
                        catch (Exception ex) { Error?.Invoke($"Reconnexion: {ex.Message}"); }
                    }
                };

                var result = await _client.ConnectAsync(_opts);
                return result.ResultCode == MqttClientConnectResultCode.Success
                    ? "Connecté"
                    : $"Échec: {result.ResultCode}";
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message);
                return $"Erreur: {ex.Message}";
            }
        }


        public async Task PublishAsync(string topic, string payload)
        {
            if (_client?.IsConnected != true)
            {
                return;
            }

            var msg = new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload ?? string.Empty)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await _client.PublishAsync(msg);
        }


        /// <summary>
        /// Déconnexion propre + arrêt du ping.
        /// </summary>
        public async Task DisconnectAsync()
        {
            _shouldReconnect = false; // on n’essaie plus de se reconnecter
            StopPingLoop();

            if (_client == null)
                return;

            try
            {
                var disc = new MqttClientDisconnectOptions();
                await _client.DisconnectAsync(disc);
            }
            catch (Exception ex)
            {
                Error?.Invoke($"Disconnect: {ex.Message}");
            }
            finally
            {
                _client?.Dispose();
                _client = null;
                _subscriptions.Clear();
                _pingHealthy = false;
            }
        }

        /// <summary>
        /// Publication simple (QoS1, retain configurable via param).
        /// </summary>
        public async Task PublishAsync(string topic, string payload, bool retain = true, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            if (!IsConnected) return;

            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload ?? string.Empty)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag(retain)
                .Build();

            await _client.PublishAsync(msg);
        }

        public async Task PublishTemperatureMax(string value)
        {
            if (!IsConnected) return;

            await PublishAsync("/Sup/TemperatureMax", value, retain: true, qos: MqttQualityOfServiceLevel.AtLeastOnce);
            
        }
        public async Task PublishConsigneTemperature(string value)
        {
            if (!IsConnected) return;

            await PublishAsync("/Sup/ConsigneTemperture", value, retain: true, qos: MqttQualityOfServiceLevel.AtLeastOnce);

        }
        /// <summary>
        /// “Momentary” : envoie true puis false après 500 ms (QoS1 retain).
        /// </summary>
        public async Task PublishMomentaryAsync(string topic, int holdMs = 500)
        {
            if (!IsConnected) return;

            await PublishAsync(topic, "true", retain: true, qos: MqttQualityOfServiceLevel.AtLeastOnce);
            await Task.Delay(holdMs);
            await PublishAsync(topic, "false", retain: true, qos: MqttQualityOfServiceLevel.AtLeastOnce);
        }

        /// <summary>
        /// Abonnement à un topic exact avec callback (payload, index).
        /// </summary>
        public async Task SubscribeAsync(string topic, Action<string, int> callback, int index = 0, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            if (!IsConnected) return;

            _subscriptions[topic] = (callback, index);
            await _client.SubscribeAsync(topic, qos);
        }

        /// <summary>
        /// Désabonnement d’un topic.
        /// </summary>
        public async Task UnsubscribeAsync(string topic)
        {
            if (!IsConnected) return;

            _subscriptions.TryRemove(topic, out _);
            await _client.UnsubscribeAsync(topic);
        }

        /// <summary>
        /// Lance une boucle de ping (QoS1) qui met à jour IsPingHealthy.
        /// </summary>
        public void StartPingLoop()
        {
            _pingHealthy = true;
            _pingTimer?.Dispose();

            _pingTimer = new Timer(async _ =>
            {
                if (!IsConnected)
                {
                    _pingHealthy = false;
                    return;
                }

                try
                {
                    var msg = new MQTTnet.MqttApplicationMessageBuilder()
                        .WithTopic("health/ping")
                        .WithPayload("ping")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    await _client!.PublishAsync(msg);
                    _pingHealthy = true;
                }
                catch
                {
                    _pingHealthy = false;
                }
            }, null, TimeSpan.Zero, _pingPeriod);
        }

        private void StopPingLoop()
        {
            _pingTimer?.Dispose();
            _pingTimer = null;
            _pingHealthy = false;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
    }
}
