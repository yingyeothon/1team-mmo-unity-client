using System.Linq;
using BattleLib;
using Xunit;

namespace BattleLibTest
{
    public class BasicTest
    {
        // Context의 초기 상태
        [Fact]
        public void InitialContext()
        {
            var c = new Context();
            Assert.Equal(ContextState.Empty, c.State);
            Assert.Null(c.CurrentTurnShip);
            Assert.Empty(c.PartyList);
            Assert.Null(c.WinParty);
        }

        // Ship의 초기 상태
        [Fact]
        public void InitialShip()
        {
            var s = new Ship();
            Assert.Null(s.Context);
            Assert.Equal(1, s.Hp);
            Assert.Equal(1, s.Mp);
            Assert.Equal(1, s.MaxHp);
            Assert.Equal(1, s.MaxMp);
            Assert.Null(s.Party);
            Assert.Equal(ShipState.Alive, s.State);
        }

        [Fact]
        public void ShipHpMp()
        {
            var s = new Ship();
            Assert.Equal(1, s.Hp);
            Assert.Equal(1, s.Mp);
            Assert.Equal(1, s.MaxHp);
            Assert.Equal(1, s.MaxMp);
            s.Hp = 2;
            Assert.Equal(2, s.Hp);
            Assert.Equal(2, s.MaxHp);
            s.Mp = 2;
            Assert.Equal(2, s.Mp);
            Assert.Equal(2, s.MaxMp);
            s.MaxHp = 1;
            Assert.Equal(1, s.Hp);
            Assert.Equal(1, s.MaxHp);
            s.MaxMp = 1;
            Assert.Equal(1, s.Mp);
            Assert.Equal(1, s.MaxMp);
        }

        // Party의 초기 상태
        [Fact]
        public void InitialParty()
        {
            var p = new Party();
            Assert.Null(p.Context);
            Assert.Empty(p.ShipList);
        }

        // SingleAttack 및 그에 따른 Ship 파괴
        [Fact]
        public void BasicSingleAttack()
        {
            var c = new Context();

            Assert.Equal(ContextState.Empty, c.State);

            var s1 = new Ship();
            var s2 = new Ship();

            Assert.Equal(1, s1.Hp);
            Assert.Equal(1, s1.Mp);
            Assert.Equal(ShipState.Alive, s1.State);

            Assert.Equal(1, s2.Hp);
            Assert.Equal(1, s2.Mp);
            Assert.Equal(ShipState.Alive, s2.State);

            var p1 = new Party();
            var p2 = new Party();

            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);

            c.SetCurrentTurnParty(p1);

            Assert.Equal(ContextState.Ready, c.State);

            Assert.Equal(s1, c.CurrentTurnShip);

            var commandList = s1.CommandList;

            Assert.Single(commandList);

            var firstCommand = commandList[0];

            Assert.Equal(s1, firstCommand.Source);
            Assert.Equal(BattleCommandType.SingleAttack, firstCommand.CommandType);

            firstCommand.Target = s2;

            var commandResult = c.ExecuteCommand(firstCommand);
            var deltaList = commandResult.DeltaList.GetEnumerator();
            deltaList.MoveNext();

            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
            Assert.Equal(s1, deltaList.Current.Source);
            Assert.Equal(s2, deltaList.Current.Target);
            deltaList.MoveNext();

            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
            Assert.Equal(s1, deltaList.Current.Source);
            Assert.Equal(s2, deltaList.Current.Target);
            Assert.Equal(-1, deltaList.Current.Value);
            deltaList.MoveNext();

            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.Destroyed, deltaList.Current.DeltaType);
            Assert.Equal(s1, deltaList.Current.Source);
            Assert.Equal(s2, deltaList.Current.Target);
            deltaList.MoveNext();

            Assert.Null(deltaList.Current);
            deltaList.Dispose();

