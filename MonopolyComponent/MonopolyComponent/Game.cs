using System;
using System.Collections.Generic;
using System.Threading;

namespace MonopolyComponent
{
    class Game
    {
        private readonly GameManager _gameManager;
        public Game(int numTurns, int numPlayers)
        {
            _gameManager = new GameManager(numTurns, numPlayers);
        }

        public void PlayGame()
        {
            _gameManager.PlayGame();
        }



    }
}