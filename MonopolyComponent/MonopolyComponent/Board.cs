using System;
using System.Collections.Generic;

namespace MonopolyComponent
{
    //holds all board related data
    public class Board
    {
        private readonly List<BoardSquare> _squares = new List<BoardSquare>();
        private int _jailPosition;

        public int GetBoardSize()
        {
            return _squares.Count;
        }

        public int GetJailPosition()
        {
            return _jailPosition;
        }

        public void AddNewSquare(BoardSquare square)
        {
            if (square.Name == "Jail")
            {
                _jailPosition = _squares.Count;
            }

            _squares.Add(square);
        }

        public int GetSquarePosition(string squareName)
        {
            for (var i = 0; i < _squares.Count; ++i)
            {
                if (squareName == _squares[i].Name)
                {
                    return i;
                }
            }

            throw new InvalidOperationException("You have passed a square name that doesn't exist on the board");
        }

        public BoardSquare GetSquare (int boardPosition)
        {
            return _squares[boardPosition];
        }
    }
}