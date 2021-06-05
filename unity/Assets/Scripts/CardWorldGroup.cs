#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CardWorldGroup : MonoBehaviour
{
    [SerializeField]
    CardWorld[] cardWorldList;

    [SerializeField]
    RenderTexture[] renderTextureList;

    [ContextMenu("Update")]
    void UpdateCardWorldList()
    {
        if (this == null) return;
        
        cardWorldList = GetComponentsInChildren<CardWorld>();
        for (var i = 0; i < cardWorldList.Length; i++)
        {
            cardWorldList[i].WorldNumber = i + 1;
            cardWorldList[i].RenderTexture = renderTextureList[i];
        }
    }

#if UNITY_EDITOR
    void OnTransformChildrenChanged()
    {
        if (Application.isPlaying) return;
        
        EditorApplication.delayCall += UpdateCardWorldList;
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        EditorApplication.delayCall += UpdateCardWorldList;
    }
#endif
    
    public ShipPivot GetRandomShip(ShipPivot exceptThis)
    {
        var candidateList = cardWorldList.Where(e => e.IsShipDestroyed == false && e.ShipPivot != exceptThis).ToList();
        return candidateList.Count > 0 ? candidateList[Random.Range(0, candidateList.Count)].ShipPivot : null;
    }
}
