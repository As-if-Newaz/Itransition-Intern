using System;
using System.Collections.Generic;

namespace Task_3
{
    public class Game
    {
        public List<Dice> dice;
        public FairRandomGenerator random;
        public GameMenu menu;
        public Hash hash;

        public Game(List<Dice> dice, FairRandomGenerator random, GameMenu menu, Hash hash)
        {
            this.dice = dice;
            this.random = random;
            this.menu = menu;
            this.hash = hash;
        }

        public void Start()
        {
            var computerFirst = random.GenerateFairNumber(0, 1) == 1;
            var (computerDice, userDice) = SelectDice(computerFirst);
            PlayRound(computerDice, userDice);
        }

        public (Dice computer, Dice user) SelectDice(bool computerFirst)
        {
            if (computerFirst)
            {
                var computerIndex = hash.GenerateSecureRandomNumber(0, dice.Count - 1);
                Console.WriteLine($"I make the first move and choose the " + dice[computerIndex].displayTxt + " dice");
                return (dice[computerIndex], dice[menu.ShowDiceSelectionMenu(computerIndex)]);
            }
            return UserFirst();
        }

        public void PlayRound(Dice computerDice, Dice userDice)
        {
            var computerRoll = computerDice.Roll(random.GenerateFairNumber(0, computerDice.diceFaces.Length - 1));
            var userRoll = userDice.Roll(random.GenerateFairNumber(0, userDice.diceFaces.Length - 1));
            Console.WriteLine($"My Roll : {computerRoll}, Your Roll : {userRoll}");
            Console.WriteLine(userRoll > computerRoll ? "You Won!" : computerRoll > userRoll ? "I won!" : "It's a Tie!");
        }

        public (Dice computer, Dice user) UserFirst()
        {
            Console.WriteLine(Prompt.userFirstMove);
            var userIndex = menu.ShowDiceSelectionMenu();
            var availableIndices = new List<int>();
            for (int i = 0; i < dice.Count; i++) if (i != userIndex) availableIndices.Add(i);
            var computerIndex = hash.GenerateSecureRandomNumber(0, dice.Count - 1);
            Console.WriteLine($"And I choose the "+computerIndex+" no. dice = " + dice[computerIndex].displayTxt);
            return (dice[computerIndex], dice[userIndex]);
        }
        
    }
}