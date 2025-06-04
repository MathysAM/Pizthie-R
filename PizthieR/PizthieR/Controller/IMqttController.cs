using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizthieR.Controller
{
    public interface IMqttController
    {
        Task ConnectAsync(string url, string user, string pass);
        Task DisconnectAsync();
        Task PublishAsync(string topic, string value);
    }
}
