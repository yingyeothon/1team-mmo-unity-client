namespace BattleLib
{
    public class Precision : IBattleCommand
    {
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.Precision;
        public Ship Source { get; set; }
        public Ship Target
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public void ExecuteCommand(Context c)
        {
            Source.AttackRate = 2;
            c.AddDelta(new Delta{Source = Source, Target = Source, DeltaType = DeltaType.Precision});
        }
    }
}