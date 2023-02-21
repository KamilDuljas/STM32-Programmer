using System;
using System.IO;

namespace STM32Programmer
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("HELP: \nPass path to bin file as argument");
                Console.WriteLine("Example: STM32Programmer C:\\file.bin");
                return 0;
            }

            if (!Utils.CheckFile(args[0]))
                return 1;

            // Startup CRC tests
            CRC.SanityCheck();

            Bootloader bootloader = new Bootloader();

            if (!bootloader.Echo())
                return 1;

            FileStream file = File.OpenRead(args[0]);

            if(!bootloader.SetFirmwareSize((uint)file.Length))
                return 1;

            if (!bootloader.UpdateFirmware(args[0]))
                return 1;

            if (!bootloader.VerifyChecksum(file))
                return 1;

            // TODO: jump to app after prompt
            //Console.WriteLine("Are you jump to the application now? ()")
            return 0;
        }
    }
}