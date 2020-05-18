using System;

namespace MonopolyComponent
{
    public static class Dice
    {
        public static int RollDice()
        {
            var rand = new Random(Guid.NewGuid().GetHashCode());
            return rand.Next(2, 12);
        }
    }
}