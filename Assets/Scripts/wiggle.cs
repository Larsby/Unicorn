
using UnityEngine;
using System.Collections;
using Medvedya.SpriteDeformerTools;


public class wiggle : MonoBehaviour
{



	//public Sprite sprite;
	//public Material material;
	SpriteDeformerStatic mySprite;
	private SpritePoint centerPoint;
	private bool isAnimating = false;
	float timeChange;
	float levveler;
	float spritepos;

	void Start ()
	{
		//init ();	
	}

	void init ()
	{
		if (mySprite == null) {
		
			mySprite = gameObject.GetComponent<SpriteDeformerStatic> ();
			//mySprite.sprite = gameObject.GetComponent<Sprite> ();
			//mySprite.material = material;
			//mySprite.SetRectanglePoints ();

		//	centerPoint = new SpritePoint (0.5f, 0.5f);
 		//	mySprite.AddPoint (centerPoint);

			Bounds b = mySprite.bounds;
			foreach (var item in mySprite.points) {
				b.Encapsulate ((Vector3)mySprite.SpritePositionToLocal (item.spritePosition));
			}
			mySprite.bounds = b;
			mySprite.UpdateMeshImmediate ();


			timeChange = Random.Range (12, 14);
			levveler = Random.Range (0.25f, .35f);
			spritepos = Random.Range (0.9f, 1.1f);


		}
		
	}

	public void setIsAnimating (bool inVar)
	{
		isAnimating = inVar;
		if (isAnimating) {
	
			//TweenSX.Add (gameObject, 0.5f, 0.5f).EaseInOutBack ();
			//TweenX.Add (gameObject, 0.2f, 0.55f).Relative ().EaseInOutBack (); 

		}
	}

	void Update ()
	{
		if (mySprite == null) {
			init ();
		

		} else if (isAnimating) {
 
			//centerPoint.offset2d = 
			//new Vector2 (Mathf.Cos (Time.time) * 0.3f, 
			//	Mathf.Sin (Time.time) * 0.3f);
			foreach (SpritePoint item in mySprite.points) {
				//item.spritePosition.x += Random.Range (-0.01f, 0.01f);

				//	Debug.Log ("item.spritePosition.x:" + item.spritePosition.x);
				item.spritePosition.y += (Mathf.Sin (Time.time * timeChange) * levveler) * (spritepos - item.spritePosition.x);
				 
			 
			}

			mySprite.dirty_offset = true;

		 
		}
	}

}
