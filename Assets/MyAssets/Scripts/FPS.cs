using UnityEngine;
using System.Collections;
using UnityEngine.UI;
// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.

public class FPS : MonoBehaviour
{
	
	public float updateInterval = 1.0f;
	private float accum = 0.0f; // FPS accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private float fps = 15.0f; // Current FPS
	private double lastSample;
	private int gotIntervals = 0;
	public bool bMaxFrameSpeed =true;

	public Text displayText;

	void Start()
	{
		if (bMaxFrameSpeed)
			Application.targetFrameRate=60;
		else
			Application.targetFrameRate=30;

		timeleft = updateInterval;
		lastSample = Time.realtimeSinceStartup;
	}

	float GetFPS() { return fps; }
	bool HasFPS() { return gotIntervals > 2; }
	 
	void Update()
	{
		/*
		if(Input.GetKeyDown("'"))
			GetComponent<GUIText>().enabled = !GetComponent<GUIText>().enabled;
		*/
		
		if(true) //GetComponent<GUIText>().enabled)
		{
			++frames;
			float newSample = Time.realtimeSinceStartup;
			float deltaTime = newSample - (float)lastSample;
			lastSample = newSample;

			timeleft -= deltaTime;
			accum += 1.0f / deltaTime;
			
			// Interval ended - update GUI text and start new interval
			if( timeleft <= 0.0f )
			{
				// display two fractional digits (f2 format)
				fps = accum/frames;
				displayText.text = "FPS: " + fps.ToString("f2");
				timeleft = updateInterval;
				accum = 0.0f;
				frames = 0;
				++gotIntervals;
			}
		}
	}
}
