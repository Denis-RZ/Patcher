using System;

namespace TestProject
{
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
        
        public int Multiply(int a, int b)
        {
            return a * b;
        }
        
        public void PrintResult(int result)
        {
            Console.WriteLine("Result: " + result);
        }
    }
}
