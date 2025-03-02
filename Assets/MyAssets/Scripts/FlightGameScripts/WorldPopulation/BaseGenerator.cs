using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BaseGenerator : MonoBehaviour {
	public int[][] layoutPattern;	//In theory we can use a grid, although I don't even know we'll need that.
	public enum enBaseType { NULL, STRAIGHT, CORNER}
	public enBaseType BaseType = enBaseType.STRAIGHT;

	public float tileSize = 100f;

	public GameObject baseParent;

	public GameObject HangarPrefab;

	public List<GameObject> TilePrefabs;

	public bool bGenerateBase = false;

	public DestructableObject[] spawnedBaseObjects;

	//because we need some concept of where the center of the base is
	public Vector3 BaseCenterPoint = Vector3.zero;

	// Use this for initialization
	public IEnumerator CreateBase () {
		yield return null;//We really need to make a trigger after the terrain has finished building
		if (baseParent)
		{
			DestroyImmediate(baseParent);   //Clear our old base
		}
		MakeStraightBase(Vector3.zero);
	}

	void Update()
    {
		if (bGenerateBase)
        {
			bGenerateBase = false;
			if (baseParent)
            {
				DestroyImmediate(baseParent);	//Clear our old base
            }
			MakeStraightBase(Vector3.zero);	//Make a new base
        }
    }

	public Vector3 getTerrainHeightAtPoint(Vector3 point)
	{
		RaycastHit hit;
		LayerMask maskAll = 0x01 << LayerMask.NameToLayer("Ground");    //We only want to cast against the terrain
																		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(point + Vector3.up * 2000f, -Vector3.up, out hit, Mathf.Infinity, maskAll))

		{
			//Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
			//Debug.Log("Did Hit");
			return hit.point;
		}
		else
		{
			//Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
			//Debug.Log("Did not Hit");
		}
		return point;   //Assume we didn't find anything so lets go ahead

	}

	void MakeStraightBase(Vector3 startLocation)
    {
		//A straight base has a runway up one side and the rest of the base off to the left/right
		baseParent = new GameObject("Base Parent");
		baseParent.transform.position = getTerrainHeightAtPoint(startLocation);
		//So we're going to build all of the base off of the base parent

		//We need a runway to one side.
		//Beside the runway we've got Hangars
		int numHangars = Random.Range(3, 7);
		//In theory we've got an "out line" which the base will branch out from, and we can adjust the hangars offset from that. For the moment lets just add stuff!
		for (int i=0; i<numHangars; i++)
        {
			GameObject newHangar = Instantiate(HangarPrefab) as GameObject;
			newHangar.transform.SetParent(baseParent.transform);
			newHangar.transform.localPosition = Vector3.forward * i * tileSize; //Include a side offset perhaps? We'll need that for the tiles
			newHangar.transform.position = getTerrainHeightAtPoint(newHangar.transform.position);
        }
		//To the other side of the hangars we've got the rest of the base
		int clusterPoint = Random.Range(0, numHangars); //So we'll setup a probability table for putting down our detail tiles
		Vector3 localClusterPoint = Vector3.forward * clusterPoint * tileSize;
		int width = Random.Range(3, 7);

		bool[,] cells = new bool[width, numHangars];	//quick map so that we don't put structures on top of other structures

		float chord = Mathf.Sqrt(width * width + numHangars * numHangars);
		int targetNumStructures = Random.Range(Mathf.FloorToInt(width * numHangars * 0.33f), Mathf.FloorToInt(width * numHangars * 0.66f));
		int numStructures = 0;
		while (numStructures < targetNumStructures)
		{
			for (int y = 0; y < numHangars; y++)
			{
				for (int x = 0; x < width; x++) //We need to step this off so we don't put these down on our hangars
				{
					if (!cells[x, y])
					{
						Vector3 tilePosition = Vector3.forward * y * tileSize + Vector3.right * (x+1) * (tileSize);
						float tileDistance = Vector3.Distance(localClusterPoint, tilePosition);
						float tile_prop = tileDistance * 1.44f / (chord * tileSize);
						if (Random.value > tile_prop)
						{
							GameObject newTile = Instantiate(TilePrefabs[Random.Range(0, TilePrefabs.Count)]);
							newTile.transform.eulerAngles = new Vector3(0, Random.Range(0, 4) * 90, 0);
							newTile.transform.SetParent(baseParent.transform);
							newTile.transform.localPosition = tilePosition;
							newTile.transform.position = getTerrainHeightAtPoint(newTile.transform.position);
							//We should do a random rotation, 0, 90, 180, 270
							numStructures++;
							cells[x, y] = true;
						}
					}
				}
			}
		}

		//Ok, we should populate our objects for monitoring with the level gameplay
		spawnedBaseObjects = baseParent.GetComponentsInChildren<DestructableObject>();

		//Quick and dirty calculation for our center point. Really we should get a size value too, but I'll worry about that later
		BaseCenterPoint = Vector3.zero;
		foreach (DestructableObject thisObject in spawnedBaseObjects)
        {
			BaseCenterPoint += thisObject.gameObject.transform.position;
        }
		BaseCenterPoint /= (float)spawnedBaseObjects.Length;
	}

	public Vector3 getBomberTarget()
    {
		//For the moment lets return anything, although I'd like to advance this to try to return targets along a path
		//We could also have values assigned to different targets and use those to assess mission success
		return spawnedBaseObjects[Random.Range(0, spawnedBaseObjects.Length)].gameObject.transform.position;

	}
}
