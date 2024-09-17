using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//Handles input from each section, and should also be used to populate the menu information etc.
public class Mission_MapSection : MonoBehaviour {

	public Text tileLabel;

	public int segmentNumber=-1; //used to reference this when it's selected
	public Mission_MapManager ourManager;
	public GameObject conflictMarker;   //Just an indicator we'll turn on/off to indicate that this tile is in conflict
	public TMP_Text ConflictText;
	public GameObject KeyAreaMarker;

	public GameObject hoverIndicator;

	public Button ourButton;
	public int m_team=-1; //which team does this square belong to?
	protected float m_areaForce = 20f;

	public bool bNoMansLand = false;    //Used by our system to know if this is a tile where fighting is happening

	Color TileColor = Color.white;
	public int team { 
		get {return m_team; }
		set {m_team = value; }
	}

	public float areaForce {
		get { return m_areaForce; }
		set { m_areaForce = value; }
	}

	public void setKeyArea(bool bEnabled)
    {
		KeyAreaMarker.SetActive(bEnabled);
	}

	public void setConflictMarker(Color thisTint, string thisText, bool bIsOn)
    {
		conflictMarker.SetActive(bIsOn);
		//Because we're just using text...
		ConflictText.color = thisTint;
		ConflictText.text = thisText;
	}

	public void setConflictMarkerText(string thisText)
    {
		ConflictText.text = thisText;
	}

	public void Update()
	{
		if (Menu_MapPointerHandler.Instance)
		{
			hoverIndicator.SetActive(Menu_MapPointerHandler.Instance.HoveredTile == gameObject && ourButton.interactable);
		}
	}

	public void setTileLabel(string newLabel) {
		tileLabel.text = newLabel;
	}

	public void setTileConflict(bool toThis)
    {
		conflictMarker.SetActive(toThis);
	}

	public void setTint(Color newColor, bool bInteractable, bool bAnimateTint) {
		ourButton.interactable = bInteractable;	//So we can only select border tiles to fly

		if (TileColor != newColor && bAnimateTint)
        {
			//This isn't working as well as it should be, but it's working I suppose
			gameObject.transform.DOPunchScale(Vector3.one, 1f, 10, 1).SetUpdate(true).OnComplete(() => { gameObject.transform.localScale = Vector3.one; });
        }

		TileColor = newColor;
		gameObject.GetComponent<Image>().color = newColor;
	}

	public void setTeam(int newTeam) {
		m_team = newTeam;
	}

	//this button has been pressed
	public void sectionClick() {
		ourManager.selectMapSegment(segmentNumber);
	}
}
