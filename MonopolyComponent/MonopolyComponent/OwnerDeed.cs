using System;
using System.Reflection.Metadata.Ecma335;

namespace MonopolyComponent
{
    //represents the owning of a property.
    //contains cost, rent owed and the name of the property
    public class OwnerDeed
    {
        protected Player Owner = null;
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public int Rent { get; private set; }

        public virtual int GetRent()
        {
            return Rent;
        }

        public void SetOwner(Player owner)
        {
            Owner = owner;
        }

        public OwnerDeed(string name, int cost, int rent)
        {
            Name = name;
            Cost = cost;
            Rent = rent;
        }
    }

    public class StationDeed : OwnerDeed
    {

        public override int GetRent()
        {
            return Rent * Owner.GetStationMultiplier();
        }

        public StationDeed(string name, int cost, int rent) : base(name, cost, rent)
        {

        }
    }

    public class UtilityDeed : OwnerDeed
    {

        public override int GetRent()
        {
            return Dice.RollDice() * Owner.GetUtilityMultiplier();
        }

        public UtilityDeed(string name, int cost, int rent) : base(name, cost, rent)
        {

        }
    }

    public static class DeedMaker
    {
        public static OwnerDeed MakeDeed<T>(string name, int cost, int rent) where T : OwnerDeed
        {
            return (OwnerDeed)Activator.CreateInstance(typeof(T), name, cost, rent);
        }
    }
}