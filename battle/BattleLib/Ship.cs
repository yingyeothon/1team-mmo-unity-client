using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleLib
{
    public class IntProp
    {
        public int Current { get; set; } = 1;
        public int Max { get; set; } = 1;
    }
    
    public class Ship
    {
        public long Id { get; set; }

        public int Hp
        {
            get => hp.Current;
            set
            {
                hp.Current = value;
                hp.Max = Math.Max(hp.Current, hp.Max);
            } 
        }

        public int Mp
        {
            get => mp.Current;
            set
            {
                mp.Current = value;
                mp.Max = Math.Max(mp.Current, mp.Max);
            }
        }

        public int MaxHp
        {
            get => hp.Max;
            set
            {
                hp.Max = value;
                hp.Current = Math.Min(hp.Current, hp.Max);
            }
        }

        public int MaxMp
        {
            get => mp.Max;
            set
            {
                mp.Max = value;
                mp.Current = Math.Min(mp.Current, mp.Max);
            }
        }

        public ShipState State { get; } = ShipState.Alive;
        public IList<IBattleCommand> CommandList => commandList;
        public Party Party { get; set; }
        public Context Context { get; set; }
        public Ship NextShip => GetNextShip();
        public int AttackRate { get; set; } = 1;

        readonly IntProp hp = new IntProp();
        readonly IntProp mp = new IntProp();

        Ship GetNextShip()
        {
            var shipIndex = Party.ShipList.IndexOf(this);
            if (shipIndex + 1 < Party.ShipList.Count)
            {
                var nextShipWithinParty = Party.ShipList[shipIndex + 1];
                if (nextShipWithinParty.Hp == 0)
                {
                    return nextShipWithinParty.NextShip;
                }

                return nextShipWithinParty;
            }

            var partyIndex = Party.Context.PartyList.IndexOf(Party);
            var nextPartyIndex = (partyIndex + 1) % Party.Context.PartyList.Count;
            var nextShipInNextParty = Party.Context.PartyList[nextPartyIndex].ShipList[0];
            if (nextShipInNextParty.Hp == 0)
            {
                return nextShipInNextParty.NextShip;
            }

            return nextShipInNextParty;
        }

        public void LearnCommand(IBattleCommand cmd)
        {
            if (commandList.Contains(cmd)) throw new DuplicatedRelationException();
            
            if (commandList.Any(e => e.CommandType == cmd.CommandType)) throw new ConflictedBattleCommandTypeException();
            
            if (cmd.Source != null && cmd.Source != this) throw new ConflictedRelationException();
            
            commandList.Add(cmd);
            cmd.Source = this;
        }
        
        readonly List<IBattleCommand> commandList = new List<IBattleCommand>
        {
            new SingleAttack()
        };

        public Ship()
        {
            foreach (var cmd in commandList)
            {
                cmd.Source = this;
            }
        }
    }

    public enum ShipState
    {
        Alive,
        Destroyed
    }
}