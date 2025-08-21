using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizthieR.Controller
{
    public class MqttController
    {
        private MQTTnet.Client.IMqttClient _client;
        private MQTTnet.Client.MqttClientOptions _opts;   // <<< PAS IMqttClientOptions

        private const string WssUrl = "wss://4d1f194df18748a393eeabb274d5e439.s1.eu.hivemq.cloud:8884/mqtt";
       

        public MqttController() 
        {
        }
        public async Task<string> ConnectAsync(string user, string pass)
        {
            try
            {
                var builder = new MQTTnet.Client.MqttClientOptionsBuilder()
                    .WithWebSocketServer(WssUrl) // WebSocket obligatoire en WASM
                    .WithClientId($"wasm_{Guid.NewGuid():N}".Substring(0, 16))
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

                if (!string.IsNullOrWhiteSpace(user))
                    builder = builder.WithCredentials(user, pass);

                _opts = builder.Build();

                _client = new MQTTnet.MqttFactory().CreateMqttClient();

                // Event pour les messages entrants
                _client.ApplicationMessageReceivedAsync += e =>
                {
                   
                    return Task.CompletedTask;
                };

                // Event pour reconnexion
                _client.DisconnectedAsync += async e =>
                {
                    
                    await Task.Delay(2000);
                    try { await _client.ConnectAsync(_opts); } catch { }
                };

                // Ici, on attend vraiment le résultat
                var result = await _client.ConnectAsync(_opts);

                if (result.ResultCode == MQTTnet.Client.MqttClientConnectResultCode.Success)
                {
                    return "Connecté"; // ✅ seulement si succès
                }
                else
                {
                    return $"Échec: {result.ResultCode}";
                }
            }
            catch (Exception ex)
            {
               
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