            Assert.Equal(ContextState.Finished, c.State);
            Assert.Equal(p1, c.WinParty);
        }

        // 모든 Ship의 기본 상태에서 첫 번째 BattleCommand의 타입은 SingleAttack이다.
        [Fact]
        public void DefaultShipCommand()
        {
            var s = new Ship();
            var commandList = s.CommandList;
            Assert.Single(commandList);
            var command = commandList[0];
            Assert.Equal(BattleCommandType.SingleAttack, command.CommandType);
        }

        [Fact]
        public void LearnHealCommand()
        {
            var s = new Ship();
            var cmd = new Heal();
            s.LearnCommand(cmd);

            var commandList = s.CommandList;
            Assert.Equal(2, commandList.Count);
            Assert.Equal(BattleCommandType.SingleAttack, commandList[0].CommandType);
            Assert.Equal(BattleCommandType.Heal, commandList[1].CommandType);
        }

        [Fact]
        public void LearnDoubleHpCommand()
        {
            var s = new Ship();
            var cmd = new DoubleHp(null);
            s.LearnCommand(cmd);

            var commandList = s.CommandList;
            Assert.Equal(2, commandList.Count);
            Assert.Equal(BattleCommandType.SingleAttack, commandList[0].CommandType);
            Assert.Equal(BattleCommandType.DoubleHp, commandList[1].CommandType);
        }

        // 같은 BattleCommand를 두 번 배울 수는 없다.
        [Fact]
        public void DuplicatedLearn()
        {
            var s = new Ship();
            var cmd = new DoubleHp(null);
            s.LearnCommand(cmd);
            Assert.Throws<DuplicatedRelationException>(() => s.LearnCommand(cmd));
        }

        // BattleCommandType이 같은 서로 다른 BattleCommand를 두 번 배울 수는 없다.
        [Fact]
        public void ConflictedBattleCommandTypeLearn()
        {
            var s = new Ship();
            var cmd = new DoubleHp(null);
            s.LearnCommand(cmd);
            var cmd2 = new DoubleHp(null);
            Assert.Throws<ConflictedBattleCommandTypeException>(() => s.LearnCommand(cmd2));
        }

        // 이미 Ship이 배운 BattleCommand는 다른 Ship이 배울 수 없다.
        [Fact]
        public void ConflictedLearn()
        {
            var s1 = new Ship();
            var cmd = new DoubleHp(null);
            s1.LearnCommand(cmd);

            var s2 = new Ship();
            Assert.Throws<ConflictedRelationException>(() => s2.LearnCommand(cmd));
        }

        [Fact]
        public void BasicDoubleHp()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            Assert.Equal(s1, c.CurrentTurnShip);

            s1.LearnCommand(new DoubleHp(null));
            var cmd1 = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.DoubleHp);
            Assert.NotNull(cmd1);
            cmd1.Target = s1;

            using (var deltaList = c.ExecuteCommand(cmd1).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Target);
                Assert.Equal(1, deltaList.Current.Value);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.TurnChanged, deltaList.Current.DeltaType);
                Assert.Equal(s2, deltaList.Current.Source);
                Assert.Equal(s2, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }

            // 아직 승부 나지 않았다.
            Assert.Equal(ContextState.Ready, c.State);
            Assert.Null(c.WinParty);
            Assert.Equal(s2, c.CurrentTurnShip);

            var cmd2 = s2.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.SingleAttack);
            Assert.NotNull(cmd2);
            cmd2.Target = s1;

            using (var deltaList = c.ExecuteCommand(cmd2).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
                Assert.Equal(s2, deltaList.Current.Source);
                Assert.Equal(s1, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s2, deltaList.Current.Source);
                Assert.Equal(s1, deltaList.Current.Target);
                Assert.Equal(-1, deltaList.Current.Value);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.TurnChanged, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s1, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }

            // 아직 승부 나지 않았다.
            Assert.Equal(ContextState.Ready, c.State);
            Assert.Null(c.WinParty);
            Assert.Equal(s1, c.CurrentTurnShip);

            var cmd3 = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.SingleAttack);
            Assert.NotNull(cmd3);
            cmd3.Target = s2;

            using (var deltaList = c.ExecuteCommand(cmd3).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2, deltaList.Current.Target);
                Assert.Equal(-1, deltaList.Current.Value);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Destroyed, deltaList.Current.DeltaType);
                Assert.Equal(s2, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }

            // 승부가 났다!
            Assert.Equal(ContextState.Finished, c.State);
            Assert.Equal(p1, c.WinParty);
            Assert.Null(c.CurrentTurnShip);
        }

        // 특정 Party를 턴으로 지정했을 때, 해당 Party의 첫 번째 Ship의 턴 상태가 된다.
        [Fact]
        public void SetTurnRule()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            Assert.Equal(s1, c.CurrentTurnShip);
        }

        // UndefinedTurn 상태가 아닐 때 턴을 지정할 수는 없다.
        [Fact]
        public void SetTurnRuleNotReady()
        {
            var c = new Context();
            var p1 = new Party();
            Assert.Throws<InvalidBattleStateException>(() => c.SetCurrentTurnParty(p1));
        }

        // 특정 Ship 다음 행동이 가능한 Ship을 반환하는 NextShip 기능을 테스트한다.
        [Fact]
        public void NextShip()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            Assert.Equal(s2, s1.NextShip);
            Assert.Equal(s1, s2.NextShip);
        }

        // 특정 Ship 다음 행동이 가능한 Ship을 반환하는 NextShip 기능을 테스트한다.
        [Fact]
        public void NextShip2()
        {
            var c = new Context();
            var s1A = new Ship();
            var s1B = new Ship();
            var s2A = new Ship();
            var s2B = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1A);
            p1.AddShip(s1B);
            p2.AddShip(s2A);
            p2.AddShip(s2B);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            Assert.Equal(s1B, s1A.NextShip);
            Assert.Equal(s2A, s1B.NextShip);
            Assert.Equal(s2B, s2A.NextShip);
            Assert.Equal(s1A, s2B.NextShip);
        }

        // 특정 Ship 다음 행동이 가능한 Ship을 반환하는 NextShip 기능을 테스트한다.
        // HP = 0인 Ship은 스킵해야 한다.
        [Fact]
        public void NextShip3()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            s2.Hp = 0;
            Assert.Equal(s1, s1.NextShip);
        }

        [Fact]
        public void SkipTurn()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            Assert.Equal(s1, c.CurrentTurnShip);
            var cmd = new SkipTurn(s1);

            var cmdResult = c.ExecuteCommand(cmd);
            var deltaList2 = cmdResult.DeltaList;
            Assert.Single(deltaList2);
            Assert.Equal(DeltaType.TurnChanged, deltaList2[0].DeltaType);
            Assert.Equal(s2, deltaList2[0].Source);
            Assert.Equal(s2, deltaList2[0].Target);

            Assert.Equal(ContextState.Ready, c.State);
            Assert.Null(c.WinParty);
            Assert.Equal(s2, c.CurrentTurnShip);
        }

        // Context, Party, Ship의 상호 참조 관계를 확인한다.
        [Fact]
        public void BasicRelation()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);

            // Party <-> Ship
            Assert.Contains(s1, p1.ShipList);
            Assert.Contains(s2, p2.ShipList);
            Assert.Equal(p1, s1.Party);
            Assert.Equal(p2, s2.Party);

            // Party, Ship -> Context
            Assert.Equal(c, s1.Context);
            Assert.Equal(c, s2.Context);
            Assert.Equal(c, p1.Context);
            Assert.Equal(c, p2.Context);

            // Context -> Party
            Assert.Contains(p1, c.PartyList);
            Assert.Contains(p2, c.PartyList);
        }

        // Party에 Ship을 두 번 넣거나, Context에 Party를 두 번 넣을 수 없다.
        [Fact]
        public void DuplicatedRelation()
        {
            var c = new Context();
            var s = new Ship();
            var p = new Party();
            p.AddShip(s);
            Assert.Throws<DuplicatedRelationException>(() => p.AddShip(s));
            c.AddParty(p);
            Assert.Throws<DuplicatedRelationException>(() => c.AddParty(p));
        }

        // Party에 속한 Ship은 다른 Party에 속할 수 없다.
        // Context에 속한 Party는 다른 Context에 속할 수 없다.
        [Fact]
        public void ConflictedRelation()
        {
            var c = new Context();
            var s = new Ship();
            var p = new Party();
            p.AddShip(s);
            c.AddParty(p);

            var p2 = new Party();
            Assert.Throws<ConflictedRelationException>(() => p2.AddShip(s));

            var c2 = new Context();
            Assert.Throws<ConflictedRelationException>(() => c2.AddParty(p));
        }

        [Fact]
        public void ContextStateCheck()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();

            // 텅텅 빈 상태
            Assert.Equal(ContextState.Empty, c.State);

            // 파티 하나만 있는 상태
            c.AddParty(p1);
            Assert.Equal(ContextState.SingleParty, c.State);

            // 파티는 둘 이상 있으나 빈 파티가 하나 이상 있는 상태
            c.AddParty(p2);
            Assert.Equal(ContextState.PartyEmpty, c.State);

            // 파티는 둘 이상 있으나 빈 파티가 하나 이상 있는 상태 
            p1.AddShip(s1);
            Assert.Equal(ContextState.PartyEmpty, c.State);

            // 채워진 파티 둘 이상 있지만 현재 턴 미지정 상태
            p2.AddShip(s2);
            Assert.Equal(ContextState.UndefinedTurn, c.State);

            // BattleCommand 실행할 수 있는 상태
            c.SetCurrentTurnParty(p1);
            Assert.Equal(ContextState.Ready, c.State);
        }

        // 턴이 돌아온 Party에 속한 모든 Ship이 한 번씩 BattleCommand를 실행할 수 있다.
        [Fact]
        public void BasicTurnOrder()
        {
            var c = new Context();
            var s1A = new Ship();
            var s1B = new Ship();
            var s2A = new Ship();
            var s2B = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1A);
            p1.AddShip(s1B);
            p2.AddShip(s2A);
            p2.AddShip(s2B);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);

            Assert.Equal(s1A, c.CurrentTurnShip);
            Assert.Equal(p1, c.CurrentTurnShip.Party);
            Assert.True(c.ExecuteCommand(new SkipTurn(s1A)).Valid);

            Assert.Equal(s1B, c.CurrentTurnShip);
            Assert.Equal(p1, c.CurrentTurnShip.Party);
            Assert.True(c.ExecuteCommand(new SkipTurn(s1B)).Valid);

            Assert.Equal(s2A, c.CurrentTurnShip);
            Assert.Equal(p2, c.CurrentTurnShip.Party);
            Assert.True(c.ExecuteCommand(new SkipTurn(s2A)).Valid);

            Assert.Equal(s2B, c.CurrentTurnShip);
            Assert.Equal(p2, c.CurrentTurnShip.Party);
            Assert.True(c.ExecuteCommand(new SkipTurn(s2B)).Valid);
        }

        // 정밀 조준 시에는 그 다음 턴에 공격력이 두 배가 된다.
        [Fact]
        public void Precision()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            s1.LearnCommand(new Precision());
            var cmd = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.Precision);
            Assert.NotNull(cmd);
            using (var deltaList = c.ExecuteCommand(cmd).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Precision, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s1, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.TurnChanged, deltaList.Current.DeltaType);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }

            Assert.True(c.ExecuteCommand(new DoubleHp(s2)).Valid);
            var cmd2 = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.SingleAttack);
            Assert.NotNull(cmd2);
            cmd2.Target = s2;
            using (var deltaList = c.ExecuteCommand(cmd2).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2, deltaList.Current.Target);
                Assert.Equal(-2, deltaList.Current.Value);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Destroyed, deltaList.Current.DeltaType);
                Assert.Equal(s2, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }
        }

        // 전체 공격 기본
        [Fact]
        public void Bombard()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2A = new Ship();
            var s2B = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2A);
            p2.AddShip(s2B);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            s1.LearnCommand(new Bombard());
            var cmd = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.Bombard);
            Assert.NotNull(cmd);
            cmd.Target = s2A;
            using (var deltaList = c.ExecuteCommand(cmd).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2A, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2B, deltaList.Current.Target);
                deltaList.MoveNext();
                
                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2A, deltaList.Current.Target);
                Assert.Equal(-1, deltaList.Current.Value);
                deltaList.MoveNext();
                
                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s2B, deltaList.Current.Target);
                Assert.Equal(-1, deltaList.Current.Value);
                deltaList.MoveNext();
                
                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Destroyed, deltaList.Current.DeltaType);
                Assert.Equal(s2A, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Destroyed, deltaList.Current.DeltaType);
                Assert.Equal(s2B, deltaList.Current.Target);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }
        }

        // MP 사용해서 스스로를 Heal한다.
        [Fact]
        public void Heal()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();

            s1.Hp = s1.MaxHp = 2;
            s2.Hp = s2.MaxHp = 2;

            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);
            s1.LearnCommand(new Heal());
            var s1HealCmd = s1.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.Heal);
            Assert.NotNull(s1HealCmd);

            Assert.Equal(1, s1.Mp);

            using (var deltaList = c.ExecuteCommand(s1HealCmd).DeltaList.GetEnumerator())
            {
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.Heal, deltaList.Current.DeltaType);
                Assert.Equal(s1, deltaList.Current.Source);
                Assert.Equal(s1, deltaList.Current.Target);
                deltaList.MoveNext();

                AssertDeltaSelfValue(s1, DeltaType.Mp, -1, deltaList.Current);
                deltaList.MoveNext();

                AssertDeltaSelfValue(s1, DeltaType.Hp, 0, deltaList.Current);
                deltaList.MoveNext();

                Assert.NotNull(deltaList.Current);
                Assert.Equal(DeltaType.TurnChanged, deltaList.Current.DeltaType);
                deltaList.MoveNext();

                Assert.Null(deltaList.Current);
            }

            Assert.Equal(2, s1.Hp);
            Assert.Equal(0, s1.Mp);

            var cmd2 = s2.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.SingleAttack);
            Assert.NotNull(cmd2);
            cmd2.Target = s1;
            c.ExecuteCommand(cmd2);

            Assert.Equal(1, s1.Hp);
            Assert.Equal(0, s1.Mp);

            Assert.Throws<NotEnoughMpException>(() => c.ExecuteCommand(s1HealCmd));
        }

        static void AssertDeltaSelfValue(Ship s, DeltaType deltaType, int deltaValue, Delta delta)
        {
            Assert.NotNull(delta);
            Assert.Equal(deltaType, delta.DeltaType);
            Assert.Equal(s, delta.Source);
            Assert.Equal(s, delta.Target);
            Assert.Equal(deltaValue, delta.Value);
        }

        [Fact]
        public void Basic4vs4()
        {
            var c = new Context();
            var s1a = new Ship();
            var s1b = new Ship();
            var s1c = new Ship();
            var s1d = new Ship();
            var s2a = new Ship();
            var s2b = new Ship();
            var s2c = new Ship();
            var s2d = new Ship();

            var p1 = new Party();
            p1.AddShip(s1a);
            p1.AddShip(s1b);
            p1.AddShip(s1c);
            p1.AddShip(s1d);

            var p2 = new Party();
            p2.AddShip(s2a);
            p2.AddShip(s2b);
            p2.AddShip(s2c);
            p2.AddShip(s2d);

            c.AddParty(p1);
            c.AddParty(p2);

            c.SetCurrentTurnParty(p1);

            ExecuteSingleAttack(c, s1a, s2a);
            ExecuteSingleAttack(c, s1b, s2b);
            ExecuteSingleAttack(c, s1c, s2c);
            ExecuteSingleAttack(c, s1d, s2d);

            Assert.Equal(ContextState.Finished, c.State);
            Assert.Equal(p1, c.WinParty);
        }

        static void ExecuteSingleAttack(Context c, Ship attackSourceShip, Ship attackTargetShip)
        {
            var cmd = attackSourceShip.CommandList.FirstOrDefault(e => e.CommandType == BattleCommandType.SingleAttack);
            Assert.NotNull(cmd);
            cmd.Target = attackTargetShip;
            c.ExecuteCommand(cmd);
        }

        [Fact]
        public void HealIsSelfTargetType()
        {
            Assert.Equal(TargetType.Self, new Heal().TargetType);
        }
        
        // 봇이 조종하는 파티 (봇 파티)
        // 항상 SingleAttack만 함
        // 1:1
        [Fact]
        public void BasicBot()
        {
            var c = new Context();
            var s1 = new Ship();
            var s2 = new Ship();
            var p1 = new Party();
            var p2 = new Party();
            p1.AddShip(s1);
            p2.AddShip(s2);
            c.AddParty(p1);
            c.AddParty(p2);
            c.SetCurrentTurnParty(p1);

            // 상대방 파티는 봇이다.
            p2.IsBot = true;

            // 서로 한번씩 공격하는 것을 봐야하니 바로 죽지 않도록 HP 증가시켜준다.
            s1.Hp = 2;
            s2.Hp = 2;

            var commandList = s1.CommandList;

            var firstCommand = commandList[0];

            firstCommand.Target = s2;

            var commandResult = c.ExecuteCommand(firstCommand);
            var deltaList = commandResult.DeltaList.GetEnumerator();
            deltaList.MoveNext();

            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.Fire, deltaList.Current.DeltaType);
            Assert.Equal(s1, deltaList.Current.Source);
            Assert.Equal(s2, deltaList.Current.Target);
            deltaList.MoveNext();

            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.Hp, deltaList.Current.DeltaType);
            Assert.Equal(s1, deltaList.Current.Source);
            Assert.Equal(s2, deltaList.Current.Target);
            Assert.Equal(-1, deltaList.Current.Value);
            deltaList.MoveNext();
            
            Assert.NotNull(deltaList.Current);
            Assert.Equal(DeltaType.TurnChanged, deltaList.Current.DeltaType);
            deltaList.MoveNext();
            
            Assert.Null(deltaList.Current);
            deltaList.Dispose();
            
            // s1 -> s2 공격한 후 상태 (플레이어가 공격)
            Assert.Equal(2, s1.Hp);
            Assert.Equal(1, s2.Hp);
            
            // 봇 차례이다.
            Assert.True(c.CurrentTurnShip.Party.IsBot);
            
            // 봇이 행동할 차례에는 행동이 내부적으로 결정되므로 null 넘긴다.
            var commandResult2 = c.ExecuteCommand(null);
            var deltaList2 = commandResult2.DeltaList.GetEnumerator();
            deltaList2.MoveNext();
            
            Assert.NotNull(deltaList2.Current);
            Assert.Equal(DeltaType.Fire, deltaList2.Current.DeltaType);
            Assert.Equal(s2, deltaList2.Current.Source);
            Assert.Equal(s1, deltaList2.Current.Target);
            deltaList2.MoveNext();

            Assert.NotNull(deltaList2.Current);
            Assert.Equal(DeltaType.Hp, deltaList2.Current.DeltaType);
            Assert.Equal(s2, deltaList2.Current.Source);
            Assert.Equal(s1, deltaList2.Current.Target);
            Assert.Equal(-1, deltaList2.Current.Value);
            deltaList2.MoveNext();
            
            Assert.NotNull(deltaList2.Current);
            Assert.Equal(DeltaType.TurnChanged, deltaList2.Current.DeltaType);
            deltaList2.MoveNext();

            Assert.Null(deltaList2.Current);
            deltaList2.Dispose();
            
            // s2 -> s1 공격한 후 상태 (봇이 공격)
            Assert.Equal(1, s1.Hp);
            Assert.Equal(1, s2.Hp);
        }
    }
}