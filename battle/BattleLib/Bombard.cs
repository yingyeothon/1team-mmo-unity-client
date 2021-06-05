namespace BattleLib
{
    public class Bombard : IBattleCommand
    {
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.Bombard;
        public Ship Source { get; set; }
        Ship Target { get; set; }
        public void ExecuteCommand(Context c)
        {
            foreach (var targetShip in Target.Party.ShipList)
            {
                SingleAttack.ExecuteBasicSingleAttack(c, Source, targetShip);
            }
        }

        public void SetTarget(Ship s)
        {
            Target = s;
        }
    }
}