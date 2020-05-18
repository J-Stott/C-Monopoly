using System;
using System.Collections.Generic;

namespace MonopolyComponent
{
    public class Ownership
    {
        private int _stationMultiplier;
        private int _utilityMultiplier = 1;
        private readonly Dictionary<string, OwnerDeed> _ownedProperties = new Dictionary<string, OwnerDeed>();

        public void IncreaseStationMultiplier()
        {
            _stationMultiplier++;
        }

        public void IncreaseUtilityMultiplier()
        {
            _utilityMultiplier *= 3;
        }

        public int GetStationMultiplier()
        {
            return _stationMultiplier;
        }

        public int GetUtilityMultiplier()
        {
            return _utilityMultiplier;
        }

        public void AddDeed(OwnerDeed deed)
        {
            _ownedProperties.Add(deed.Name, deed);
        }

        public OwnerDeed GetDeed(string name)
        {
            if (!_ownedProperties.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException( name, "You have provided an incorrect deed name for this player");
            }

            return _ownedProperties[name];
        }
    }
}