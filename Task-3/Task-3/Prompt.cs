using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_3
{
    public class Prompt
    {
        public const string InvalidDiceCount = "Please specify at least three dice";

        public const string InvalidFaceCount = "The dice must have same number of faces!";

        public const string InvalidFaceValues = "All face values must be positive integers";

        public const string EmptyDiceConfiguration = "Dice configuration cannot be empty";

        public const string InvalidDiceFormat = "Invalid dice format. Use comma-separated integers";

        public const string GameDescription =
        "This game demonstrates non-transitive relations with custom dice\n" +
        "In each round, both players select different dice and roll them\n" +
        "The player who rolls the higher number wins";

        public const string ProbabilityTableDescription =
            "\nProbability table showing win chances for each dice pair:";

        public const string UserChoice = "Choose your dice: ";

        public const string GameOptions = "X - exit\nH - help";

        public const string IngameHelp = "Please select a number from the list to add to the computer's selection";

        public const string UserSelection = "Your Selection: ";

        public const string ContinuePrompt = "Press Enter to continue...";

        public const string GuessSelection = "Try to guess my selection";

        public const string userFirstMove = "You make the first move";

        public const string GameStart = "Welcome to the Non-Transitive Dice Game!";

        public const string InvalidSelection = "Invalid selection, please try again.";

        public const string example = "Example: dotnet run 2,2,4,4,9,9 6,8,1,1,8,6 7,5,3,7,5,3";
    }
}
