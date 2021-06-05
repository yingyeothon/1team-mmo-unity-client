using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleLib;
using UnityEngine;

public class BattleSimulator : MonoBehaviour
{
    [SerializeField]
    CardWorld[] cardWorldList;

    [SerializeField]
    CardView[] cardViewList;

    [SerializeField]
    TooltipView tooltipView;

    [SerializeField]
    CommandButtonGroup commandButtonGroup;

    readonly Context context = new Context();
    readonly Party party1 = new Party();
    readonly Party party2 = new Party();

    readonly List<Ship> shipList1 = new List<Ship>();
    readonly List<Ship> shipList2 = new List<Ship>();

    readonly Dictionary<Ship, CardWorld> ship2CardWorldDict = new Dictionary<Ship, CardWorld>();
    readonly Dictionary<Ship, CardView> ship2CardViewDict = new Dictionary<Ship, CardView>();
    readonly Dictionary<CardView, Ship> cardView2ShipDict = new Dictionary<CardView, Ship>();

    bool busy;
    TaskCompletionSource<Ship> shipSelectionTsc;

    void Start()
    {
        InitContext();
    }

    void InitContext()
    {
        if (context.State != ContextState.Empty)
        {
            Debug.LogError("Context is not empty.");
            return;
        }

        shipList1.Add(new Ship());
        shipList1.Add(new Ship());
        shipList1.Add(new Ship());
        shipList1.Add(new Ship());

        shipList2.Add(new Ship());
        shipList2.Add(new Ship());
        shipList2.Add(new Ship());
        shipList2.Add(new Ship());

        ship2CardWorldDict[shipList1[0]] = cardWorldList[0];
        ship2CardWorldDict[shipList1[1]] = cardWorldList[1];
        ship2CardWorldDict[shipList1[2]] = cardWorldList[2];
        ship2CardWorldDict[shipList1[3]] = cardWorldList[3];
        ship2CardWorldDict[shipList2[0]] = cardWorldList[4];
        ship2CardWorldDict[shipList2[1]] = cardWorldList[5];
        ship2CardWorldDict[shipList2[2]] = cardWorldList[6];
        ship2CardWorldDict[shipList2[3]] = cardWorldList[7];

        ship2CardViewDict[shipList1[0]] = cardViewList[0];
        ship2CardViewDict[shipList1[1]] = cardViewList[1];
        ship2CardViewDict[shipList1[2]] = cardViewList[2];
        ship2CardViewDict[shipList1[3]] = cardViewList[3];
        ship2CardViewDict[shipList2[0]] = cardViewList[4];
        ship2CardViewDict[shipList2[1]] = cardViewList[5];
        ship2CardViewDict[shipList2[2]] = cardViewList[6];
        ship2CardViewDict[shipList2[3]] = cardViewList[7];

        cardView2ShipDict[cardViewList[0]] = shipList1[0];
        cardView2ShipDict[cardViewList[1]] = shipList1[1];
        cardView2ShipDict[cardViewList[2]] = shipList1[2];
        cardView2ShipDict[cardViewList[3]] = shipList1[3];
        cardView2ShipDict[cardViewList[4]] = shipList2[0];
        cardView2ShipDict[cardViewList[5]] = shipList2[1];
        cardView2ShipDict[cardViewList[6]] = shipList2[2];
        cardView2ShipDict[cardViewList[7]] = shipList2[3];

        shipList1[0].LearnCommand(new Heal());
        shipList1[0].LearnCommand(new Bombard());
        shipList1[0].MaxHp = 5;
        shipList1[0].Hp = 1;
        
        shipList2[0].Hp = 2;
        shipList2[1].Hp = 2;
        shipList2[2].Hp = 2;
        shipList2[3].Hp = 2;

        party2.IsBot = true;

        foreach (var s in shipList1)
        {
            party1.AddShip(s);

            UpdateHpMp(ship2CardViewDict[s], s);

            ship2CardViewDict[s].Glow = false;
        }

        foreach (var s in shipList2)
        {
            party2.AddShip(s);

            UpdateHpMp(ship2CardViewDict[s], s);

            ship2CardViewDict[s].Glow = false;
        }

        context.AddParty(party1);
        context.AddParty(party2);

        context.SetCurrentTurnParty(party1);
        ship2CardViewDict[context.CurrentTurnShip].Glow = true;

        commandButtonGroup.UpdateCommandList(context.CurrentTurnShip);

        Debug.Log(context.State);
    }

    public async Task ExecuteBattleCommandAsync(IBattleCommand cmd)
    {
        if (busy) return;

        commandButtonGroup.gameObject.SetActive(false);

        if (context.CurrentTurnShip.Party.IsBot == false
            && cmd.TargetType == TargetType.Single)
        {
            tooltipView.Text1 = "목표물 선택";
            tooltipView.Text2 = "커맨드의 목표물을 선택하세요.";

            var selectedShip = await WaitForShipSelection();
            cmd.Target = selectedShip;
        }

        busy = true;
        await ExecuteBattleCommandInternalAsync(cmd);
        busy = false;
        commandButtonGroup.gameObject.SetActive(context.State == ContextState.Ready);

        commandButtonGroup.UpdateCommandList(context.CurrentTurnShip);

        // 다음 턴에 봇이라면 바로 행동을 해 준다.
        if (context.State == ContextState.Ready && context.CurrentTurnShip.Party.IsBot)
        {
            await ExecuteBattleCommandAsync(null);
        }
    }

