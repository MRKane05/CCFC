using UnityEngine;

 

public class VirtualJoystick : MonoBehaviour {

	public GUIText ourGUIText;
    public static Vector2 VJRvector;    // Joystick's controls in Screen-Space.
    public Vector2 VJRnormals;   		// Joystick's normalized controls.
    public static bool VJRdoubleTap;    // Player double tapped this Joystick.
    public Color activeColor;           // Joystick's color when active.
    public Color inactiveColor;         // Joystick's color when inactive.
    public Texture2D joystick2D;        // Joystick's Image.
    public Texture2D background2D;      // Background's Image.
   	public Vector4 StickActiveArea;		// The Area where the joystick is active, in screen-relitive coords, xMin,xMax, yMin, yMax;
	private GameObject backOBJ;         // Background's Object.
    private GUITexture joystick;        // Joystick's GUI.
    private GUITexture background;      // Background's GUI.
    private Vector2 origin;             // Touch's Origin in Screen-Space.
    private Vector2 position;           // Pixel Position in Screen-Space.
    private int size;                   // Screen's smaller side.
    private float length;               // The maximum distance the Joystick can be pushed.
    private bool gotPosition;           // Joystick has a position.
    private int fingerID;               // ID of finger touching this Joystick.
    private int lastID;                 // ID of last finger touching this Joystick.
    private float tapTimer;             // Double-tap's timer.
    private bool enable;                // VJR external control.
	private bool bNeedsReset=false;			// Used with the finger touch checking ID for a finger getting held down.
   	public float JoySize = 0.6F;
	//
	Rect ControlAreaRect;				// Will be built from the information in StickActiveArea and is used to register the touch.
	float StickFadeTime, StickFadeDuration=0.3F; //Fade Details, mainly only for fade-outs. Problem here is that we get a hang over effect while this is in place.
	//Color StickIntendedColor; 			//What the stick color should be. So it fades in and out and looks cool
	
    public void DisableJoystick() {
		enable = false; ResetJoystick();
	}
    public void EnableJoystick() {
		enable = true; ResetJoystick();
	}
    // 

    private void ResetJoystick() {
		bNeedsReset=false;
        VJRvector = new Vector2(0,0); 
		VJRnormals = VJRvector;
        lastID = fingerID; 
		fingerID = -1; 
		tapTimer = 0.150f;
        joystick.color = inactiveColor; 
		StickFadeTime=Time.time+StickFadeDuration;
		position = origin; 
		gotPosition = false;
    }

    private Vector2 GetRadius(Vector2 midPoint, Vector2 endPoint, float maxDistance) {
        Vector2 distance = endPoint;
        if (Vector2.Distance(midPoint,endPoint) > maxDistance) {
            distance = endPoint-midPoint; distance.Normalize();
            return (distance*maxDistance)+midPoint;
        }
        return distance;
    }

    private void GetPosition() {
        foreach (Touch touch in Input.touches) {
            if (touch.fingerId >= 0 && fingerID < Input.touchCount) {
                if (ControlAreaRect.Contains(new Vector2(Input.GetTouch(touch.fingerId).position.x, Screen.height-Input.GetTouch(touch.fingerId).position.y)) && Input.GetTouch(touch.fingerId).phase == TouchPhase.Began) {
					fingerID = touch.fingerId;
					position = Input.GetTouch(fingerID).position;
					origin = position;
                    joystick.texture = joystick2D; 
					joystick.color = activeColor;
					StickFadeTime = Time.time-StickFadeDuration*2; //just to be sure we don't fade it out again!
                    background.texture = background2D; 
					background.color = activeColor;
					/*
                    if (fingerID == lastID && tapTimer > 0) {
						VJRdoubleTap = true;
					} 
					*/
					gotPosition = true;
                }
            }
        }
    }

    private void GetConstraints() {
		//mins...
        if (origin.x < StickActiveArea[0]*Screen.width+(background.pixelInset.width/2)+25) {
			origin.x = StickActiveArea[0]*Screen.width+(background.pixelInset.width/2)+25;
		}
        if (origin.y < StickActiveArea[2]*Screen.height+(background.pixelInset.height/2)+25) {
			origin.y = StickActiveArea[2]*Screen.height+(background.pixelInset.height/2)+25;
		}
		//maxes...
        if (origin.x > StickActiveArea[1]*Screen.width-(background.pixelInset.width/2)-25) {
			origin.x = StickActiveArea[1]*Screen.width-(background.pixelInset.width/2)-25;
		}
        if (origin.y > StickActiveArea[3]*Screen.height-(background.pixelInset.width/2)-25) {
			origin.y = StickActiveArea[3]*Screen.height-(background.pixelInset.width/2)-25;
		}
    }
    
    private Vector2 GetControls(Vector2 pos, Vector2 ori) {
        Vector2 vector = new Vector2();
        if (Vector2.Distance(pos,ori) > 0) {
			vector = new Vector2(pos.x-ori.x,pos.y-ori.y);
		}
        return vector;
    }

