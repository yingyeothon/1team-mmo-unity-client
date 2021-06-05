namespace BattleLib
{
    public class DoubleHp : IBattleCommand
    {
        public DoubleHp()
        {
        }

        public DoubleHp(Ship s) : this()
        {
            Source = s;
            Target = s;
        }
        
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.DoubleHp;
        public Ship Source { get; set; }
        public Ship Target { get; set; }

        public void ExecuteCommand(Context c)
        {
            var oldHp = Target.Hp;
            Target.Hp += oldHp;
            c.AddDelta(new Delta{Source = Source, Target = Target, DeltaType = DeltaType.Hp, Value = oldHp});
        }
    }
}