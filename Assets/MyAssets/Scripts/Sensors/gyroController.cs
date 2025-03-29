using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.PSVita;

//test class for creating a gyro controller for aircraft input
public class gyroController : MonoBehaviour {
	private static gyroController instance = null;
	public static gyroController Instance { get { return instance; } }


	public TextMeshProUGUI logText;

	public Vector2 SmoothedPitchRoll = Vector2.zero;
	float gyroSmoothing = 45f;
	public GameObject cameraObject;
	Quaternion refernceQuat = Quaternion.identity;

    private void Awake()
    {
		Input.gyro.enabled = true;
        if (gyroController.Instance)
        {
			Debug.LogError("There's already a gyocontroller.instance present. You really fucked up somewhere");
        }
		instance = this;
    }

    // Use this for initialization
    void Start() {
		Input.gyro.updateInterval = 0.01f; // 1f / 30f;
	}

    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
			Calibrate(); //Quick hack to calibrate gyro by tapping screen
		}

		if (cameraObject)	//rotate our camera. We might need to have an option for this
        {
			cameraObject.transform.localEulerAngles = new Vector3(0, 0, SmoothedPitchRoll.y);
        }
	}

	public Quaternion GyroRaw()
    {
		return Input.gyro.attitude * refernceQuat;
    }

	public Vector2 GyroRollPitch()
    {
		//X: pitch starts at 0, positive is tilting back
		//Y: twist console. Right is positive
		//Z: "tilt" console. right is positive
		Vector3 gyroAngles = (Input.gyro.attitude * refernceQuat).eulerAngles;
		float pitch = wrapAngle(gyroAngles.x);
		float roll = wrapAngle(gyroAngles.y) + wrapAngle(gyroAngles.z);

		if (logText)
		{
			logText.text = (Input.gyro.attitude * refernceQuat).eulerAngles.ToString();
		}
		return new Vector2(roll, pitch);
    }

    // Update is called once per frame
    void FixedUpdate() {

		Vector2 DevicePitchRoll = getDeviceRollRitch();
		SmoothedPitchRoll = new Vector2(Mathf.Lerp(SmoothedPitchRoll.x, DevicePitchRoll.x, Mathf.Clamp01(Time.fixedDeltaTime * gyroSmoothing)), 
			Mathf.Lerp(SmoothedPitchRoll.y, DevicePitchRoll.y, Mathf.Clamp01(Time.fixedDeltaTime * gyroSmoothing)));
		//logText.text = DevicePitchRoll.ToString() + "\n" + AdjustedAccelerometer.ToString() + "\n" + (Input.gyro.attitude * Quaternion.Inverse(refernceQuat)).ToEulerAngles();
	}

	public Vector2 getDeviceRollRitch()
    {
		Vector3 accelerometerVector = AdjustedAccelerometer;
				
		float deviceRoll = loopAnglePosition(Mathf.Atan2(accelerometerVector.x, -accelerometerVector.z));
		float devicePitch = -loopAnglePosition(Mathf.Atan2(accelerometerVector.y, -accelerometerVector.z));
		//logText.text = accelerometerVector.ToString() + "\n" + deviceRoll.ToString() + ", " + devicePitch.ToString();
		return new Vector2(deviceRoll, devicePitch);
    }

	float loopAnglePosition(float angle)
    {
		angle *= 180f / Mathf.PI;
		if (angle > 180)
        {
			return angle - 360f;
        }
		return angle;
    }

	float wrapAngle(float angle)
    {
		if (angle > 180)
        {
			return angle - 360f;
        }
		return angle;
    }

	float shiftConvertAnglePosition(float angle)
    {
		//So the result of this math is that the numbers are -Pi*0.5f when resting, which is actually usable I think. It just needs a shift
		angle = angle * 180f / Mathf.PI;    //Convert to degrees
		angle -= 180;   //Offset to euler which we'll use
		return angle;
    }

	Matrix4x4 baseMatrix = Matrix4x4.identity;
	public void Calibrate()
	{
		Quaternion rotate = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), Input.acceleration);
		Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotate, new Vector3(1.0f, 1.0f, 1.0f));
		this.baseMatrix = matrix.inverse;

		refernceQuat = Quaternion.Inverse(Input.gyro.attitude);
	}


	public Vector3 AdjustedAccelerometer
	{
		get
		{
			return this.baseMatrix.MultiplyVector(Input.gyro.gravity);
		}
	}
}
