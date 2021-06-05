using System.Collections.Generic;

namespace BattleLib
{
    public class Delta
    {
        public DeltaType DeltaType { get; set; }
        public Ship Source { get; set; }
        public Ship Target { get; set; }
        public int Value { get; set; }
    }
}