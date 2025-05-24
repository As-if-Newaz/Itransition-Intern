using ConsoleTables;
using System;
using System.Collections.Generic;


namespace Task_3
{
    public class TableRenderer
    {
        public void RenderProbabilityTable(List<Dice> dice, double[,] probabilities)
        {
            int diceCount = dice.Count;
            var columns = new List<string> { "User dice v" };
            columns.AddRange(dice.ConvertAll(d => d.displayTxt));
            var table = new ConsoleTable(columns.ToArray());
            for (int i = 0; i < diceCount; i++)
            {
                var row = new List<string> { dice[i].displayTxt };
                for (int j = 0; j < diceCount; j++)
                {
                    string cell = i == j ? "- (" + probabilities[i, j].ToString("F4") + ")" : probabilities[i, j].ToString("F4");
                    row.Add(cell);
                }
                table.AddRow(row.ToArray());
            }
            table.Options.EnableCount = false;
            table.Write(Format.Alternative);
        }
    }
}