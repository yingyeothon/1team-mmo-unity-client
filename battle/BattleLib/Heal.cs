using System;

namespace BattleLib
{
    public class Heal : IBattleCommand
    {
        public TargetType TargetType { get; set; } = TargetType.Self;
        public BattleCommandType CommandType => BattleCommandType.Heal;
        public Ship Source { get; set; }

        public void ExecuteCommand(Context c)
        {
            if (Source.Mp < 1) throw new NotEnoughMpException();

            Source.Mp--;
            
            c.AddDelta(new Delta {DeltaType = DeltaType.Heal, Source = Source, Target = Source});
            c.AddDelta(new Delta {DeltaType = DeltaType.Mp, Source = Source, Target = Source, Value = -1});

            var oldHp = Source.Hp;
            Source.Hp = Math.Min(oldHp + 1, Source.MaxHp);
            
            c.AddDelta(new Delta {DeltaType = DeltaType.Hp, Source = Source, Target = Source, Value = Source.Hp - oldHp});
        }

        public void SetTarget(Ship s)
        {
            throw new CannotChangeTargetException();
        }
    }
}