using System.Diagnostics;

namespace STM32Programmer
{
    public static class CRC
    {
        public static void SanityCheck()
        {
            // Sanity check CRC mechanism
            uint result1 = ComputeSTM32Checksum(new uint[] { 0x12345678 });
            Trace.Assert(result1 == 0xDF8A8A2B);
            uint result2 = ComputeSTM32Checksum(new uint[] { 0x8E09BAF6 });
            Trace.Assert(result2 == 0x5C00CC44);
            uint result3 = ComputeSTM32Checksum(new uint[] { 0x12345678, 0x8E09BAF6 });
            Trace.Assert(result3 == 0x04F1F147);
            uint result4 = ComputeSTM32Checksum(new uint[] { 0x8E09BAF6, 0x12345678 });
            Trace.Assert(result4 == 0x90B2EE2D);
        }

        public static uint ComputeSTM32Checksum(uint[] inputData, uint initial = 0xFFFFFFFF, uint polynomial = 0x04C11DB7)
        {
            uint crc = initial;
            foreach (uint current in inputData)
            {
                crc ^= current;
                // Process all the bits in input data.
                for (uint bitIndex = 0; (bitIndex < 32); ++bitIndex)
                {
                    // If the MSB for CRC == 1
                    if ((crc & 0x80000000) != 0)
                    {
                        crc = ((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }
            return crc;
        }
    }
}