    async Task<Ship> WaitForShipSelection()
    {
        if (shipSelectionTsc != null)
        {
            Debug.LogError("Overlapped ship selection request");
            return null;
        }

        foreach (var cardView in cardViewList)
        {
            var s = cardView2ShipDict[cardView];
            cardView.SelectionOutline = s.Party != party1 && s.Hp > 0;
        }

        shipSelectionTsc = new TaskCompletionSource<Ship>();
        var selectedShip = await shipSelectionTsc.Task;
        shipSelectionTsc = null;

        foreach (var cardView in cardViewList)
        {
            cardView.SelectionOutline = false;
        }

        return selectedShip;
    }

    async Task ExecuteBattleCommandInternalAsync(IBattleCommand cmd)
    {
        var oldTurnShip = context.CurrentTurnShip;
        var cmdResult = context.ExecuteCommand(cmd);
        for (var index = 0; index < cmdResult.DeltaList.Count; index++)
        {
            var delta = cmdResult.DeltaList[index];

            Debug.Log(delta.DeltaType);

            var sourceShipPivot = ship2CardWorldDict[delta.Source].ShipPivot;
            var targetShipPivot = ship2CardWorldDict[delta.Target].ShipPivot;

            switch (delta.DeltaType)
            {
                case DeltaType.Fire:
                    tooltipView.Text1 = "일반 공격";
                    tooltipView.Text2 = "";

                    sourceShipPivot.LookAt(targetShipPivot);
                    var laser = sourceShipPivot.Fire();
                    ship2CardViewDict[delta.Target].Glow = true;

                    if (index < cmdResult.DeltaList.Count - 1
                        && cmdResult.DeltaList[index + 1].DeltaType == delta.DeltaType)
                    {
                    }
                    else
                    {
                        while (laser != null && laser.gameObject != null)
                        {
                            await Task.Yield();
                        }
                    }

                    break;
                case DeltaType.Hp:
                    if (delta.Value < 0)
                    {
                        tooltipView.Text1 = "목표물 HP 감소";
                        tooltipView.Text2 = $"목표물의 HP가 {Mathf.Abs(delta.Value)} 감소했습니다.";
                    }

                    UpdateHpMp(ship2CardViewDict[delta.Target], delta.Target);

                    if (index < cmdResult.DeltaList.Count - 1
                        && cmdResult.DeltaList[index + 1].DeltaType == delta.DeltaType)
                    {
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }

                    break;
                case DeltaType.Destroyed:
                    tooltipView.Text1 = "파괴";
                    tooltipView.Text2 = "목표물이 파괴되었습니다.";
                    ship2CardViewDict[delta.Target].Glow = false;
                    targetShipPivot.DestroyWithExplosion();

                    if (index < cmdResult.DeltaList.Count - 1
                        && cmdResult.DeltaList[index + 1].DeltaType == delta.DeltaType)
                    {
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }

                    break;
                case DeltaType.TurnChanged:
                    foreach (var cardView in cardViewList)
                    {
                        cardView.Glow = false;
                    }
                    ship2CardViewDict[context.CurrentTurnShip].Glow = true;
                    tooltipView.Text1 = "턴 변경";
                    tooltipView.Text2 = "턴이 변경됐습니다.";

                    break;
                case DeltaType.Precision:
                    break;
                case DeltaType.Heal:
                    targetShipPivot.InstantiateHealEffect();
                    break;
                case DeltaType.Mp:
                    UpdateHpMp(ship2CardViewDict[delta.Source], delta.Source);
                    UpdateHpMp(ship2CardViewDict[delta.Target], delta.Target);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (context.State == ContextState.Finished)
        {
            ship2CardViewDict[oldTurnShip].Glow = false;
            tooltipView.Text1 = "전투 종료";
            tooltipView.Text2 = context.WinParty == party1 ? "플레이어가 승리했습니다!" : "플레이어가 패배했습니다.";
        }
    }

    static void UpdateHpMp(CardView cardView, Ship ship)
    {
        cardView.HpText = $"{ship.Hp}/{ship.MaxHp}";
        if (ship.MaxHp > 0)
        {
            cardView.HpRatio = (float) ship.Hp / ship.MaxHp;
        }
        else
        {
            cardView.HpRatio = 0;
        }

        cardView.MpText = $"{ship.Mp}/{ship.MaxMp}";
        if (ship.MaxMp > 0)
        {
            cardView.MpRatio = (float) ship.Mp / ship.MaxMp;
        }
        else
        {
            cardView.MpRatio = 0;
        }
    }

    public void OnShipClick(CardView cardView)
    {
        shipSelectionTsc?.SetResult(cardView2ShipDict[cardView]);
    }
}