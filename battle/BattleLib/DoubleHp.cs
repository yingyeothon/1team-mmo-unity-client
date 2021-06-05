namespace BattleLib
{
    public class DoubleHp : IBattleCommand
    {
        public DoubleHp(Ship s)
        {
            Source = s;
            SetTarget(s);
        }
        
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.DoubleHp;
        public Ship Source { get; set; }
        Ship Target { get; set; }

        public void ExecuteCommand(Context c)
        {
            var oldHp = Target.Hp;
            Target.Hp += oldHp;
            c.AddDelta(new Delta{Source = Source, Target = Target, DeltaType = DeltaType.Hp, Value = oldHp});
        }

        public void SetTarget(Ship s)
        {
            Target = s;
        }
    }
}