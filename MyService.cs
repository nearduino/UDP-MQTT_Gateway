using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDP_MQTT_Gateway
{
    internal class MyService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service starting...");

            MyStart();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service stopping...");
            return Task.CompletedTask;
        }

        public async void MyStart()
        {
            MQTTBroker mqttBroker = new();
            MQTTCLient mqttClientSender = new();
            UDPClient udpClientSender = new();

            await mqttBroker.Start();
            await mqttClientSender.Start("MQTT_sender");
            udpClientSender.Init();

            mqttClientSender.SetClient(udpClientSender);
            udpClientSender.SetMqttClient(mqttClientSender);

            await udpClientSender.Listen();

            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info("Ja sam loger, eee");

            Console.ReadLine();
        }
    }
}
