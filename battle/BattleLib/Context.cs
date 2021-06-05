using System.Collections.Generic;
using System.Linq;

namespace BattleLib
{
    public class Context : IDeltaAccumulator
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
            if (cmd == null)
            {
                if (currentTurnParty.IsBot)
                {
                    return ExecuteCommandBot();
                }
                
                throw new CommandNullException();
            }
            
            if (cmd.Source == null) throw new SourceNullException();

            if (cmd.Source != CurrentTurnShip) throw new InvalidBattleStateException();

            cmd.ExecuteCommand(this);

            var result = new CommandResult {Valid = true};

            foreach (var d in deltaList)
            {
                result.DeltaList.Add(d);
            }

            // 다음 행동할 Ship이 현재 Ship과 다르고, HP가 남아있는 Ship이 있는 Party가 한 개보다 많다면
            // 다음 행동할 Ship으로 턴 넘겨준다.
            // 그게 아니라면 전투는 결판이 난 것이다.
            if (currentTurnShip != cmd.Source.NextShip
                && partyList.Count(p => p.ShipList.Any(s => s.Hp > 0)) > 1)
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

        CommandResult ExecuteCommandBot()
        {
            if (currentTurnParty.IsBot == false)
            {
                throw new InvalidBattleStateException();
            }
            
            var enemyParty = partyList.FirstOrDefault(e => e != currentTurnParty);
            if (enemyParty == null)
            {
                throw new InvalidBattleStateException();
            }

            var enemyShip = enemyParty.ShipList.FirstOrDefault(e => e.Hp > 0);
            if (enemyShip == null)
            {
                throw new InvalidBattleStateException();
            }

            var singleAttack = new SingleAttack {Source = currentTurnShip};
            singleAttack.SetTarget(enemyShip);

            return ExecuteCommand(singleAttack);
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