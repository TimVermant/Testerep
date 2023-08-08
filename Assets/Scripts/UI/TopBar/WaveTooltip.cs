using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WaveTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Debug.Log(pointerEventData.position);
       // FindObjectOfType<TimelineIndicatorManager>().ShowWaveTooltip();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {


    }
}
