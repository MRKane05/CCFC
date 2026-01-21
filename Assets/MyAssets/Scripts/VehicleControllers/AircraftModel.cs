using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A base class for setting up the apparence of one of our models. This handles things like colour and maybe later textures
public class AircraftModel : MonoBehaviour {
	public GameObject modelBase;	//This'll be a LOD group
	public GameObject outlineBase;  //And this'll be a LOD group

	public Material modelMat;
	public Material outlineMat;
	
	void Start () {
		//Sort out our materials

		LODGroup lodGroup = modelBase.GetComponent<LODGroup>();
		if (lodGroup)
        {
			modelMat = new Material(modelBase.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial);
			foreach (Transform child in modelBase.transform)
            {
				child.GetComponent<Renderer>().material = modelMat;
            }
        } else
        {
			modelMat = new Material(modelBase.GetComponent<Renderer>().sharedMaterial);
			modelBase.GetComponent<Renderer>().material = modelMat;
		}
		//And our outline (which will be used for targeting)
		LODGroup outlineLodGroup = modelBase.GetComponent<LODGroup>();
		if (outlineLodGroup)
		{
			outlineMat = new Material(outlineBase.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial);
			foreach (Transform child in outlineBase.transform)
			{
				child.GetComponent<Renderer>().material = outlineMat;
			}
		}
		else
		{
			outlineMat = new Material(outlineBase.GetComponent<Renderer>().sharedMaterial);
			outlineBase.GetComponent<Renderer>().material = modelMat;
		}
	}
}
