using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A quick script to give us some movement for the camera, it'll need refinement, but this is a place to start at least
public class MapCameraMovement : MonoBehaviour {


    void Update()
    {
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
