using System;
using DSL;
namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Sample.GenerateSample(1);
            Console.WriteLine("\rPress any key to continue ..."); Console.ReadKey();
            Sample.Test(1);
            Console.WriteLine("\rPress any key to continue ..."); Console.ReadKey();
            Sample.Generate("[ FORWARD 2; MOMENTUM 1.05; REPEAT(-20, [TURN 45; GO])]");
            Console.WriteLine("\rPress any key to continue ..."); Console.ReadKey();
        }
    }
}
