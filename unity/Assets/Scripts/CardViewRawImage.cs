using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardViewRawImage : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    CardView cardView;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        cardView.OnPointerClickRawImage();
    }
}
