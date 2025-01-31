using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A quick script to give us some movement for the camera, it'll need refinement, but this is a place to start at least
public class MapCameraMovement : MonoBehaviour {
    Vector3 basePosition = Vector3.zero;
    public bool bDoingLerp = false;

    public Range XLimits = new Range(-10, 10);
    public Range YLimits = new Range(3, 15);
    public Range ZLimits = new Range(-3, 6);
    void Start()
    {
        basePosition = gameObject.transform.position;
    }

    IEnumerator moveToPosition(Vector3 newPosition)
    {
        bDoingLerp = true;
        //Debug.Log(Vector3.SqrMagnitude(gameObject.transform.position - newPosition));
        while (Vector3.SqrMagnitude(gameObject.transform.position-newPosition) > 1f)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, newPosition, Time.unscaledDeltaTime * 10f);
            yield return null;
          //  Debug.Log("Doing Camera Movement");
        }
        //And after that we should be at our position so we can go about doing whatever we've got to do :)
        //Debug.Log("Finished Camera Move");
        bDoingLerp = false;
    }

    public void DoMoveToPosition(Vector3 targetPosition)
    {
        //Debug.Log("Doing Camera Move");
        StartCoroutine(moveToPosition(targetPosition));
    }

    //After showing everything we want to jump back to where we were to begin with
    public void returnToStart()
    {
        StartCoroutine(moveToPosition(basePosition));
    }


    void Update()
    {
        if (!bDoingLerp)
        {
            DoCameraMovement();
        }
    }

    void DoCameraMovement() { 
        //For moving our camera around the viewing plane
        transform.position += Vector3.forward * Input.GetAxis("Left Stick Vertical") * Time.deltaTime * 10f;
        transform.position -= Vector3.right * Input.GetAxis("Left Stick Horizontal") * Time.deltaTime * 10f;

        //Move with keyboard
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.right * Time.deltaTime * 10f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position -= Vector3.right * Time.deltaTime * 10f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position -= Vector3.forward * Time.deltaTime * 10f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.forward * Time.deltaTime * 10f;
        }

        //For zooming in/out (keep in mind this will be very broken)
        //I'd prefer to relegate this to the shoulders and have a select arrow for the map, but for the moment this'll do
        //transform.position -= transform.forward * Input.GetAxis("Right Stick Vertical") * Time.deltaTime * 5f;
        if (Input.GetButton("Right Shoulder") || Input.GetKey(KeyCode.Q)) {
            transform.position += transform.forward * Time.deltaTime * 5f;
        } else if (Input.GetButton("Left Shoulder") || Input.GetKey(KeyCode.E))
        {
            transform.position -= transform.forward * Time.deltaTime * 5f;
        }

        //Our transform position needs to be clamped somehow so that we can't exceed our map bounds and get lost
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, XLimits.Min, XLimits.Max),
            Mathf.Clamp(transform.position.y, YLimits.Min, YLimits.Max),
            Mathf.Clamp(transform.position.z, ZLimits.Min, ZLimits.Max));
    }
}
