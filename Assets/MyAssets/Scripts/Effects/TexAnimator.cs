using UnityEngine;
using System.Collections;

//for the things that we can't programme into the shader itself...
[ExecuteInEditMode]
public class TexAnimator : MonoBehaviour {
	public Material thisMat;
	public string channelName;
	public Vector4 animSpeed;

	Vector4 animOffset;
	
	// Update is called once per frame
	void LateUpdate () {
		animOffset += new Vector4(Mathf.Repeat (animSpeed[0]*Time.deltaTime, 1f), Mathf.Repeat (animSpeed[1]*Time.deltaTime, 1f), Mathf.Repeat (animSpeed[2]*Time.deltaTime, 1f), Mathf.Repeat (animSpeed[3]*Time.deltaTime, 1f));
		if (thisMat)
		{
			thisMat.SetVector(channelName, animOffset);
		}
	}
}
