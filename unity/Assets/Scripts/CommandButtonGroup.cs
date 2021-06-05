using BattleLib;
using UnityEngine;

public class CommandButtonGroup : MonoBehaviour
{
    [SerializeField]
    GameObject commandButtonPrefab;

    [SerializeField]
    BattleSimulator battleSimulator;

    public void UpdateCommandList(Ship s)
    {
        transform.DestroyAllChildren();
        
        if (s == null) return;
        
        foreach (var cmd in s.CommandList)
        {
            var cmdBtn = Instantiate(commandButtonPrefab, transform).GetComponent<CommandButton>();
            cmdBtn.SetCommand(battleSimulator, cmd);
        }
    }
}