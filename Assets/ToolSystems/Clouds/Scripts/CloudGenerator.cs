using UnityEngine;
using System.Collections;


[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshFilter))]
public class CloudGenerator : MonoBehaviour {

	float repositionDistance=90; //how far before we reposition the cloud? Zero is no reposition
	private float repositionSquare;


	//we won't need any normals
	Vector3[] verts;
	Vector2[] uvs, uv2s;
	Color[] colors;

	Vector3[] cloudCenters; //these are the center points of the cloud items

	private Mesh mesh;

	public Texture2D cloudDensityTex; //used to generate our cloud "shapes"
	public float densityCutoff=0.3F; //if it's below this value then don't allow a point down.
	public float cloudDensityArea=0.1F; //the larger this is the more it'll span the above texture

	public int cloudPlanes = 8;
	public float cloudHeight=1, cloudWidth=3, cloudLength=2; //and a rotation? Not sure about that. Don't care at the moment
	public float panelSize = 1;
	public float uvDensity = 0.33F, panelSegments=2; //how tightly we pack around the UV Map, and what's the subdivision of the maskUVs?

	//Finally, our color graduations...
	//would be nice to somehow give these some serious depth etc, but for the mo we'll just try some basic stuff
	public Color highColor, lowColor;


	public bool bAllowMaskFlip=true; //if true masks can be Randomly xFlipped by the system to allow doubling the mask random
	public bool bRegen=false;
	// Use this for initialization
	void Start () {
		generateCloud();
		repositionSquare = repositionDistance*repositionDistance; //square distance stuff.
	}

	void generateCloud() {
		mesh = new Mesh();

		int[] triangles = new int[cloudPlanes*6]; //two tris per quad
		//assign out our tris
		for (int i=0; i<cloudPlanes; i++) {
			triangles[i*6 + 0] = i*4 + 0; //these need to relate to the quads
			triangles[i*6 + 1] = i*4 + 1; //these need to relate to the quads
			triangles[i*6 + 2] = i*4 + 3; //these need to relate to the quads
			//triangle 2:
			triangles[i*6 + 3] = i*4 + 0; //these need to relate to the quads
			triangles[i*6 + 4] = i*4 + 3; //these need to relate to the quads
			triangles[i*6 + 5] = i*4 + 2; //these need to relate to the quads
		}

		//assign our cloud centers - will just be (at the moment) randomly placed center points around
		//which the polygon faces are put
		cloudCenters = new Vector3[cloudPlanes];
		int texSize = cloudDensityTex.width;
		//we could do with a "clumping" pattern for this
		for (int i=0; i<cloudPlanes; i++) {
			//actually, we can use a texture here and a segment from that cloud texture to create
			//a cloud density and shape by checking (again) against randoms
			Vector2 cloudDensityCorner = new Vector2(Random.Range(0, 1-cloudDensityArea),Random.Range(0, 1-cloudDensityArea)); //used for sampling a section of texture
			float random=1, texture=0;
			float xPos=0, yPos=0;


			while (random > texture && texture < densityCutoff) { //go through and spawn random values within the field to get our clouds
				xPos = Random.Range(0F, 1F);
				yPos = Random.Range(0F, 1F);

				texture = cloudDensityTex.GetPixel(Mathf.RoundToInt(texSize*(cloudDensityCorner.x + xPos*cloudDensityArea)), Mathf.RoundToInt(texSize*(cloudDensityCorner.y + yPos*cloudDensityArea)))[3]; //read this alpha
				random = Random.Range (0.1F, 1F); //our threshold and area values
			}

			cloudCenters[i] = new Vector3(xPos*cloudWidth, Random.Range (0, cloudHeight), yPos*cloudLength);
		}

		//for the actual assignment of the verticies we need to know what direction the camera is pointing in, and get the up and right coordinates from that, but we'll cheat for now
		Vector3 targetUp = Vector3.up;
		Vector3 targetRight = Vector3.right;

		verts = new Vector3[cloudPlanes*4];
		uvs = new Vector2[cloudPlanes*4];
		uv2s = new Vector2[cloudPlanes*4];
		colors = new Color[cloudPlanes*4];

		for (int i=0; i<cloudPlanes; i++) {
			verts[i*4+0] = cloudCenters[i]+targetUp*panelSize-targetRight*panelSize;
			verts[i*4+1] = cloudCenters[i]+targetUp*panelSize+targetRight*panelSize;
			verts[i*4+2] = cloudCenters[i]-targetUp*panelSize-targetRight*panelSize;
			verts[i*4+3] = cloudCenters[i]-targetUp*panelSize+targetRight*panelSize;

			//next is our UV offsets.
			Vector2 uvBase = new Vector2(Random.Range (0F, 1F), Random.Range (0F, 1F)); //use this for the base coords of our UV system

			//put our UV stuff down as random placements
			uvs[i*4+0] = uvBase + new Vector2(0, 0);
			uvs[i*4+1] = uvBase + new Vector2(uvDensity, 0);
			uvs[i*4+2] = uvBase + new Vector2(0, uvDensity);
			uvs[i*4+3] = uvBase + new Vector2(uvDensity, uvDensity);

			//Cloud colors...a little more involved than plain simplicity...
			//Should we stratify these in some way?
			//Might be an idea to cross again against the height of the actual point here and the maximum possible point of the cloud? Kind of
			//only works better in my head
			colors[i*4+0] = highColor; //Color.Lerp (highColor, lowColor, 0.5F); //curve definition on the upper edge of the cloud
			colors[i*4+1] = highColor;
			colors[i*4+2] = lowColor;
			colors[i*4+3] = lowColor;

			//Pull our UV2s from a grid of different "shapes" for each segment, these will be used to add to the alphas
			float panelX = Mathf.RoundToInt(Random.Range(0, panelSegments-1F))/panelSegments; //what's the offset on our first panel segment?
			float panelY = Mathf.RoundToInt(Random.Range(0, panelSegments-1F))/panelSegments; //and the offset on our second segment
			uvBase = new Vector2(panelX, panelY);
			float panelSegment = 1f/panelSegments;


			//this needs to be randomlly flippable from the mask system

			int maskDir=1;
			if (bAllowMaskFlip && (Random.Range(0F, 1F) > 0.5F))
				maskDir = -1; //flip this out
			uv2s[i*4+0] = uvBase + new Vector2(0, 0);
			uv2s[i*4+1] = uvBase + new Vector2(panelSegment*maskDir, 0);
			uv2s[i*4+2] = uvBase + new Vector2(0, panelSegment);
			uv2s[i*4+3] = uvBase + new Vector2(panelSegment*maskDir, panelSegment);

		}

		//set our UVS for this object
		mesh.vertices = verts;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.uv2 = uv2s;
		mesh.colors = colors;

		//and our other stuff.
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		GetComponent<MeshFilter>().sharedMesh = mesh;


	}

