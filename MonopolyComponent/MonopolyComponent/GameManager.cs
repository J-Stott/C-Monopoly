using System;
using System.Collections.Generic;
using System.Threading;

namespace MonopolyComponent
{
    public class LandArgs : EventArgs
    {
        public Player Player { get; }

        public LandArgs(Player p)
        {
            Player = p;
        }
    }

    //the game manager handles the creation of the board, the players and the gameplay
    //also contains majority of event handlers for landing on a board square
    public class GameManager
    {
        private readonly int _numTurns;
        private int _passGoReward = 200;
        private readonly List<Player> _players = new List<Player>();
        private readonly List<PlayerModifierCard> _chanceCards = new List<PlayerModifierCard>();
        private readonly List<PlayerModifierCard> _communityChestCards = new List<PlayerModifierCard>();
        private readonly Board _board = new Board();

        readonly Dictionary<string, OwnerDeed> _deeds = new Dictionary<string, OwnerDeed>();

        public GameManager(int numTurns, int numPlayers)
        {
            _numTurns = numTurns;

            InitGame(numPlayers);
        }

        public void PlayGame()
        {
            for (var i = 0; i < _numTurns; ++i)
            {
                foreach (var player in _players)
                {
                    TakeTurn(player);
                }
            }

            EndGame();
        }

        public int GetSquarePosition(string squareName)
        {
            return _board.GetSquarePosition(squareName);
        }

        private void TakeTurn(Player player)
        {
            if (player.IsFree())
            {
                var currentPosition = player.GetBoardPosition();
                var diceRoll = Dice.RollDice();
                var loopedBoard = (currentPosition + diceRoll) > _board.GetBoardSize();

                if (loopedBoard)
                {
                    player.LoopedBoard();
                }


                var newPosition = (currentPosition + diceRoll) % _board.GetBoardSize();
                player.SetPlayerPosition(newPosition);

                if (player.HasPassedGo())
                {
                    PassGo(player);
                }


                LandPlayerOnSquare(player);
            }
            else
            {
                player.ReduceJailTerm();
                Console.WriteLine("Player {0} is in jail for {1} more turn(s)", player.PlayerType.ToString(), player.GetJailTurns());
            }

            Thread.Sleep(1000);
        }

        private void EndGame()
        {
            var winner = _players[0];
            var winningMoney = winner.GetPlayerMoney();

            foreach (var player in _players)
            {
                var money = player.GetPlayerMoney();
                Console.WriteLine("Player {0} finished with {1} Monopoly Money", player.PlayerType.ToString(), money);

                if (money <= winningMoney) continue;
                winningMoney = money;
                winner = player;
            }

            Console.WriteLine("The winner was Player {0}", winner.PlayerType.ToString());
        }

        public List<Player> GetPlayers()
        {
            return _players;
        }

        public void LandPlayerOnSquare(Player player)
        {
            var currentPosition = player.GetBoardPosition();
            var square = _board.GetSquare(currentPosition);
            square.OnLanded(player);
        }

        private void PassGo(Player player)
        {
            Console.WriteLine("Player {0} has passed go! Player has collected {1}", player.PlayerType.ToString(), _passGoReward);
            player.PayToPlayer(_passGoReward);
        }

        public void AddDeed(OwnerDeed deed)
        {
            _deeds.Add(deed.Name, deed);
        }

        private void OnPlayerLandsOnRegularSquare(object obj, LandArgs args)
        {
            //obj should always be a board square
            var square = (BoardSquare) obj;

            Console.WriteLine("Player {0} relaxes on {1}", args.Player.PlayerType.ToString(), square.Name);
        }

        private void OnPlayerLandsOnBuyableSquare(object obj, LandArgs args)
        {
            var square = (BoardSquare)obj;
            var deed = _deeds[square.Name];
            var player = args.Player;
            var playerMoney = player.GetPlayerMoney();
            if (playerMoney >= deed.Cost)
            {
                Console.WriteLine("Player {0} is buying {1}", player.PlayerType.ToString(), square.Name);
                player.TakeFromPlayer(deed.Cost);
                player.AddDeed(deed);
                _deeds.Remove(square.Name);

                //set new event handler here for purchased squares
                square.SetEventHandler(player.onPlayerLandsOnOwnedSquare);
            }
            else
            {
                Console.WriteLine("Player {0} cannot afford {1}", player.PlayerType.ToString(), square.Name);
            }
        }


        private void OnPlayerLandsOnTaxSquare(object obj, LandArgs args)
        {
            var square = (TaxSquare)obj;
            var player = args.Player;
            var tax = square.TaxValue;

            Console.WriteLine("Player {0} has payed {1} in tax", player.PlayerType.ToString(), tax);
            player.TakeFromPlayer(tax);

        }

