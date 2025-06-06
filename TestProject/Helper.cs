using System;

namespace TestProject
{
    public static class Helper
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
        
        public static int Square(int number)
        {
            return number * number;
        }
    }
}
