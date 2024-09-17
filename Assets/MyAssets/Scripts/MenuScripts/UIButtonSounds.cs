using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//A helper script that'll take different sounds and send commands through to the system to play/do different things
public class UIButtonSounds : MonoBehaviour, ISelectHandler
{
    float sliderStartValue = 1f;
    float sliderTickValue = 1f / 11f;

    public void OnClick()
    {
        /*
        if (GameController.Instance)
        {
            GameController.Instance.PlayClick();
        }*/
    }

    public void OnSelect(BaseEventData eventData)
    {
        /*
        if (GameController.Instance)
        {
            GameController.Instance.PlaySelect();
        }*/
    }

    public void ValueChanged(float toThis)
    {
        if (Mathf.Abs(sliderStartValue - toThis) > sliderTickValue)
        {
            sliderStartValue = toThis;  //We should set this for when it crosses thresholds...
            /*
            if (GameController.Instance)
            {
                GameController.Instance.PlaySelect();
            }*/
        }
    }
}
