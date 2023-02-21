using System;
using System.IO;

namespace STM32Programmer
{
    public static class Utils
    {
        public static bool CheckFile(string binFile)
        {
            if (!File.Exists(binFile))
            {
                Console.WriteLine($"File does not exist. Filepath: {binFile}");
                return false;
            }
            return true;
        }
    }
}