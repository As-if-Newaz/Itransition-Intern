using System;
using System.Linq;
using System.Collections.Generic;

namespace Task_3
{
    public class DiceParser
    {
        public List<Dice> ParseDiceConfigurations(string[] args)
        {
            if (args.Length < 3 && args.Length > 0)
            { Console.WriteLine(Prompt.InvalidDiceCount + "\n" + Prompt.example); Environment.Exit(0); }

            var diceList = args.Select(arg => new Dice(ParseFaces(arg))).ToList();
            ValidateConfigurations(diceList);
            
            return diceList;
        }

        public int[] ParseFaces(string input)
        {
            ParseCheck(input);
            return input.Split(',').Select(int.Parse).ToArray();
        }

        public void ValidateConfigurations(List<Dice> dice)
        {
            
            var firstLength = dice[0].diceFaces.Length;
            if (dice.Any(d => d.diceFaces.Length != firstLength))
            {Console.WriteLine(Prompt.InvalidFaceCount + "\n" + Prompt.example); Environment.Exit(0);}
        }

        public void ParseCheck(string faceString)
        {
            var facesStrArray = faceString.Split(',');
            if (string.IsNullOrWhiteSpace(faceString) || facesStrArray.Length == 0)
            {
                Console.WriteLine(Prompt.EmptyDiceConfiguration);
            }
            var faces = new int[facesStrArray.Length];
            for (int i = 0; i < facesStrArray.Length; i++)
            {
                if (!int.TryParse(facesStrArray[i], out faces[i]))
                {
                    Console.WriteLine(Prompt.InvalidFaceValues + "\n" + Prompt.example);
                    Environment.Exit(0);
                }
            }
        }
    }
}