using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using NLog;

namespace UDP_MQTT_Gateway
{
    public class MQTTCLient
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        // Set options
        static string broker = "127.0.0.1";
        static int port = 1883;
        //static string clientId = "C#client";
        static string subTopic = "/dev/david/pub";
        static string pubTopic = "/dev/david/sub";
        static string username = "Victoria";
        static string password = "victoriasecret";
        UDPClient? udpClient;

        // Create a MQTT client factory
        static MqttFactory mqttFactory = new MqttFactory();

        // Create a MQTT client instance
        // D: Ovo nije static jer cu imati vise client instanci
        IMqttClient client = mqttFactory.CreateMqttClient();

        // Create MQTT client options
        MqttClientOptions options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port) // MQTT broker address and port
            .WithCredentials(username, password) // Set username and password
            .WithCleanSession()
            .Build();

        // Message received from UDPCLient and message to be sent to UDPClient
        public string? recvMessage;
        public string? sendMessage;

        public void SetClient(UDPClient c)
        {
            udpClient = c;
        }
        public async Task Start(string clientId)
        {
            // Eventovi
            // D: U njih dodajemo delegate sa +=
            client.ConnectedAsync += e =>
            {
                // Callback function when a message is received
                client.ApplicationMessageReceivedAsync += e =>
                {
                    if (udpClient != null)
                    {
                        udpClient.Broadcast(sendMessage);
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return Task.FromResult(new Exception("Ne moze broadcast, udpClient je null."));
                    }
                    
                };
                return Task.CompletedTask;
            };

            client.DisconnectedAsync += e =>
            {
                //Console.WriteLine("Client: Disconnected");
                return Task.CompletedTask;
            };
            
            options.ClientId = clientId;

            await MyConnect();
            await MySubscribe();           

            //Console.WriteLine("Press any key to stop the client...");
            //Console.ReadLine();
        }

        // Connect metoda
        // D: Pokusava da se konektuje na broker sve dok ne uspe
        public async Task MyConnect()
        {
            MqttClientConnectResultCode connectCode = MqttClientConnectResultCode.UnspecifiedError;
            while (connectCode != MqttClientConnectResultCode.Success)
            {
                try
                {
                    var connectResult = await client.ConnectAsync(options);
                    connectCode = connectResult.ResultCode;
                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }

                if (connectCode != MqttClientConnectResultCode.Success)
                {
                    logger.Info("Trying to connect...");
                    Thread.Sleep(1000);
                }
                else if (connectCode == MqttClientConnectResultCode.Success)
                {
                    logger.Info($"Client {client.Options.ClientId} successfuly connected to broker.");
                    break;
                }
            }
        }

        // Disconnect metoda
        public async Task MyDisconnect()
        {
            try
            {
                await client.DisconnectAsync();
            }
            catch (Exception e)
            {
                logger.Info(e.Message);
            }
        }

        // Publish metoda
        // D: Publish-uje prosledjeni string na pubTopic na temu 
        public async Task MyPublish(string messagePayload)
        {
            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(pubTopic)
                    .WithPayload(messagePayload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();
                if (client.IsConnected)
                {
                    recvMessage = messagePayload;
                    await client.PublishAsync(message);
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }
            logger.Info($"{DateTime.Now} Publishovao sam {messagePayload}.");
        }

        // Subscribe metoda
        // D: Subscribe-uje se na subTopic
        public async Task MySubscribe()
        {
            try
            {
                await client.SubscribeAsync(subTopic);
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
            }
            logger.Info($"Client {client.Options.ClientId} subscribed to topic: : {subTopic}.");
        }

        // Unsubscribe metoda
        // D: Unsubscribe-uje se sa subTopic-a
        public async Task MyUnsubscribe()
        {
            try
            {
                await client.UnsubscribeAsync(subTopic);
            }
            catch (Exception e)
            {
                logger.Info(e.Message);
            }
            logger.Info($"Client {client.Options.ClientId} unsubscribed from topic: {subTopic}");
        }

        //// Metoda za citanje iz registara
        //// D: Kasnije ce biti potrebna
        //public void ReadRegistry()
        //{
        //    RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\David");

        //    string us = "";
        //    string ps = "";
        //    int pt = 0;

        //    if (key != null)
        //    {
        //        us = (string)key.GetValue("MQTT_USERNAME");
        //        ps = (string)key.GetValue("MQTT_PASSWORD");
        //        pt = (int)key.GetValue("MQTT_PORT");
        //        key.Close();
        //    }
        //}
    }
}
