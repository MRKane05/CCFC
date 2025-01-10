using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//A helper script that'll take different sounds and send commands through to the system to play/do different things
public class UIButtonFunction : MonoBehaviour, ISelectHandler
{
    //public bool bNeedsSelected = false;

    public virtual void OnClick()
    {
        if (UIInteractionSound.Instance)
        {
            UIInteractionSound.Instance.PlayClick();
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (UIInteractionSound.Instance)
        {
            UIInteractionSound.Instance.PlaySelect();
        }
    }
}
