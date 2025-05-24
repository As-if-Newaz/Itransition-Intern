using System;
using Task_3;

namespace Task_3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var dice = new DiceParser().ParseDiceConfigurations(args);
                var hash = new Hash();
                var calc = new ProbabilityCalculator();
                var tableRenderer = new TableRenderer();
                var menu = new GameMenu(dice, calc, tableRenderer);
                var random = new FairRandomGenerator(hash);
                new Game(dice, random, menu, hash).Start();
            }
            catch
            {
                Console.WriteLine(Prompt.InvalidDiceFormat);
                Console.WriteLine(Prompt.example);
            }
        }
    }
}