	void updateCloud() {
		//we should only need to reset our verticies here
		//Debug.Log (CloudViewer.Instance.cloudRight);

		for (int i=0; i<cloudPlanes; i++) {
			verts[i*4+0] = cloudCenters[i]+CloudViewer.Instance.cloudUp*panelSize-CloudViewer.Instance.cloudRight*panelSize;
			verts[i*4+1] = cloudCenters[i]+CloudViewer.Instance.cloudUp*panelSize+CloudViewer.Instance.cloudRight*panelSize;
			verts[i*4+2] = cloudCenters[i]-CloudViewer.Instance.cloudUp*panelSize-CloudViewer.Instance.cloudRight*panelSize;
			verts[i*4+3] = cloudCenters[i]-CloudViewer.Instance.cloudUp*panelSize+CloudViewer.Instance.cloudRight*panelSize;
		
		}

		mesh.vertices = verts;
	}
	
	// Update is called once per frame
	void Update () {
		if (bRegen) {
			bRegen = false;
			generateCloud();
		}

		//should put something in to make sure we need to update our cloud stuff
		if (CloudViewer.Instance!=null && CloudViewer.Instance.bShouldRefresh())
			updateCloud();

		//check to see if we need to reposition this cloud...
		if (PlayerController.Instance!=null) {
			if ((PlayerController.Instance.ourAircraft.transform.position-transform.position).sqrMagnitude > repositionSquare) {//the cloud is outside of the range.
				//pick an angle and put this cloud offset from the player aircraft
				float newAngle = Random.Range(0, Mathf.PI*2);

				Vector3 cloudOffset = new Vector3(Mathf.Sin (newAngle)*repositionDistance*0.9F, 0, Mathf.Cos (newAngle)*repositionDistance*0.9F);
			
				transform.position = PlayerController.Instance.ourAircraft.transform.position + cloudOffset;
				generateCloud(); //rebuild our cloud for randomness

			}
		}
	}
}
