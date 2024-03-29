using UnityEngine;
using System.Collections;


public class SortingOrder : MonoBehaviour
{

	public int order;
	Renderer ren = null;

	void Start ()
	{
		ren = GetComponent<Renderer> ();
		if (ren != null)
			ren.sortingOrder = order;
	}
}
