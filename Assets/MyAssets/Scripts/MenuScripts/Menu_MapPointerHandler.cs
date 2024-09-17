using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Menu_MapPointerHandler : MonoBehaviour {
	private static Menu_MapPointerHandler instance;
	public static Menu_MapPointerHandler Instance { get { return instance; } }

	public GameObject PointerObject, BaseCanvas;
	RectTransform PointerRect, ParentRect;
	public CanvasGroup BaseCanvasGroup;
	Vector3 PointerRelativePosition;
	float CursorMoveSpeed = 300f;
	float PointerAlpha = 1f;

	[HideInInspector] public GameObject HoveredTile;

	void Start()
    {
		if (instance)
		{
			Debug.Log("Duplicate attempt to create Menu_MapPointerHandler");
			Destroy(this);
			return;
		}

		instance = this;


		PointerRelativePosition = PointerObject.transform.localPosition;
		PointerRect = PointerObject.GetComponent<RectTransform>();
		ParentRect = PointerObject.transform.parent.GetComponent<RectTransform>();
    }

	// Update is called once per frame
	void Update () {
		Vector3 PointerMotion = new Vector3(Input.GetAxis("Right Stick Horizontal"), -Input.GetAxis("Right Stick Vertical"), 0);
		
		//And our keypad controls
		if (Input.GetKey(KeyCode.RightArrow))
        {
			PointerMotion += Vector3.right;
        } else if (Input.GetKey(KeyCode.LeftArrow))
        {
			PointerMotion -= Vector3.right;
        }

		if (Input.GetKey(KeyCode.UpArrow))
        {
			PointerMotion += Vector3.up;
        } else if (Input.GetKey(KeyCode.DownArrow))
        {
			PointerMotion -= Vector3.up;
        }

		//Now position our cursor accordingly
		PointerRelativePosition += PointerMotion * (Time.deltaTime * CursorMoveSpeed);
		//We need to clamp our positions
		PointerRelativePosition = new Vector3(Mathf.Clamp(PointerRelativePosition.x, -360, 360), Mathf.Clamp(PointerRelativePosition.y, -180, 180), 0);
		PointerObject.transform.localPosition = PointerRelativePosition;

		if (PointerMotion.sqrMagnitude > 0.25f)
        {
			PointerAlpha = 5f;
        } else
        {
			PointerAlpha = Mathf.Lerp(PointerAlpha, 0, Time.deltaTime);
        }

		BaseCanvasGroup.alpha = Mathf.Lerp(0.3f, 1.0f, Mathf.Clamp01(PointerAlpha));

		//Get the object under the cursor
		Vector2 PointerRayPosition = new Vector2((0.5f+(PointerRect.transform.localPosition.x/ParentRect.rect.width)) * Screen.width, (0.5f+(PointerRect.transform.localPosition.y / ParentRect.rect.height))*Screen.height);
		Ray rayVector = Camera.main.ScreenPointToRay(PointerRayPosition);
		RaycastHit hit;
		//Debug.DrawLine(rayVector.origin, rayVector.origin + rayVector.direction * 20f, Color.red, 1f);

		if (Physics.Raycast(rayVector, out hit, 20f))
		{
			//Debug.Log(hit.collider.gameObject.name);
			HoveredTile = hit.collider.gameObject;	//So that our buttons can figure out which one is being hovered over
			if (Input.GetButtonDown("Cross")|| Input.GetKeyDown(KeyCode.Return))
            {
				//See about selecting a tile :)
				Mission_MapSection SelectedTile = HoveredTile.GetComponent<Mission_MapSection>();
				if (SelectedTile)
                {
					SelectedTile.sectionClick();
                }
            }
		}
	}
}
