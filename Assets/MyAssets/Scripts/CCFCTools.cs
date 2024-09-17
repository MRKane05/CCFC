using UnityEngine;
using System.Collections;

//A collection of tools used for stuff in CCFC
public class CCFCTools : MonoBehaviour {
	static public T FindInParents<T> (Transform trans) where T : Component
	{
		if (trans == null) return null;

		T comp = trans.GetComponent<T>();

		if (comp == null)
		{
			Transform t = trans.transform.parent;
			
			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}

		return comp;
	}
}
