using System.Collections.Generic;

namespace BattleLib
{
    public class CommandResult
    {
        public IList<Delta> DeltaList => deltaList;
        public bool Valid { get; set; }

        readonly List<Delta> deltaList = new List<Delta>();
    }
}