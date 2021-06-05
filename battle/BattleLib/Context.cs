using System.Collections.Generic;
using System.Linq;

namespace BattleLib
{
    public class Context
    {
        public void AddParty(Party p)
        {
            if (p.Context != null && p.Context != this) throw new ConflictedRelationException();

            if (partyList.Contains(p)) throw new DuplicatedRelationException();

            partyList.Add(p);
            p.Context = this;
            foreach (var s in p.ShipList)
            {
                s.Context = this;
            }
        }

        public void SetCurrentTurnParty(Party p)
        {
            if (State != ContextState.UndefinedTurn) throw new InvalidBattleStateException();

            currentTurnParty = p;
            currentTurnShip = p.ShipList[0];
        }

        public CommandResult ExecuteCommand(IBattleCommand cmd)
        {
            if (cmd.Source == null) throw new SourceNullException();

            if (cmd.Source != CurrentTurnShip) throw new InvalidBattleStateException();

            cmd.ExecuteCommand(this);

            var result = new CommandResult {Valid = true};

            foreach (var d in deltaList)
            {
                result.DeltaList.Add(d);
            }
            
            if (currentTurnShip != cmd.Source.NextShip && partyList.Count(p => p.ShipList.Count(s => s.Hp > 0) > 0) > 1)
            {
                currentTurnShip = cmd.Source.NextShip;
                currentTurnParty = currentTurnShip.Party;
                var delta = new Delta
                    {DeltaType = DeltaType.TurnChanged, Source = currentTurnShip, Target = currentTurnShip};
                result.DeltaList.Add(delta);
            }
            else
            {
                finished = true;
                WinParty = currentTurnShip.Party;
                currentTurnShip = null;
                currentTurnParty = null;
            }

            deltaList.Clear();
            
            return result;
        }

        public ContextState State => GetState();

        Party currentTurnParty;
        Ship currentTurnShip;

        ContextState GetState()
        {
            if (partyList.Count == 0) return ContextState.Empty;

            if (partyList.Count == 1) return ContextState.SingleParty;

            if (partyList.Any(e => e.ShipList.Count == 0)) return ContextState.PartyEmpty;

            if (finished) return ContextState.Finished; 

            if (currentTurnParty == null) return ContextState.UndefinedTurn;
            
            return ContextState.Ready;
        }

        public Party WinParty { get; private set; } = null;
        public Ship CurrentTurnShip => currentTurnShip;
        public IList<Party> PartyList => partyList;

        readonly List<Party> partyList = new List<Party>();

        public void AbandonTurn(Ship s)
        {
            if (CurrentTurnShip != s) throw new InvalidBattleStateException();
        }

        public void AddDelta(Delta delta)
        {
            deltaList.Add(delta);
        }
        
        readonly List<Delta> deltaList = new List<Delta>();
        bool finished;
    }
}