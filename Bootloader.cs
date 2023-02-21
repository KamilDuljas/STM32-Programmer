using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace STM32Programmer
{
    public class Bootloader
    {
        public readonly string BOOTLOADER_OK = "OK!";

        public SerialPort? SerialPort;

        public enum BootloaderCommand
        {
            INVALID = 0x0,
            ECHO = 0x1,
            SETSIZE = 0x02,
            UPDATE = 0x03,
            CHECK = 0x04,
            JUMP = 0x05
        }

        private LowLevelApi api;

        public Bootloader()
        {
            SerialPort = GetComPort();
            api = new LowLevelApi(SerialPort);
            Init();
        }

        private void Init()
        {
            Console.WriteLine("Programmer: Send Init");
            api.SendCommand(CreateCommand(BootloaderCommand.ECHO, 0));
            if (!api.WaitForOk(pattern: "Bootloader ready"))
                throw new Exception("Exception during init!. Check board.");
            Console.WriteLine("Programmer: Init Ok");
            Thread.Sleep(500);
        }
        public bool Echo(bool printOutput = false)
        {
            Console.WriteLine("Programmer: Send Echo");
            for (int i = 0; i<2; i++)
            {
                api.SendCommand(CreateCommand(BootloaderCommand.ECHO, 0));
                if (api.WaitForOk())
                {
                    Console.WriteLine("Programmer: Bootloader is ready to flash");
                    return true;
                }
                else
                {
                    Console.WriteLine("IsBootloaderRunning retry...");
                }
            }
            Console.WriteLine("Programmer: Bootloader is not respond.");
            return false;
        }

        public bool SetFirmwareSize(uint size)
        {
            Console.WriteLine("Programmer: Set firmware Size: 0x{0:X}", size);
            api.SendCommand(CreateCommand(BootloaderCommand.SETSIZE, size));
            return api.WaitForOk();
        }

        public bool UpdateFirmware(string FilePath)
        {
            api.SendCommand(CreateCommand(BootloaderCommand.UPDATE, 0));
            if (!api.WaitForOk(15000))
            {
                Console.WriteLine("Programmer: Error during flashing firmware");
                return false;
            }

            Console.WriteLine("Programmer: Start update firmware...");
            byte[] firmwareFile = File.ReadAllBytes(FilePath);
            int bytesSent = 0;
            int chunkSize = 1024;
            int firmwareSize = firmwareFile.Length;
            int chunkAmount = (int)Math.Ceiling(firmwareSize / (decimal)chunkSize);
            int chunkSent = 0;

            while (bytesSent < firmwareSize)
            {
                int bytesLeft = firmwareSize - bytesSent;
                int chunkLength = bytesLeft > chunkSize ? chunkSize : bytesLeft;
                int nextChunkOffset = bytesSent + chunkLength;
                byte[] firmwareSlice = firmwareFile[bytesSent..nextChunkOffset];
                Console.WriteLine($"Programmer: Sending {firmwareSlice.Length} bytes");
                SerialPort.Write(firmwareSlice, 0, firmwareSlice.Length);
                if (!api.WaitForOk(10000))
                {
                    Console.WriteLine("Programmer: Bootloader did not respond or returned an error while programming!");
                    return false;
                }
                chunkSent += 1;
                Console.WriteLine($"Programmer: Progress: {chunkSent}/{chunkAmount}");

                bytesSent += chunkLength;
            }
            Console.WriteLine("Programmer: Update finished, verifying firmware...");
            return api.WaitForOk(2000);
        }

        public bool VerifyChecksum(FileStream file)
        {
            BinaryReader binaryReader = new(file);

            var wordCount = file.Length / sizeof(uint);
            uint[] words = new uint[wordCount];

            for (int i = 0; i < wordCount; i++)
            {
                words[i] = binaryReader.ReadUInt32();
            }
            uint checksum = CRC.ComputeSTM32Checksum(words);
            Console.WriteLine("Programmer: Checksum of load file: 0x{0:X}", checksum);
            Console.WriteLine("Programmer: Verify flashed firmware...");
            api.SendCommand(CreateCommand(BootloaderCommand.CHECK, checksum));
            if (api.WaitForOk())
            {
                Console.WriteLine("Programmer: Firmware verified. UPDATE SUCCESSFUL");
                return true;
            }
            else
            {
                Console.WriteLine("Programmer: Firmware unverified. invalid checksum");
                return false;
            }
        }

        private SerialPort GetComPort()
        {
            SerialPort _serialPort = new();
            Console.WriteLine("Programmer: Select port:");
            string[] avaiablePorts = SerialPort.GetPortNames();
            foreach (string port in avaiablePorts)
                Console.WriteLine(port);

            while (true)
            {
                string portName = Console.ReadLine().ToUpper();
                if (portName == "" || !avaiablePorts.Contains(portName))
                    continue;
                else
                {
                    _serialPort.PortName = portName;
                    break;
                }
            }

            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 2000;
            try
            {
                _serialPort.Open();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();

                //_serialPort.Write(new byte[] { 1, 0, 0, 0, 0 }, 0, 5);

                //string response = _serialPort.ReadTo("\n");
                Console.WriteLine("Programmer: Connected " + _serialPort.PortName.ToUpper());
                //Console.WriteLine(response);
                Thread.Sleep(500);
                return _serialPort;
            }
            throw new Exception("Programmer: Port is close!");
        }

        private byte[] CreateCommand(BootloaderCommand command, uint data = 0)
        {
            byte[] arr = new byte[5];
            arr[0] = (byte)command;
            var tmp = BitConverter.GetBytes(data).Reverse().ToArray();
            tmp.CopyTo(arr, 1);
            return arr;
        }
    }
}