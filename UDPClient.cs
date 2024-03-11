using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace UDP_MQTT_Gateway
{
    public class UDPClient
    {
        int receiverPort;
        UdpClient? udpReceiver;
        IPEndPoint? receiveEndPoint;
        MQTTCLient? mqttClient;

        TestHDLCommand testHDLCommand = new();

        public void SetMqttClient(MQTTCLient mc)
        {
            mqttClient = mc;
        }

        public void Init()
        {
            receiverPort = 6000;
            udpReceiver = new UdpClient(receiverPort);
            receiveEndPoint = new IPEndPoint(IPAddress.Any, receiverPort);
        }       

        public async Task Listen()
        { 
            while(true)
            {
                //Logika za "ABCD" u while petlji
                byte[]? recvBuffer = null;
                if (udpReceiver != null)
                {
                    recvBuffer = udpReceiver.Receive(ref receiveEndPoint);
                } 

                if (recvBuffer != null)
                {
                    if (recvBuffer.Length > 22)
                    {
                        string code = recvBuffer[21].ToString("X") + recvBuffer[22].ToString("X");
                        byte temperature = recvBuffer[25];
                        if (code.ToString().Equals("ABCD"))
                        {
                            string message = temperature.ToString();
                            if (mqttClient != null)
                            {
                                await mqttClient.MyPublish(message);
                            }
                            
                        }
                        else if (code == "032")
                        {
                            byte subnetID = recvBuffer[17];
                            byte deviceID = recvBuffer[18];
                            byte channel = recvBuffer[25];
                            byte level = recvBuffer[27];
                            
                            string message = $"{subnetID} {deviceID} {channel} {level}";
                            if (mqttClient != null)
                            {
                                await mqttClient.MyPublish(message);
                            }
                            
                        }  
                    }
                }
            }          
        }

        public void Broadcast(string? message)
        {
            if(message != null)
            {
                List<HDLCommand>? commands = JsonSerializer.Deserialize<List<HDLCommand>>(message);

                if (commands != null)
                {
                    foreach (HDLCommand c in commands)
                    {
                        if (c.Address != null)
                        {
                            string[] addresses = c.Address.Split('-');
                            byte[] bytes = new byte[addresses.Length + 1];
                            for (int i = 0; i < addresses.Length; i++)
                            {
                                bytes[i] = Convert.ToByte(addresses[i]);
                            }
                            if (c.Command == "OFF")
                            {
                                bytes[addresses.Length] = 0;
                            }
                            else if (c.Command == "ON")
                            {
                                bytes[addresses.Length] = 0x64;
                            }

                            testHDLCommand.SingleChannelLightingControll(bytes[0], bytes[1], bytes[2], bytes[3]);
                        }       
                    }
                }    
            }
        }
    }
}
