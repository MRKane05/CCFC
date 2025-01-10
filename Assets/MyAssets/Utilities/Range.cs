using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Range
{
	public float Min = 0;
	public float Max = 0;
	public Range(float min, float max)
	{
		Min = min;
		Max = max;
	}
	public float Get()
	{
		return UnityEngine.Random.Range(Min, Max);
	}

	public float GetLerp(float t)
	{
		return Mathf.Lerp(Min, Max, t);
	}

	public float FloorLerp(float t)
	{
		return Min + Mathf.Lerp(0, Max - Min, t);
	}

	public float GetSignedRandomInLerp(float t)
	{
		return UnityEngine.Random.Range(-GetLerp(t), GetLerp(t));
	}

	public float GetRandom()
	{
		return UnityEngine.Random.Range(Min, Max);
	}

	public bool ValueWithin(float value)
    {
		if (value > Min && value < Max)
        {
			return true;
        }
		return false;
    }
}