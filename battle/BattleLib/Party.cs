using System.Collections.Generic;

namespace BattleLib
{
    public class Party
    {
        public void AddShip(Ship s)
        {
            if (s.Party != null && s.Party != this) throw new ConflictedRelationException();
            
            if (shipList.Contains(s)) throw new DuplicatedRelationException();
            
            shipList.Add(s);
            s.Party = this;
        }

        public IList<Ship> ShipList => shipList;
        public Context Context { get; set; }

        readonly List<Ship> shipList = new List<Ship>();
    }
}