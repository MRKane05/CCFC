using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIScrollRectPositionHandler : MonoBehaviour {
    public ScrollRect ourScrollRect;
    public Selectable[] UISelectables;

    RectTransform viewportRect;
    GameObject selectedItem = null;
    public Range ViewRectRange;
    bool bIsLerping = false;
    void Start()
    {
        ourScrollRect = gameObject.GetComponent<ScrollRect>();
        viewportRect = ourScrollRect.viewport.GetComponent<RectTransform>();

        Vector3[] viewRectCorners = new Vector3[4];
        viewportRect.GetWorldCorners(viewRectCorners);
        //Lets digest these down to something simpler
        float minHeight = 999999;
        float maxHeight = -999999;
        foreach(Vector3 thisVector in viewRectCorners)
        {
            minHeight = Mathf.Min(thisVector.y, minHeight);
            maxHeight = Mathf.Max(thisVector.y, maxHeight);
        }
        minHeight -= 10;
        maxHeight += 10;
        ViewRectRange = new Range(minHeight, maxHeight);
        UISelectables = gameObject.GetComponentsInChildren<Selectable>();

    }

    void Update()
    {
        if (selectedItem == null && EventSystem.current.currentSelectedGameObject != null)
        {
            selectedItem = EventSystem.current.currentSelectedGameObject;
        }
        if (selectedItem)
        {
            if (selectedItem != EventSystem.current.currentSelectedGameObject)
            {
                selectedItem = EventSystem.current.currentSelectedGameObject;
                //Debug.Log("Switching Menu Item");
                float outOffset = 0;
                if (!GetListOffset(out outOffset))
                {
                    Vector3 contentTargetPos = ourScrollRect.content.transform.localPosition + new Vector3(0, outOffset, 0);
                    ourScrollRect.content.transform.DOLocalMove(contentTargetPos, 1f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Late, true);
                }
            }
        }
    }

    IEnumerator ShiftContentByFloat(float toThis)
    {
        bIsLerping = true;
        float startTime = Time.unscaledTime;
        float duration = 1.0f;
        Vector3 contentStartPos = ourScrollRect.content.transform.localPosition;
        Vector3 contentEndPos = contentStartPos + new Vector3(0f, toThis, 0f);
        while (Time.unscaledTime < startTime + duration)
        {
            float t = (startTime + duration - Time.unscaledTime) / duration;
            ourScrollRect.content.transform.localPosition = Vector3.Slerp(contentStartPos, contentEndPos, t);
            yield return null;
        }
        ourScrollRect.content.transform.localPosition = contentEndPos;
        bIsLerping = false;
    }

    bool GetListOffset(out float menuOffset)
    {
        menuOffset = 0;
        Vector3[] ChildCorners = new Vector3[4];
        if (EventSystem.current.currentSelectedGameObject)
        { //PROBLEM: It feels like there's a bug here waiting to happen with this
            RectTransform ChildRect = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();
            ChildRect.GetWorldCorners(ChildCorners);

            float minHeight = 999999f;
            float maxHeight = -999999f;
            foreach (Vector3 thisVector in ChildCorners)
            {
                minHeight = Mathf.Min(thisVector.y, minHeight);
                maxHeight = Mathf.Max(thisVector.y, maxHeight);
            }

            Range LocalRectRange = new Range(minHeight, maxHeight);
            //Basically we need to see if these corners are contained vertically (y axis) within the other four corners of our array
            if (LocalRectRange.Max > ViewRectRange.Max)
            {
                menuOffset = ViewRectRange.Max - LocalRectRange.Max;
                //Debug.Log("Max Range :" + LocalRectRange.Max + ", View: " + ViewRectRange.Max);
                return false;
            }

            if (LocalRectRange.Min < ViewRectRange.Min)
            {
                menuOffset = ViewRectRange.Min - LocalRectRange.Min;
                //Debug.Log("Min Range :" + (ViewRectRange.Min - LocalRectRange.Min));
                return false;
            }
        }

        return true;
    }
 
}
