namespace BattleLib
{
    public class SkipTurn : IBattleCommand
    {
        public SkipTurn(Ship s)
        {
            Source = s;
        }
        
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.SkipTurn;
        public Ship Source { get; set; }
        public void ExecuteCommand(Context c)
        {
            c.AbandonTurn(Source);
        }

        public void SetTarget(Ship s)
        {
            throw new System.NotImplementedException();
        }
    }
}