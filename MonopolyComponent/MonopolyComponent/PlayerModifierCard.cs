using System;

namespace MonopolyComponent
{
    public class PlayerModifierCard
    {
        private readonly string _description;
        private readonly IAction _action;

        public PlayerModifierCard(string description, IAction action)
        {
            _description = description;
            _action = action;
        }

        public void PlayerDrawsCard(GameManager manager, Player player)
        {
            Console.WriteLine(_description);
            _action.ActionOnPlayer(manager, player);
        }
    }
}