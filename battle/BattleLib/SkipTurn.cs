namespace BattleLib
{
    public class SkipTurn : IBattleCommand
    {
        public SkipTurn()
        {
        }

        public SkipTurn(Ship s) : this()
        {
            Source = s;
        }
        
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.SkipTurn;
        public Ship Source { get; set; }
        public Ship Target
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public void ExecuteCommand(Context c)
        {
            c.AbandonTurn(Source);
        }
    }
}