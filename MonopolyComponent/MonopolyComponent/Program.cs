using System;

namespace MonopolyComponent
{
    class Program
    {

        static void Main(string[] args)
        {
            var game = new Game(10, 4);

            game.PlayGame();
        }
    }
}
