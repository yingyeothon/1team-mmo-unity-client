namespace BattleLib
{
    public class ClientContext : IContext
    {
        public CommandResult ExecuteCommand(IBattleCommand cmd)
        {
            var ncmd = new NetworkBattleCommand
            {
                TargetType = cmd.TargetType,
                CommandType = cmd.CommandType,
                Target = cmd.Target.Id,
                Source = cmd.Source.Id,
            };

            var nresult = Call(ncmd);

            return null;
        }

        // TODO change to real network client
        private ServerContextFacade serverContextFacade = new ServerContextFacade();

        private NetworkCommandResult Call(NetworkBattleCommand ncmd)
        {
            return serverContextFacade.ExecuteCommand(ncmd);
        }
    }
}
