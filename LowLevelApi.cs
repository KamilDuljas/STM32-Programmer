using System;
using System.IO.Ports;

namespace STM32Programmer
{
    public class LowLevelApi
    {
        private readonly SerialPort Serial;

        public LowLevelApi(SerialPort serial)
        {
            this.Serial=serial;
        }

        public void SendCommand(byte[] command) => Serial.Write(command, 0, command.Length);

        public bool WaitForOk(int timeout = 5000, string pattern = "OK!")
        {
            string response = "";
            DateTime start = DateTime.Now;
            do
            {
                try
                {
                    response = Serial.ReadTo("\n");
                    Console.WriteLine(response);
                    //response += "\n";
                    if(response.Contains(pattern)) 
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Wait for output....");
                }
            } while ((DateTime.Now - start).TotalMilliseconds < timeout);
            return false;
        }

        public string GetResponse(int timeout)
        {
            string response = "";
            var start = DateTime.Now;
            do
            {
                try
                {
                    response = Serial.ReadTo("\n");
                    Console.WriteLine(response);
                    response += "\n";
                    return response;
                }
                catch (Exception)
                {
                    Console.WriteLine("Wait for output....");
                }
            } while ((DateTime.Now - start).TotalMilliseconds < timeout);
            return response;
        }
    }
}