using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ToggleButton : UIButtonFunction {
    public bool bIsSelected = false;
    public Image selectImage;

    void Start()
    {
        setSelectImage();
    }

    void setSelectImage()
    {
        selectImage.enabled = bIsSelected;
    }

    public virtual void toggleSelectState()
    {
        if (UIInteractionSound.Instance)
        {
            UIInteractionSound.Instance.PlayClick();
        }
        bIsSelected = !bIsSelected;
        setSelectImage();
    }
}