    //
    //private void Awake() {
	public void SetupStick(Vector4 newStickActiveArea) { //called from the function when the stick is created
		StickActiveArea=newStickActiveArea;
		//Setup the stick-control area. Built from StickActiveArea.
		ControlAreaRect = new Rect(Screen.width*StickActiveArea[0], Screen.height*StickActiveArea[2], Screen.width*(StickActiveArea[1]-StickActiveArea[0]), Screen.height*(StickActiveArea[3]-StickActiveArea[2]));
		
        gameObject.transform.localScale = new Vector3(0,0,0);
        gameObject.transform.position = new Vector3(0,0,999);
       
		if (Screen.width > Screen.height) {
			size = Screen.height;
		}
		else {
			size = Screen.width;
		} 
		VJRvector = new Vector2(0,0);
        joystick = gameObject.AddComponent<GUITexture>() as GUITexture;
        joystick.texture = joystick2D; 
		joystick.color = inactiveColor;
        backOBJ = new GameObject("VJR-Joystick Back");
        backOBJ.transform.localScale = new Vector3(0,0,0);
		backOBJ.transform.parent = transform;
        background = backOBJ.AddComponent<GUITexture>() as GUITexture;
        background.texture = background2D;
		background.color = inactiveColor;
        fingerID = -1; lastID = -1; 
		VJRdoubleTap = false; 
		tapTimer = 0; 
		length = size*JoySize/2; //this begs for tweaking...
		//position needs to be set to somewhere that's more suitable than this.
        position = new Vector2(Screen.width*(StickActiveArea[0]+StickActiveArea[1])/2,Screen.height*(StickActiveArea[2]+StickActiveArea[3])/2); 
		origin = position;
        gotPosition = false;
		EnableJoystick();
		enable = true;
    }
	
	public void SetStickArea(Vector4 newStickActiveArea) { //used when we want to reset our stick anywhere during play.
		ControlAreaRect = new Rect(Screen.width*StickActiveArea[0], Screen.height*StickActiveArea[2], Screen.width*(StickActiveArea[1]-StickActiveArea[0]), Screen.height*(StickActiveArea[3]-StickActiveArea[2]));
	    position = new Vector2(Screen.width*(StickActiveArea[0]+StickActiveArea[1])/2,Screen.height*(StickActiveArea[2]+StickActiveArea[3])/2); 
		origin = position;
        gotPosition = false;
		EnableJoystick();
		enable = true;
		
	}


    private void Update() {
		
		if (Time.time < StickFadeTime+0.1F) { //then we're fading out our joystick area
			joystick.color = Color.Lerp(inactiveColor, activeColor, Mathf.Clamp01((StickFadeTime-Time.time)/StickFadeDuration));
			background.color = joystick.color; //update our background also.
			//we should also reset our input here so that it doesn't subsist.
			VJRvector = Vector2.zero; 
			VJRnormals = VJRvector;
			
		}
		
		
        if (tapTimer > 0) {
			tapTimer -= Time.deltaTime;
		}
		
		//Handle our resets. Not really sure why we need this given that we've got checks later on
		if (fingerID > -1 && Input.touchCount>0) {
			bNeedsReset=true;
			foreach(Touch touch in Input.touches) {
				if (touch.fingerId == fingerID) { //then we should be reading from this one as it's still active.
					bNeedsReset=false;
				}
			}
			if (bNeedsReset) {
				ResetJoystick();
			}
		}
		
		
        if (enable == true) {
            if (Input.touchCount > 0 && gotPosition == false) {
				GetPosition(); 
				GetConstraints();
			}
            if (Input.touchCount > 0 && fingerID > -1 && gotPosition == true) {
                foreach (Touch touch in Input.touches) {
                    if (touch.fingerId == fingerID) {
                        position = touch.position; 
						position = GetRadius(origin,position,length);
                        VJRvector = GetControls(position,origin); 
						VJRnormals = new Vector2(VJRvector.x/length,VJRvector.y/length);
 
						//look to see if we've got a cancel
						if (!ControlAreaRect.Contains(new Vector2(touch.position.x, Screen.height-touch.position.y)) || touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
							ResetJoystick();
						}
           			 }
				}
			}

			
            background.pixelInset = new Rect(origin.x-(background.pixelInset.width/2),origin.y-(background.pixelInset.height/2),size*JoySize,size*JoySize);
            joystick.pixelInset = new Rect(position.x-(joystick.pixelInset.width/2),position.y-(joystick.pixelInset.height/2),size/11,size/11);
        } else if (background.pixelInset.width > 0) {
			background.pixelInset = new Rect(0,0,0,0); 
			joystick.pixelInset = new Rect(0,0,0,0);
		}
		
		if (ourGUIText) {
			ourGUIText.text = "Joystick Axis: " + VJRnormals + " FingerID " + fingerID;
		}
    }
}