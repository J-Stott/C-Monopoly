using System;
using System.Collections.Specialized;

//will change these for card actions later with Chance/Community Chest
namespace MonopolyComponent
{
    //actions for Chance/Community chest cards
    public interface IAction
    {
        void ActionOnPlayer(GameManager manager, Player player);
    }


    public class MoveAction : IAction
    {
        private readonly string _squareToMoveTo;

        public MoveAction(string squareToMoveTo)
        {
            _squareToMoveTo = squareToMoveTo;
        }
        public void ActionOnPlayer(GameManager manager, Player player)
        {
            var pos = manager.GetSquarePosition(_squareToMoveTo);
            player.SetPlayerPosition(pos);
            manager.LandPlayerOnSquare(player);

            if (_squareToMoveTo == "Go")
            {
                player.LoopedBoard();
            }
        }
    }

    public class GiveMoneyAction : IAction
    {
        private readonly int _moneyToGive;

        public GiveMoneyAction(int moneyToGive)
        {
            _moneyToGive = moneyToGive;
        }
        public void ActionOnPlayer(GameManager manager, Player player)
        {
            player.PayToPlayer(_moneyToGive);
        }
    }

    public class TakeMoneyAction : IAction
    {
        private readonly int _moneyToTake;

        public TakeMoneyAction(int moneyToTake)
        {
            _moneyToTake = moneyToTake;
        }
        public void ActionOnPlayer(GameManager manager, Player player)
        {
            player.TakeFromPlayer(_moneyToTake);
        }
    }

    public class JailAction : IAction
    {
        public void ActionOnPlayer(GameManager manager, Player player)
        {
            var jailPosition = manager.GetSquarePosition("Jail");
            player.SetPlayerPosition(jailPosition);
            player.SetJailTerm(2);
        }
    }
}