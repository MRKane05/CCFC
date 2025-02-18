using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is intended to be an instructional class that may/may not be replaced by an actual prop of similar size at runtime generation
public class BaseGen_PlaceholderObject : MonoBehaviour {
	public enum enPlaceholderSize {NULL, SINGLE, LONG, LARGE, ELBOW}
	public enPlaceholderSize placeholderSize = enPlaceholderSize.SINGLE;
}
