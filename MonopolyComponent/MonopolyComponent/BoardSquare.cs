using System;

namespace MonopolyComponent
{
    //Represents a board square. We pass an event handler to it to deal with a player landing on the square
    public class BoardSquare
    {
        public string Name { get; }
        public event EventHandler<LandArgs> LandEventHandler;

        protected BoardSquare(string name)
        {
            Name = name;
        }

        public void OnLanded(Player player)
        {
            Console.WriteLine("Player {0} landed on {1}", player.PlayerType.ToString(), Name);

            LandEventHandler?.Invoke(this, new LandArgs(player));
        }

        public void SetEventHandler(EventHandler<LandArgs> landEventHandler)
        {
            
            if (LandEventHandler == null)
            {
                LandEventHandler += landEventHandler;
            }
            else
            {
                LandEventHandler = null;
                LandEventHandler += landEventHandler;
            }
        }


        public static class Factory
        {
            public static BoardSquare GetBoardSquare(string name, EventHandler<LandArgs> landHandler)
            {
                var newSquare = new BoardSquare(name);
                newSquare.SetEventHandler(landHandler);
                return newSquare;
            }
        }
    }

    public class TaxSquare : BoardSquare
    {
        public int TaxValue { get; private set; }
        protected TaxSquare(string name, int tax) : base(name)
        {
            TaxValue = tax;
        }

        public static class TaxFactory
        {
            public static BoardSquare GetTaxSquare(string name, EventHandler<LandArgs> landHandler, int tax)
            {
                var newSquare = new TaxSquare(name, tax);
                newSquare.SetEventHandler(landHandler);
                return newSquare;
            }
        }
    }

}