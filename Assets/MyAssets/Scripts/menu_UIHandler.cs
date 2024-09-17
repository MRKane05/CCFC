using UnityEngine;
using System.Collections;

//Basically when we junction between windows, we reference back to this.
public class menu_UIHandler : MonoBehaviour {
	public GameObject menuCamera;
	//Because everything is active at once we always link back to this window
	public GameObject[] cameraWindowPositions; //the camera will interpolate to these positions as we change the game window
	
	int currentPage=0;
	
	void Start() {
		if (menuCamera==null)
			Debug.LogError("No menu camera assigned to the menu_UIHandler on " + gameObject.name);
		
		setPosition(currentPage);
		
	}
	
	void setPosition(int thisPoint) {
		menuCamera.transform.position = cameraWindowPositions[thisPoint].transform.position;
		menuCamera.transform.rotation = cameraWindowPositions[thisPoint].transform.rotation;
	}
	
	
	public void cycleForward() { //move forward with our menus
		currentPage++;
		
		setPosition(currentPage);
	}
	
	public void cycleBack() { //move back!
		currentPage--;
		
		setPosition(currentPage);
	}
}
