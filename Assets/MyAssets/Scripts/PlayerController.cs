using UnityEngine;
using System.Collections;

public class PlayerController : ActorController {
	
	//This needs to be instanced to make things easier
	private static PlayerController instance = null;
	public static PlayerController Instance {get {return instance;}}
	
	public GameObject JoyStickPrefab;
	public GUIText[] JoyMessageTexts;
	//controls how the system responds to touch and turn information
	public AnimationCurve ControlSensitivityCurve = new AnimationCurve(new Keyframe(-1,-1), new Keyframe(0,0), new Keyframe(1,1));
	AnimationCurve InvertReturnCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(10, -1), new Keyframe(180, -1), new Keyframe(180.1F, 1), new Keyframe(350, 1), new Keyframe(360, 0));
	public Vector2 GraphicsSize = new Vector2(2048, 1550);
	public GUISkin HUDGUI;
	public Texture2D targetSight;
	public float targetSightSize=128;
	public Vector4 FireButtonPosition, FireButtonStustainArea;
	
	public Vector4[] JoyStickConstraints; //this will probably be duped for both examples
	GameObject[] ourJoySticks;
	VirtualJoystick[] ourJoyMP;
	//public Joystick[] ourJoyJS;
	//public AircraftController ourAircraft; //this is what we're attached to, setup by the aircraft itself on instantiation.
	public GameObject gunSightObject; //this is what our gun is sighted into at 50ft as they use to be
	public bool bLeftieFlip=false;
	public float Deadzone = 0.1F; //a percentage of movement that's considered to be a "deadzone"
	public float YControl=1; //a multiplier used to invert y input
	public Rect GUIRect;
	public bool bFiring=false;
	float RollReturnTime, RollReturnWait = 0.7F;
	int FirePressID=-1;
	Quaternion returnAngles;
	Camera ourCamera;
	int FireState=0; //0: nothing happening, 1: started firing, 2: Ongoing firing.
	
	public float mAltitude;

	//Player input preferences
	public int yAxisBias = -1;


	void Start () {
		
		
		if (instance)
		{
			Debug.Log("Duplicate attempt to create PlayerController");
			Destroy(this);
		}
		
		instance = this;
		
		
		//Adjust our targetSight size to be compatiable with our screensize
		//targetSightSize = targetSightSize*Screen.width/GraphicsSize[0];
		
		ourCamera=Camera.main;
		
		if (Deadzone!=0) { 
			SetupInputCurves();
		}
		//deal with our positions if we've got a leftie
		if (bLeftieFlip) {
			
		}
		
		
		//Boot up our joysticks
		
		ourJoySticks = new GameObject[JoyStickConstraints.Length];
		ourJoyMP = new VirtualJoystick[JoyStickConstraints.Length];
		for (int i=0; i<JoyStickConstraints.Length; i++) { //go through and assign our sticks!
			ourJoySticks[i] = Instantiate(JoyStickPrefab, transform.position, transform.rotation) as GameObject;
			ourJoyMP[i] = ourJoySticks[i].GetComponent<VirtualJoystick>();
			ourJoyMP[i].transform.parent = transform;
			ourJoyMP[i].SetupStick(JoyStickConstraints[i]); //and finally set our stick up so that it's "real"
			ourJoyMP[i].ourGUIText = JoyMessageTexts[i];
		}
		
	
	}
	
	//This is called after health damage is taken
	public override void TakeDamage(float thisDamage, string damageType, GameObject instigator, float delay) {
		float damageProp = ourAircraft.health/ourAircraft.maxHealth;
		
		//update our health bar
		NGUI_Base.Instance.assignHealth(damageProp);

		//remember to jolt the camera
		
		
	} //really just a notification class that can be passed up to the AI

	
	void SetupInputCurves() { //used for our control sensitivity
		Keyframe[] ks = new Keyframe[4];
		ks[0]=new Keyframe(-1,-1); //this and the last one really need some fancy tangents going on.
		ks[0].outTangent = 2.5F; //should be calculated as a funciton of the deadzone.
		ks[1]=new Keyframe(-Deadzone, 0);
		ks[2]=new Keyframe(Deadzone, 0);
		ks[3]=new Keyframe(1,1);
		ks[3].inTangent = 2.5F;
		ControlSensitivityCurve = new AnimationCurve(ks);
	}
	
	void SetupControlObjects(bool bIsLeft) {
		//switch over the stuff that we need to...
		
	}
	
	Rect MakeGUIRect(Vector4 ComparativePosition, Vector2 ComparativeScreenSize) {
		return new Rect((ComparativePosition[0]*Screen.width)/ComparativeScreenSize[0], (ComparativePosition[1]*Screen.height)/ComparativeScreenSize[1], (ComparativePosition[2]*Screen.width)/ComparativeScreenSize[0], (ComparativePosition[3]*Screen.height)/ComparativeScreenSize[1]);
		
	}

	public override void targetCallback(Actor newTargetController, GameObject newTarget, int flightProcess) {
		//quick setup stuff to make sure we're all clean with the targeting.
		if (targetController) {
			targetController.bIsTarget=false;
		}

		if (newTarget) {
			//this is a great place to send a call through to "frame" our necessary target
			//Or could do it on the radar itself...

			//we'll need to rig up the different parts. Not totally sure how it'll all mesh together.
			target = newTarget;
			targetController = newTargetController;
			targetController.bIsTarget=true; //make this into our target

			//Debug.Log ("Got Target Callback");
			NGUI_Base.Instance.setTarget(newTarget);
		}
		else { //set this to the waypoint or whatever it's meant to be.
			target = null;
			if (targetController)
				targetController.bIsTarget = false; //release this target
			targetController = null;

			//NGUI_Base.Instance.setTarget (LevelController.Instance.getMissionEndGoal); //we don't really want this, the other systems should handle it
		}
	
	}


	//Set this up with NGUI
	void FireButton() {
		bFiring=true;
	}
	
	void FireRelease() {
		bFiring=false;
	}

	// Update is called once per frame
	void Update() {

		//base function...make sure we've a target if there are targets (auto switch)
		if (!target) {
			if (LevelController.Instance.enemyList.Count > 0) {
				LevelController.Instance.requestTarget(this); //populate our target system
			}
		}
		else { //we've got a target
			if (targetController.bIsDead && LevelController.Instance.enemyList.Count > 1) {
				LevelController.Instance.requestTarget(this); //we tagged out our prior fighter, call a new target
			}

			//is getting an error when the enemies run out...
			if (targetController != null) {
				if (targetController.bIsDead && LevelController.Instance.enemyList.Count == 1) { //this is the last enemy on the system
					target = null;
					targetController.bIsTarget = false; //cleanup
					targetController = null;

					NGUI_Base.Instance.setTarget(null);
				}
			}
		}

		//sundry stuff for the players convience (things like updating the altitude etc.
		if (ourAircraft) {
			mAltitude = ourAircraft.checkAltitude();
		}



		/*
		//case we've got the controller shot out
		if (targetController) {
			if (targetController.bIsDead && LevelController.Instance.enemyList.Count > 0) {
				LevelController.Instance.requestTarget(this); //call something if we've lost this target (to discourage players from drilling something out)
			}
			else { //send through that we're discarding our targets
				target = null;
				targetController.bIsTarget = false; //regardless of it being destroyed tell it that it's off the hook
				targetController = null;
			}
		}
		*/
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		handleInputControls();
#else
		handleVitaControls(); 
#endif
	}

	void handleVitaControls()
    {

		//Why did we do this again?
		if (Input.GetButtonDown("Right Shoulder"))
			FireState = 1;
		else if (Input.GetButton("Right Shoulder"))
			FireState = 2;
		else
			FireState = 0;

		if (Input.GetButton("Right Shoulder"))
			bFiring = true;
		else
			bFiring = false;

		ourAircraft.UpdateInput(yAxisBias*Input.GetAxis("Left Stick Vertical"), Input.GetAxis("Left Stick Horizontal"), Input.GetAxis("Right Stick Horizontal"), -Input.GetAxis("Right Stick Vertical"), bFiring, FireState);
	}

	void handleInputControls() { 

		//Quick keyboard interface override:

		//Not really checking for this at this stage
		if (Input.GetKeyDown(KeyCode.Space))
			FireState=1;
		else if (Input.GetKey(KeyCode.Space))
			FireState=2;
		else
			FireState=0;

		if (Input.GetKey(KeyCode.Space)) 
			bFiring=true;
		else
			bFiring=false;

		//Quick engine hack...
		float roll=0;
		float pitch=0;
		float yaw=0;
		if (Input.GetKey(KeyCode.LeftArrow)) {
			roll=-1;
		}
		else if (Input.GetKey(KeyCode.RightArrow)) {
			roll=1;
		}
		
		if (Input.GetKey(KeyCode.UpArrow)) {
			pitch=1;
		}
		else if (Input.GetKey(KeyCode.DownArrow)) {
			pitch=-1;
		}
		
		if (Input.GetKey(KeyCode.Z)) {
			yaw = -1;
		}
		else if (Input.GetKey(KeyCode.X)) {
			yaw = 1;
		}
		
		float throttleControl=0;
		if (Input.GetKey(KeyCode.D)) {
			throttleControl=1;
		}
		else if (Input.GetKey(KeyCode.C)) {
			throttleControl=-1;
		}
		
		
		//print (ourAircraft.transform.localEulerAngles);
		if (bControlArcade) { //then lookout for a "return" option
			if (Input.touchCount>0) {
				RollReturnTime = Time.time + RollReturnWait;
				ourAircraft.UpdateInput(ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[1]*YControl),ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[0]),ControlSensitivityCurve.Evaluate(ourJoyMP[1].VJRnormals[0]), ControlSensitivityCurve.Evaluate(ourJoyMP[1].VJRnormals[1]), bFiring, FireState);
			}
			else if (Time.time > RollReturnTime) {
				//a Transform.LookAt does the trick!
				returnAngles.SetLookRotation(gunSightObject.transform.position, Vector3.up);

				//Turned off for the quick engine hack...

				//ourAircraft.UpdateInput(ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[1]*YControl),ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[0]),InvertReturnCurve.Evaluate(Mathf.Repeat(returnAngles.eulerAngles[2]- ourAircraft.transform.eulerAngles[2],360)), ControlSensitivityCurve.Evaluate(ourJoyMP[1].VJRnormals[1]), bFiring, FireState);

				//editor input hack
				ourAircraft.UpdateInput(ControlSensitivityCurve.Evaluate(pitch),ControlSensitivityCurve.Evaluate(roll),InvertReturnCurve.Evaluate(Mathf.Repeat(returnAngles.eulerAngles[2]- ourAircraft.transform.eulerAngles[2],360)), throttleControl, bFiring, FireState);

			}
			else { //annol input for the controls.
				ourAircraft.UpdateInput(0,0,0,0,bFiring,FireState);
			}
		}
		
		//I've pretty much decided that this won't be happening and I'll stick to the arcade method of control
		if (!bControlArcade){ //update this accordingly.
			ourAircraft.UpdateInput(ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[1]*YControl),ControlSensitivityCurve.Evaluate(ourJoyMP[0].VJRnormals[0]),ControlSensitivityCurve.Evaluate(ourJoyMP[1].VJRnormals[0]), ControlSensitivityCurve.Evaluate(ourJoyMP[1].VJRnormals[1]), bFiring, FireState);
		}
	}
}