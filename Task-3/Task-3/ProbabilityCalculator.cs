using System;
using System.Collections.Generic;
using System.Linq;

namespace Task_3
{
    public class ProbabilityCalculator
    {
        public double[,] CalculateWinProbabilities(List<Dice> dice)
        {
            var size = dice.Count;
            var probs = new double[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    probs[i, j] = i == j ? 1.0 / 3.0 : CalculateWinProbability(dice[i], dice[j]);

            return probs;
        }

        public double CalculateWinProbability(Dice d1, Dice d2)
        {
            var wins = d1.diceFaces.SelectMany(f1 => d2.diceFaces.Select(f2 => f1 > f2)).Count(x => x);
            return (double)wins / (d1.diceFaces.Length * d2.diceFaces.Length);
        }
    }
}