using System;
using UnityEngine;

	public class PingPongVectorUtility
	{
	public float secondsForOneLengthX = 2f;
	public float secondsForOneLengthY = 1f;
	public float secondsForOneLengthZ = 2f;
	public bool restricMovement = false;
	public Vector2 upRestriction;
	public Vector2 downRestriction; 


	float timeToremove = 0;
	float startTime;
		public PingPongVectorUtility ()
		{
		}
	public Vector3 PingPong (Vector3 from, Vector3 to)
	{
		

			float x = Mathf.Lerp (from.x, to.x,
				Mathf.SmoothStep (0f, 1f,
					Mathf.PingPong ((Time.time - timeToremove) / secondsForOneLengthX, 1f)
				));

			float y = Mathf.Lerp (from.y, to.y,
				Mathf.SmoothStep (0f, 1f,
					Mathf.PingPong ((Time.time - timeToremove) / secondsForOneLengthY, 1f)
				));
		float z = Mathf.Lerp (from.z, to.z,
			Mathf.SmoothStep (0f, 1f,
				Mathf.PingPong ((Time.time - timeToremove) / secondsForOneLengthZ, 1f)
			));
		Vector3 result = new Vector3 (x, y, z);
		return result;
		} 

}

