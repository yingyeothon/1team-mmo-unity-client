using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text1;

    [SerializeField]
    TextMeshProUGUI text2;

    [SerializeField]
    Image frameImage;

    [SerializeField]
    Image glowImage;

    [SerializeField]
    Image selectionOutlineImage;

    [SerializeField]
    TextMeshProUGUI hpText;

    [SerializeField]
    TextMeshProUGUI mpText;

    [SerializeField]
    Image hpSlider;

    [SerializeField]
    Image mpSlider;

    [SerializeField]
    Color frameColor;

    [SerializeField]
    Color fontColor;

    [SerializeField]
    BattleSimulator battleSimulator;

    public string Text1
    {
        set => text1.text = value;
    }

    public string Text2
    {
        set => text2.text = value;
    }
    
    public string HpText
    {
        set => hpText.text = value;
    }

    public string MpText
    {
        set => mpText.text = value;
    }

    public float HpRatio
    {
        set => hpSlider.fillAmount = value;
    }

    public float MpRatio
    {
        set => mpSlider.fillAmount = value;
    }

    void OnValidate()
    {
        frameImage.color = glowImage.color = frameColor;
        text1.color = text2.color = fontColor;
    }

    public bool Glow
    {
        set => glowImage.enabled = value;
    }

    public bool SelectionOutline
    {
        get => selectionOutlineImage.enabled;
        set => selectionOutlineImage.enabled = value;
    }

    public void OnPointerClickRawImage()
    {
        if (SelectionOutline)
        {
            battleSimulator.OnShipClick(this);
        }
    }

    void Awake()
    {
        SelectionOutline = false;
        Glow = false;
    }
}