namespace BattleLib
{
    public class NetworkDelta
    {
        public DeltaType DeltaType { get; set; }
        public long Source { get; set; }
        public long Target { get; set; }
        public int Value { get; set; }
    }
}