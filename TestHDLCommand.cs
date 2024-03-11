using NullFX.CRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace UDP_MQTT_Gateway
{
    internal class TestHDLCommand
    {
        Logger logger;
        byte[] header = [0x00, 0x00, 0x00, 0x00, 0x48, 0x44, 0x4c, 0x4d, 0x49, 0x52, 0x41, 0x43, 0x4c, 0x45, 0xaa, 0xaa];

        public TestHDLCommand()
        {
            byte[] c = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, 0x13, 0x64, 0x01, 0x64, 0x00, 0x00];
            ushort crcc = 0;
            for (int i = 0; i < c.Length; i++)
            {
                crcc = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, c);
            }
            byte[] crccByte = BitConverter.GetBytes(crcc);
            (crccByte[1], crccByte[0]) = (crccByte[0], crccByte[1]);
            IEnumerable<byte> content = header.Concat(c).Concat(crccByte);

            //foreach (byte b in content)
            //{
            //    Console.Write(b.ToString("X") + " ");
            //}

        }
        public void SingleChannelLightingControll(byte subnetId, byte deviceId, byte channel, byte level)
        {
            byte[] c = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, subnetId, deviceId, channel, level, 0x00, 0x00];
            
            ushort crcc = 0;
            for (int i = 0; i < c.Length; i++)
            {
                crcc = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, c);
            }
            byte[] crccByte = BitConverter.GetBytes(crcc);
            (crccByte[1], crccByte[0]) = (crccByte[0], crccByte[1]);
            IEnumerable<byte> content = header.Concat(c).Concat(crccByte);

            UdpClient udpClient = new UdpClient();
            byte[] sendBytes = content.ToArray();
            try
            {
                udpClient.Send(sendBytes, sendBytes.Length, "255.255.255.255", 6000);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                logger.Info(e.ToString());
            }
            //Console.WriteLine($"{DateTime.Now} Bytes sent to address: {subnetId}-{deviceId}-{channel} with command {level}.");
            logger.Info($"{DateTime.Now} Bytes sent to address: {subnetId}-{deviceId}-{channel} with command {level}.");
        }

    }
}
