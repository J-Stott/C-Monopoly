namespace MonopolyComponent
{
    //holds player money
    public class Wallet
    {
        public int Value { get; private set; } = 2000;

        public void AddToWallet(int money)
        {
            Value += money;
        }

        public void TakeFromWallet(int money)
        {
            Value -= money;
        }
    }
}