        private void OnPlayerLandsOnGoToJailSquare(object obj, LandArgs args)
        {
            var jailPosition = _board.GetJailPosition();
            var player = args.Player;

            Console.WriteLine("Player {0} has gone to jail. Player {0} has gone directly to jail. Has not passed go or collected 200", player.PlayerType.ToString());
            player.SetPlayerPosition(jailPosition);
            player.SetJailTerm(2);
        }

        private void OnPlayerLandsOnChanceSquare(object obj, LandArgs args)
        {
            var player = args.Player;

            Console.WriteLine("Player {0} has drawn a Chance card", player.PlayerType.ToString());
            var rnd = new Random();
            var index = rnd.Next(_chanceCards.Count);

            var card = _chanceCards[index];
            card.PlayerDrawsCard(this, player);
        }

        private void OnPlayerLandsOnCommunityChestSquare(object obj, LandArgs args)
        {
            var player = args.Player;

            Console.WriteLine("Player {0} has drawn a Community Chest card", player.PlayerType.ToString());
            var rnd = new Random();
            var index = rnd.Next(_communityChestCards.Count);

            var card = _chanceCards[index];
            card.PlayerDrawsCard(this, player);
        }

        private void InitGame(int numPlayers)
        {
            for (var i = 0; i < numPlayers; ++i)
            {
                var playerType = (PlayerType)i;
                var player = new Player(playerType);

                _players.Add(player);
            }

            AddBoardSquares();
            SetupCards();
        }

        //square construction functions
        private void AddSquare(string name, EventHandler<LandArgs> landHandler)
        {
            var newSquare = BoardSquare.Factory.GetBoardSquare(name, landHandler);
            _board.AddNewSquare(newSquare);
        }

        private void AddTaxSquare(string name, EventHandler<LandArgs> landHandler, int tax)
        {
            var newSquare = TaxSquare.TaxFactory.GetTaxSquare(name, landHandler, tax);
            _board.AddNewSquare(newSquare);
        }

        private void AddBuyableSquare<TDeed>(string name, EventHandler<LandArgs> landHandler, int cost, int rent)
            where TDeed : OwnerDeed
        {
            var newSquare = BoardSquare.Factory.GetBoardSquare(name, landHandler);

            AddDeed(DeedMaker.MakeDeed<TDeed>(name, cost, rent));
            _board.AddNewSquare(newSquare);
        }

