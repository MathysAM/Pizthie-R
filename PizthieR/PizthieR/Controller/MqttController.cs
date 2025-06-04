using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizthieR.Controller
{
    public class MqttController
    {
        public MqttController() 
        {
        }

        public async void Connection(string name,string mdp)
        {
            await App.MqttController.ConnectAsync("wss://4d1f194df18748a393eeabb274d5e439.s1.eu.hivemq.cloud:8884/mqtt", name, mdp);
        }

    }
}
