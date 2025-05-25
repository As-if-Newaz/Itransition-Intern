using System;
using System.Collections.Generic;
using System.Linq;
using Task_3;

namespace Task_3
{
    public class GameMenu
    {
        private List<Dice> dice { get; set; }
        private ProbabilityCalculator calc { get; set; }
        private TableRenderer tableRenderer { get; set; }

        public GameMenu(List<Dice> dice, ProbabilityCalculator calc, TableRenderer tableRenderer)
        {
            this.dice = dice;
            this.calc = calc;
            this.tableRenderer = tableRenderer;
        }

        public int ShowDiceSelectionMenu(int exclude = -1)
        {
            while (true)
            {
                Console.WriteLine(Prompt.UserChoice);
                DisplayOptions(exclude); 
                Console.WriteLine(Prompt.GameOptions);
                Console.Write(Prompt.UserSelection);
                var input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "X") Environment.Exit(0);
                if (input == "H") ShowHelp();
                else if (int.TryParse(input, out int idx) && idx >= 0 && idx < dice.Count && idx != exclude)
                {
                    Console.WriteLine("You choose the " + dice[idx].displayTxt + " dice.");
                    return idx;
                }
                Console.WriteLine(Prompt.InvalidSelection);
            }
        }

        public void DisplayOptions(int exclude)
        {
            for (int i = 0; i < dice.Count; i++)
                if (i != exclude)
                    Console.WriteLine($"{i} - {dice[i].displayTxt}");
        }

        public void ShowHelp()
        {
            Console.WriteLine(Prompt.GameDescription);
            Console.WriteLine(Prompt.ProbabilityTableDescription);
            var probs = calc.CalculateWinProbabilities(dice);
            tableRenderer.RenderProbabilityTable(dice, probs);
            Console.WriteLine(Prompt.ContinuePrompt);
            Console.ReadLine();
        }
    }
}
