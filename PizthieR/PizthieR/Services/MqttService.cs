using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace PizthieR.Services;

public class MqttService
{
    private readonly IMqttClient _client;

    public MqttService()
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
    }

    public async Task ConnectAsync(string uri, string username, string password, CancellationToken cancellationToken = default)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithWebSocketServer(uri)
            .WithCredentials(username, password)
            .WithTls()
            .Build();

        await _client.ConnectAsync(options, cancellationToken);
    }
}