using BattleLib;
using TMPro;
using UnityEngine;

public class CommandButton : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI commandText;

    [SerializeField]
    TextMeshProUGUI costText;

    BattleSimulator battleSimulator;
    IBattleCommand command;
    
    string CommandText
    {
        set => commandText.text = value;
    }

    string CostText
    {
        set => costText.text = value;
    }

    public async void OnClick()
    {
        await battleSimulator.ExecuteBattleCommandAsync(command);
    }

    public void SetCommand(BattleSimulator sim, IBattleCommand cmd)
    {
        battleSimulator = sim;
        command = cmd;
        
        CommandText = cmd.CommandType.ToString();
        CostText = "-";
    }
}