        private void AddBoardSquares()
        {
            AddSquare("Go", OnPlayerLandsOnRegularSquare);
            AddBuyableSquare<OwnerDeed>("Old Kent Road", OnPlayerLandsOnBuyableSquare, 60, 6);
            AddSquare("Community Chest", OnPlayerLandsOnCommunityChestSquare);
            AddBuyableSquare<OwnerDeed>("Whitechapel Road", OnPlayerLandsOnBuyableSquare, 60, 6);
            AddTaxSquare("Income Tax", OnPlayerLandsOnTaxSquare, 200);
            AddBuyableSquare<StationDeed>("Kings Cross Station", OnPlayerLandsOnBuyableSquare, 200, 25);
            AddBuyableSquare<OwnerDeed>("The Angel Islington", OnPlayerLandsOnBuyableSquare, 100, 10);
            AddSquare("Chance", OnPlayerLandsOnChanceSquare);
            AddBuyableSquare<OwnerDeed>("Euston Road", OnPlayerLandsOnBuyableSquare, 100, 10);
            AddBuyableSquare<OwnerDeed>("Pentonville Road", OnPlayerLandsOnBuyableSquare, 120, 12);
            AddSquare("Jail", OnPlayerLandsOnRegularSquare);
            AddBuyableSquare<OwnerDeed>("Pall Mall", OnPlayerLandsOnBuyableSquare, 140, 14);
            AddBuyableSquare<UtilityDeed>("Electric Company", OnPlayerLandsOnBuyableSquare, 150, 0);
            AddBuyableSquare<OwnerDeed>("Whitehall", OnPlayerLandsOnBuyableSquare, 140, 14);
            AddBuyableSquare<OwnerDeed>("Northumberland Avenue", OnPlayerLandsOnBuyableSquare, 160, 16);
            AddBuyableSquare<StationDeed>("Marylebone Station", OnPlayerLandsOnBuyableSquare, 200, 25);
            AddBuyableSquare<OwnerDeed>("Bow Street", OnPlayerLandsOnBuyableSquare, 180, 18);
            AddSquare("Community Chest", OnPlayerLandsOnCommunityChestSquare);
            AddBuyableSquare<OwnerDeed>("Marlborough Street", OnPlayerLandsOnBuyableSquare, 180, 18);
            AddBuyableSquare<OwnerDeed>("Vine Street", OnPlayerLandsOnBuyableSquare, 200, 20);
            AddSquare("Free Parking", OnPlayerLandsOnRegularSquare);
            AddBuyableSquare<OwnerDeed>("Strand", OnPlayerLandsOnBuyableSquare, 220, 22);
            AddSquare("Chance", OnPlayerLandsOnChanceSquare);
            AddBuyableSquare<OwnerDeed>("Fleet Street", OnPlayerLandsOnBuyableSquare, 220, 22);
            AddBuyableSquare<OwnerDeed>("Trafalgar Square", OnPlayerLandsOnBuyableSquare, 240, 24);
            AddBuyableSquare<StationDeed>("Fenchurch Street Station", OnPlayerLandsOnBuyableSquare, 200, 25);
            AddBuyableSquare<OwnerDeed>("Leicester Square", OnPlayerLandsOnBuyableSquare, 260, 26);
            AddBuyableSquare<OwnerDeed>("Coventry Street", OnPlayerLandsOnBuyableSquare, 260, 26);
            AddBuyableSquare<UtilityDeed>("Water Works", OnPlayerLandsOnBuyableSquare, 150, 0);
            AddBuyableSquare<OwnerDeed>("Picadilly", OnPlayerLandsOnBuyableSquare, 280, 28);
            AddSquare("Go To Jail", OnPlayerLandsOnGoToJailSquare);
            AddBuyableSquare<OwnerDeed>("Regent Street", OnPlayerLandsOnBuyableSquare, 300, 30);
            AddBuyableSquare<OwnerDeed>("Oxford Street", OnPlayerLandsOnBuyableSquare, 300, 30);
            AddSquare("Community Chest", OnPlayerLandsOnCommunityChestSquare);
            AddBuyableSquare<OwnerDeed>("Bond Street", OnPlayerLandsOnBuyableSquare, 320, 32);
            AddBuyableSquare<StationDeed>("Liverpool Street Station", OnPlayerLandsOnBuyableSquare, 200, 25);
            AddSquare("Chance", OnPlayerLandsOnChanceSquare);
            AddBuyableSquare<OwnerDeed>("Park Lane", OnPlayerLandsOnBuyableSquare, 350, 35);
            AddTaxSquare("Income Tax", OnPlayerLandsOnTaxSquare, 100);
            AddBuyableSquare<OwnerDeed>("Mayfair", OnPlayerLandsOnBuyableSquare, 400, 40);
        }

        private void SetupCards()
        {
            var jailCard = new PlayerModifierCard("Go to Jail. Go Directly To Jail. Do not pass Go. Do not collect 200", new JailAction());
            _chanceCards.Add(jailCard);
            _communityChestCards.Add(jailCard);

            var bankCard = new PlayerModifierCard("Bank error in your favour. Collect 50", new GiveMoneyAction(50));
            _chanceCards.Add(bankCard);

            var goCard = new PlayerModifierCard("Advance to Go", new MoveAction("Go"));
            _chanceCards.Add(goCard);

            var mayfairCard = new PlayerModifierCard("Advance to Mayfair", new MoveAction("Mayfair"));
            _chanceCards.Add(mayfairCard);

            var feesCard = new PlayerModifierCard("Pay School Fees. 150", new TakeMoneyAction(150));
            _chanceCards.Add(feesCard);

            var crosswordCard = new PlayerModifierCard("You have won a crossword competition. 100", new GiveMoneyAction(100));
            _chanceCards.Add(crosswordCard);

            var drunkCard = new PlayerModifierCard("Drunk in charge fine. 20", new TakeMoneyAction(20));
            _chanceCards.Add(drunkCard);

            var beautyCard = new PlayerModifierCard("You've won second prize in a beauty contest. Collect 10", new GiveMoneyAction(10));
            _communityChestCards.Add(beautyCard);

            var insuranceCard = new PlayerModifierCard("Pay your insurance premium. 50", new TakeMoneyAction(50));
            _communityChestCards.Add(insuranceCard);

            var kentCard = new PlayerModifierCard("Go back to Old Kent Road", new MoveAction("Old Kent Road"));
            _communityChestCards.Add(kentCard);

            var hospitalCard = new PlayerModifierCard("Hospital Bill. 100", new TakeMoneyAction(100));
            _communityChestCards.Add(hospitalCard);

            var taxCard = new PlayerModifierCard("Income tax refund. 20", new GiveMoneyAction(20));
            _communityChestCards.Add(taxCard);

            var annuityCard = new PlayerModifierCard("Annuity matures. 100", new GiveMoneyAction(100));
            _communityChestCards.Add(annuityCard);
        }

    }
}
