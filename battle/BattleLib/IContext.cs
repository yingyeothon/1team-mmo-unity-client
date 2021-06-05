namespace BattleLib
{
    public interface IContext
    {
        CommandResult ExecuteCommand(IBattleCommand cmd);
    }
}