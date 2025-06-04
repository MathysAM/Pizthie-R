using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;


using PizthieR.Controller; // car l’interface est dans Controller/

namespace PizthieR.Browser.Services
{
    public partial class MqttControllerWeb : IMqttController
    {
        [JSImport("connectMqtt", "mqtt.js")]
        internal static partial void ConnectMqtt(string url, string user, string pass);

        [JSImport("disconnectMqtt", "mqtt.js")]
        internal static partial void DisconnectMqtt();

        [JSImport("publishMqttValue", "mqtt.js")]
        internal static partial void PublishMqttValue(string topic, string value);

        public Task ConnectAsync(string url, string user, string pass)
        {
            ConnectMqtt(url, user, pass);
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            DisconnectMqtt();
            return Task.CompletedTask;
        }

        public Task PublishAsync(string topic, string value)
        {
            PublishMqttValue(topic, value);
            return Task.CompletedTask;
        }
    }
}
