using System.Collections.Generic;

namespace BattleLib {

    public class ServerContextFacade {
        private Context context = new Context();

        public NetworkCommandResult ExecuteCommand(NetworkBattleCommand nCmd) {
            var cmd = ConvertCommand(nCmd);
            var result = context.ExecuteCommand(cmd);
            return ConvertResult(result);
        }

        private IBattleCommand ConvertCommand(NetworkBattleCommand nCmd) {
            switch (nCmd.CommandType) {
                case BattleCommandType.Bombard:
                    return CopyProperties(nCmd, new Bombard());
                case BattleCommandType.SingleAttack:
                    return CopyProperties(nCmd, new SingleAttack());
                case BattleCommandType.Heal:
                    return CopyProperties(nCmd, new Heal());
                case BattleCommandType.DoubleHp:
                    return CopyProperties(nCmd, new DoubleHp());
                case BattleCommandType.Precision:
                    return CopyProperties(nCmd, new Precision());
                case BattleCommandType.SkipTurn:
                    return CopyProperties(nCmd, new SkipTurn());
                default:
                    throw new System.ArgumentException();
            }
        }

        private NetworkCommandResult ConvertResult(CommandResult result) {
            return new NetworkCommandResult {
                Valid = result.Valid,
                DeltaList = ConvertDeltaList(result.DeltaList),
            };
        }

        private IList<NetworkDelta> ConvertDeltaList(IList<Delta> deltaList) {
            var nDeltaList = new List<NetworkDelta>();
            foreach (var delta in deltaList)
            {
                
            }
            return nDeltaList;
        }

        private NetworkDelta ConvertDelta(Delta delta) {
            return new NetworkDelta();
        }

        private IBattleCommand CopyProperties(NetworkBattleCommand nCmd, IBattleCommand cmd) {
            cmd.TargetType = nCmd.TargetType;
            cmd.Source = context.FindShipById(nCmd.Source);
            cmd.Target = context.FindShipById(nCmd.Target);
            return cmd;
        }
    }
}
