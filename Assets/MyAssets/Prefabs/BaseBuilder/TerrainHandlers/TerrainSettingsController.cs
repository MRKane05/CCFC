using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainSettingsController : MonoBehaviour {
	Terrain terrain;
	public float detailObjectDistance = 100f;	//Adjust LOD distance
	public float basemapDistance = 500f;// Adjust base map rendering distance
	public float heightmapPixelError = 10f; // Higher value = lower detail at a distance (better performance)

	public bool bApplySettings = false;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (bApplySettings)
        {
			bApplySettings = false;
			setValues();
        }
	}

	public void setValues()
    {
		if (!terrain)
        {
			terrain = gameObject.GetComponent<Terrain>();
			terrain.detailObjectDistance = detailObjectDistance;
			terrain.basemapDistance = basemapDistance;
			terrain.heightmapPixelError = heightmapPixelError;
        }
    }
}
