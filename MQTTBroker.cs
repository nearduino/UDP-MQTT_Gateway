using MQTTnet;
using MQTTnet.Server;
using NLog;
using System.Text;
using System.Text.Json;

namespace UDP_MQTT_Gateway
{
    public class MQTTBroker
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        static string storePath = "Broker/RetainedMessages.json";
        static string username = "Victoria";
        static string password = "victoriasecret";


        static MqttServerOptionsBuilder options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(1883)
            .WithPersistentSessions();
        
        // D: static znaci da moze biti samo jedan broker
        static MqttServer broker = new MqttFactory()
            .CreateMqttServer(options.Build());

        public async Task Start()
        {
            broker.LoadingRetainedMessageAsync += ServerOnLoadingRetainedMessageAsync;

            try
            {
                await broker.StartAsync();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                logger.Info(ex.ToString());
            }

            //Console.WriteLine("Broker: Started.");
            logger.Info("Broker: Started.");

            broker.ClientConnectedAsync += Broker_ClientConnectedAsync;
            broker.ValidatingConnectionAsync += Client_Validation;
            broker.InterceptingPublishAsync += Broker_InterceptingPublishAsync;
            broker.ClientDisconnectedAsync += Broker_ClientDisconnectedAsync;
        }

        private Task Broker_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            //Console.WriteLine("Client disconnected: ClientId = {0}, Endpoint = {1}", arg.ClientId, arg.Endpoint);
            logger.Info("Client disconnected: ClientId = {0}, Endpoint = {1}", arg.ClientId, arg.Endpoint);
            return Task.CompletedTask;
        }

        private Task Client_Validation(ValidatingConnectionEventArgs args)
        {
            if (args.UserName != username || args.Password != password)
            {
                //Console.WriteLine("Client is not authorized (wrong username or password)");
                logger.Info("Client is not authorized (wrong username or password)");
                return Task.FromException(new Exception("Client is not authorized"));
            }
            return Task.CompletedTask;
        }

        private Task Broker_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            var payloadSegment = arg.ApplicationMessage.PayloadSegment;
            string? payload = null;
            try
            {
                payload = Encoding.UTF8.GetString(payloadSegment.Array!, payloadSegment.Offset, payloadSegment.Count); ;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                logger.Info(ex.Message);
            }

            if (payload != null)
            {
                //Console.WriteLine(
                //" TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",
                //DateTime.Now,
                //arg.ClientId,
                //arg.ApplicationMessage?.Topic,
                //payload,
                //arg.ApplicationMessage?.QualityOfServiceLevel,
                //arg.ApplicationMessage?.Retain);
                
                logger.Info(
                " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",
                DateTime.Now,
                arg.ClientId,
                arg.ApplicationMessage?.Topic,
                payload,
                arg.ApplicationMessage?.QualityOfServiceLevel,
                arg.ApplicationMessage?.Retain);
            }
            return Task.CompletedTask;
        }

        private Task Broker_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            //Console.WriteLine("New connection: ClientId = {0}, Endpoint = {1}", arg.ClientId, arg.Endpoint);
            logger.Info("New connection: ClientId = {0}, Endpoint = {1}", arg.ClientId, arg.Endpoint);

            return Task.CompletedTask;
        }

        private async Task ServerOnLoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
        {
            var buffer = JsonSerializer.SerializeToUtf8Bytes(arg.LoadedRetainedMessages);
            await File.WriteAllBytesAsync(storePath, buffer);
            logger.Info("Zadrzane poruke su snimljene");
        }
    }
}
