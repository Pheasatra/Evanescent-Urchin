using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//-----------------------------------------------------------------------

public class ButtonAllClick : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent leftClick;
    public UnityEvent middleClick;
    public UnityEvent rightClick;

    //-----------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            // Left click
            case PointerEventData.InputButton.Left:
                leftClick.Invoke();
                break;

            // Middle click
            case PointerEventData.InputButton.Middle:
                middleClick.Invoke();
                break;

            // Right click
            case PointerEventData.InputButton.Right:
                rightClick.Invoke();
                break;
        }
    }
}
