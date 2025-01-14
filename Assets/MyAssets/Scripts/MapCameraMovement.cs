using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A quick script to give us some movement for the camera, it'll need refinement, but this is a place to start at least
public class MapCameraMovement : MonoBehaviour {
    Vector3 basePosition = Vector3.zero;
    public bool bDoingLerp = false;
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

        //For zooming in/out (keep in mind this will be very broken)
        //I'd prefer to relegate this to the shoulders and have a select arrow for the map, but for the moment this'll do
        //transform.position -= transform.forward * Input.GetAxis("Right Stick Vertical") * Time.deltaTime * 5f;
        if (Input.GetButton("Right Shoulder")) {
            transform.position += transform.forward * Time.deltaTime * 5f;
        } else if (Input.GetButton("Left Shoulder"))
        {
            transform.position -= transform.forward * Time.deltaTime * 5f;
        }
    }
}
