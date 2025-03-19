using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SetImageColors : MonoBehaviour {

    public Image targetImage;
    public Color NormalColor = Color.white;
    public Color HighlightedColor = Color.white;
    public Color PressedColor = Color.white;
    public Color DisabledColor = Color.white;

    void Awake()
    {
        targetImage = gameObject.GetComponent<Image>();
    }

    public void setSelectedState(bool bSelected)
    {
        targetImage.color = bSelected ? HighlightedColor : NormalColor;
    }
}
