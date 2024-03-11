using NLog;
using NullFX.CRC;

namespace UDP_MQTT_Gateway
{
    public class TestChecksum
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        readonly string A = "0F 1E FD FF FE 00 31 13 61 01 00 00 00";
        readonly string cA = "05 87"; // ovo je checksuma koju treba dobiti
        readonly string B = "0F 1E FD FF FE 00 31 13 61 01 64 00 00";
        readonly string cB = "42 2C";
        readonly string C = "0F 1E FD FF FE 00 31 13 64 01 64 00 00"; // pali svetlo
        readonly string cC = "61 7B";
        readonly string D = "0F 1E FD FF FE 00 31 13 64 01 00 00 00"; // gasi svetlo
        readonly string cD = "26 D0";

        public TestChecksum()
        {
            byte[] a = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, 0x13, 0x61, 0x01, 0x00, 0x00, 0x00];
            byte[] b = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, 0x13, 0x61, 0x01, 0x64, 0x00, 0x00];
            byte[] c = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, 0x13, 0x64, 0x01, 0x64, 0x00, 0x00];
            byte[] d = [0x0F, 0x1E, 0xFD, 0xFF, 0xFE, 0x00, 0x31, 0x13, 0x64, 0x01, 0x00, 0x00, 0x00];

            var crca = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, a);
            var crcb = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, b);
            var crcc = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, c);
            var crcd = Crc16.ComputeChecksum(Crc16Algorithm.Ccitt, d);

            logger.Info(crca.ToString("X"));
            logger.Info(crcb.ToString("X"));
            logger.Info(crcc.ToString("X"));
            logger.Info(crcd.ToString("X"));
        }
    }
}
