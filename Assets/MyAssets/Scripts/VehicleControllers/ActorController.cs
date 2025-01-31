using UnityEngine;
using System.Collections;

public class ActorController : MonoBehaviour {
	public Actor ourAircraft;

	public bool bControlArcade=true; //are we using an arcade input style?
	public int team=-1;
	
	protected LevelController levelLink;
	//our targeting stuff
	public GameObject target;
	public Actor targetController;

	public string ourFlightGroup; //can be anything, but is usually something we use for sending messages through the chain of AI
	public string flightGroup {
		get { return ourFlightGroup; }
		set { ourFlightGroup = value; }
	}

	#region AIForwardValues
	//public GameObject followTarg;
	//public string pattern="ATTACK", patternStage="SETUP";
	#endregion

	// Use this for initialization
	void Start () {
	
	}
	
	void Awake() {
		levelLink = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();	
	}
	
	#region forwardCalls
	public virtual void getNotification(string thisMessage, string thisTag) { }

	public virtual void targetCallback(Actor newTargetController, GameObject newTarget, int flightProcess) { }

	public virtual void setPatrol(float awakeTime) { }
	#endregion

	//Cleanup our stuff from the links etc
	void OnDestroy() { 
		//Clean out our information from levelLink
		//levelLink.removeSelf(gameObject, team);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public virtual void TakeDamage(float thisDamage, string damageType, GameObject instigator, float delay) {} //really just a notification class that can be passed up to the AI
}
