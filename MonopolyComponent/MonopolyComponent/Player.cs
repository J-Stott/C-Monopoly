using System;

namespace MonopolyComponent
{
    public enum PlayerType
    {
        Car = 0,
        Dog,
        Hat,
        Iron,
        Boat,
        Boot
    }

    //handles all player functionality. Includes one event handler.
    //The player cares when another player lands on a square they own
    public class Player
    {
        public PlayerType PlayerType { get; private set; }
        private int _jailTurns = 0;
        private readonly Wallet _wallet = new Wallet();
        private int _boardPosition;
        private readonly Ownership _ownership = new Ownership();
        private bool _loopedBoard = false;

        public Player(PlayerType type)
        {
            PlayerType = type;
        }

        public void PayToPlayer(int value)
        {
            _wallet.AddToWallet(value);
        }

        public void TakeFromPlayer(int value)
        {
            _wallet.TakeFromWallet(value);
        }

        public int GetPlayerMoney()
        {
            return _wallet.Value;
        }

        public void SetPlayerPosition(int boardPosition)
        {
            _boardPosition = boardPosition;
        }

        public int GetBoardPosition()
        {
            return _boardPosition;
        }

        public void SetJailTerm(int numTurns)
        {
            _jailTurns = numTurns;
        }

        public void ReduceJailTerm()
        {
            --_jailTurns;
        }

        public int GetJailTurns()
        {
            return _jailTurns;
        }

        public bool IsFree()
        {
            return _jailTurns == 0;
        }

        public void LoopedBoard()
        {
            _loopedBoard = true;
        }

        public bool HasPassedGo()
        {
            if (_loopedBoard && _boardPosition > 0)
            {
                _loopedBoard = false;
                return true;
            }

            return false;
        }

        public int GetStationMultiplier()
        {
            return _ownership.GetStationMultiplier();
        }

        public int GetUtilityMultiplier()
        {
            return _ownership.GetUtilityMultiplier();
        }

        public void IncreaseStationMultiplier()
        {
            _ownership.IncreaseStationMultiplier();
        }

        public void IncreaseUtilityMultiplier()
        {
            _ownership.IncreaseUtilityMultiplier();
        }

        public void AddDeed(OwnerDeed deed)
        {
            switch (deed)
            {
                case StationDeed station:
                    IncreaseStationMultiplier();
                    break;
                case UtilityDeed utility:
                    IncreaseUtilityMultiplier();
                    break;
            }

            deed.SetOwner(this);
            _ownership.AddDeed(deed);
        }

        public int GetOwnedSquareRent(string name)
        {
            return _ownership.GetDeed(name).GetRent();
        }

        public void onPlayerLandsOnOwnedSquare(object obj, LandArgs args)
        {
            var square = (BoardSquare)obj;
            var player = args.Player;

            if (player == this)
            {
                Console.WriteLine("Player {0} enjoyed their time on {1}", player.PlayerType.ToString(), square.Name);
            }
            else
            {
                var playerMoney = player.GetPlayerMoney();
                var rentCost = GetOwnedSquareRent(square.Name);
                if (playerMoney >= rentCost)
                {
                    Console.WriteLine("Player {0} is paying {1} rent to Owner {2}", player.PlayerType.ToString(), rentCost, this.PlayerType.ToString());
                    player.TakeFromPlayer(rentCost);
                    PayToPlayer(rentCost);

                }
                else
                {
                    Console.WriteLine("Player {0} cannot afford to pay rent to Owner {1}", player.PlayerType.ToString(), this.PlayerType.ToString());
                }
            }
        }

    }
}