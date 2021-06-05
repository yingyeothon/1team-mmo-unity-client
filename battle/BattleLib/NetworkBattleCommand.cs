namespace BattleLib
{
    public class NetworkBattleCommand
    {
        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType { get; set; }
        public long Source { get; set; }
        public long Target { get; set; }
    }
}