namespace BattleLib
{
    public class SingleAttack : IBattleCommand
    {
        public SingleAttack(Ship s1, Ship s2)
        {
            throw new System.NotImplementedException();
        }

        public SingleAttack()
        {
        }

        public TargetType TargetType { get; set; }
        public BattleCommandType CommandType => BattleCommandType.SingleAttack;
        public Ship Source { get; set; }
        Ship Target { get; set; }

        public void ExecuteCommand(Context c)
        {
            var source = Source;
            var target = Target;
            
            ExecuteBasicSingleAttack(c, source, target);
        }

        public static void ExecuteBasicSingleAttack(IDeltaAccumulator deltaAccumulator, Ship source, Ship target)
        {
            deltaAccumulator.AddDelta(new Delta {DeltaType = DeltaType.Fire, Source = source, Target = target});
            
            var damage = source.AttackRate;
            target.Hp -= damage;
            deltaAccumulator.AddDelta(new Delta {DeltaType = DeltaType.Hp, Source = source, Target = target, Value = -damage});
            if (target.Hp <= 0)
            {
                deltaAccumulator.AddDelta(new Delta {DeltaType = DeltaType.Destroyed, Source = source, Target = target});
            }
        }

        public void SetTarget(Ship s)
        {
            Target = s;
        }
    }
}