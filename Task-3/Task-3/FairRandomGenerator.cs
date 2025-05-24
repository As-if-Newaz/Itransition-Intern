using System;

namespace Task_3
{
    public class FairRandomGenerator
    {
        private Hash hash { get; set; }

        public FairRandomGenerator(Hash hash)
        {
            this.hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public int GenerateFairNumber(int min, int max)
        {
            var number = hash.GenerateSecureRandomNumber(min, max);
            var key = hash.GenerateSecureRandomKey();
            var hmac = hash.CalculateHMAC(key, number);
            Console.WriteLine("I selected a random value in the range " + min + ".." + max + "\n(HMAC=" + hmac + ")");
            var userNumber = GetUserSelection(min, max);
            Console.WriteLine("My number is " + number + " (KEY=" + Convert.ToHexString(key) + ")");
            var result = (number + userNumber) % (max - min + 1);
            Console.WriteLine($"The fair number generation result is {number} + {userNumber} = {result} ( mod {(max - min + 1)}).");
            return result;
        }

        public int GetUserSelection(int min, int max)
        {
            PrintUserPrompt(min, max);
            while (true)
            {
                Console.Write(Prompt.UserSelection);
                var input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "X") Environment.Exit(0);
                else if(input == "H") {Console.WriteLine(Prompt.IngameHelp); continue;}
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($" Invalid Selection, Please enter a number between {min} and {max}");
            }
        }

        public void PrintUserPrompt(int min, int max)
        {
            Console.WriteLine(Prompt.GuessSelection);
            for (int i = min; i <= max; i++)
            {
                Console.WriteLine($"{i} - {i}");
            }
            Console.WriteLine(Prompt.GameOptions);
          
        }

    }
}