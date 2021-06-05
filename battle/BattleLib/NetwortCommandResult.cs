using System.Collections.Generic;

namespace BattleLib
{
    public class NetworkCommandResult
    {
        public IList<NetworkDelta> DeltaList { get; set; }
        public bool Valid { get; set; }
    }
}