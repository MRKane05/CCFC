using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMissionBreifPanel : MonoBehaviour
{
	public TextMeshProUGUI title;
	public TextMeshProUGUI description;

	// Use this for initialization
	IEnumerator Start()
	{
		while (!gameManager.Instance)
        {
			yield return null;
        }

		title.text = gameManager.Instance.panelTitle;
		description.text = gameManager.Instance.panelContent;
	}
}
