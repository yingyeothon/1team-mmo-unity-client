using System;
using TMPro;
using UnityEngine;

public class TooltipView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text1;

    [SerializeField]
    TextMeshProUGUI text2;
    
    public string Text1
    {
        set => text1.text = value;
    }

    public string Text2
    {
        set => text2.text = value;
    }

    void Awake()
    {
        Text1 = string.Empty;
        Text2 = String.Empty;
    }
}
