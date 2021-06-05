using System.Collections.Generic;

namespace BattleLib
{
    public class Bombard : IBattleCommand
    {
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.Bombard;
        public Ship Source { get; set; }
        public Ship Target { get; set; }

        public void ExecuteCommand(Context c)
        {
            var accumulator = new BombardDeltaAccumulator();

            foreach (var targetShip in Target.Party.ShipList)
            {
                SingleAttack.ExecuteBasicSingleAttack(accumulator, Source, targetShip);
            }

            accumulator.SortDeltaList();

            foreach (var delta in accumulator.DeltaList)
            {
                c.AddDelta(delta);
            }
        }
    }

    class BombardDeltaAccumulator : IDeltaAccumulator
    {
        readonly List<Delta> deltaList = new List<Delta>();

        public void AddDelta(Delta delta)
        {
            deltaList.Add(delta);
        }

        static readonly Dictionary<DeltaType, int> DeltaTypePrecedence = new Dictionary<DeltaType, int>
        {
            {DeltaType.Fire, 1},
            {DeltaType.Precision, 2},
            {DeltaType.Hp, 3},
            {DeltaType.Mp, 4},
            {DeltaType.Heal, 5},
            {DeltaType.Destroyed, 6},
            {DeltaType.TurnChanged, 7}
        };

        public void SortDeltaList()
        {
            deltaList.Sort((a, b) => DeltaTypePrecedence[a.DeltaType] - DeltaTypePrecedence[b.DeltaType]);
        }

        public IEnumerable<Delta> DeltaList => deltaList;
    }
}