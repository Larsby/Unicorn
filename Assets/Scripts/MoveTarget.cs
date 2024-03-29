using UnityEngine;
using System.Collections;

public class MoveTarget : MonoBehaviour
{
	//public Transform farEnd;
	private Vector3 frometh;
	private Vector3 untoeth;
	private float secondsForOneLength = 2f;

	void Start ()
	{
		frometh = transform.position;
		untoeth = new Vector3 (frometh.x, frometh.y - 2, frometh.z);
	}

	void Update ()
	{
		transform.position = Vector3.Lerp (frometh, untoeth,
			Mathf.SmoothStep (0f, 1f,
				Mathf.PingPong (Time.time / secondsForOneLength, 1f)
			));
	}
}