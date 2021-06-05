using System.Collections.Generic;

namespace BattleLib
{
    public interface IBattleCommand
    {
        TargetType TargetType { get; set; }
        BattleCommandType CommandType { get; }
        Ship Source { get; set; }
        Ship Target { get; set; }
        void ExecuteCommand(Context c);
    }
}