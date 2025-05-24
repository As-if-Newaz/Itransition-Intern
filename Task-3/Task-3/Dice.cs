using System.Linq;

namespace Task_3
{
    public class Dice
    {
        public int[] diceFaces { get; set; }
        public string displayTxt { get; set; }
        public Dice(int[] faces)
        {
            if (faces.Length == 0)
            {
                throw new ArgumentException(Prompt.EmptyDiceConfiguration);
            }
            else
            {
                diceFaces = faces;
                displayTxt = string.Join(",", faces);
            }
        }
        public int Roll(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= diceFaces.Length)
            {
                throw new ArgumentException(nameof(faceIndex), "Face index must be within the range 0 and " + (diceFaces.Length - 1));
            }
            return diceFaces[faceIndex];
        }

    }
}