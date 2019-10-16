using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceMover : MonoBehaviour
{

	//public Dictionary<int, GameObject> layers = new Dictionary<int, GameObject>();
	public GameObject[] layers;

	private float step = 0;


	[Range(-360, 360)]
	public float dps_rotate = 0;
	[Range(-360, 360)]
	public float dps_rotateTo = 0;

	public Transform target;

	[Range(-360, 360)]
	public float chord_deg = 0;
	private float chord_frq = 0;

	private float timeCount = 0.0f;


	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{


		//rotateTowards();
		foreach (GameObject layer in layers)
		{
			rotate(layer);

			//lookRotation(layer);

			//rotateTowards(layer);

			slerp(layer);

			//fromToRotation(layer);

			//angle(layer);

			euler(layer);
		}

		//Quaternion.Euler(transform.rotation, transform.rotation, transform.rotation);
		//.SetEulerRotation(layer1.transform.eulerAngles.x, layer1.transform.eulerAngles.y +1, layer1.transform.eulerAngles.z);
	}

	void rotate(GameObject obj)
	{
		step = dps_rotate * Time.deltaTime;
		obj.transform.rotation *= Quaternion.AngleAxis(step, Vector3.up);
	}

	void rotateTowards(GameObject obj)
	{
		step = dps_rotateTo * Time.deltaTime;
		obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, target.rotation, step);
	}

	void lookRotation(GameObject obj)
	{
		Vector3 relativePos = target.position - obj.transform.position;

		obj.transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);
	}

	void fromToRotation(GameObject obj)
	{
		// usualy use this so an axis follows a target direction
		obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, obj.transform.forward);
	}

	void angle(GameObject obj)
	{

		Quaternion.Angle(obj.transform.rotation, target.transform.rotation);
	}

	void euler(GameObject obj)
	{
		obj.transform.rotation *= Quaternion.Euler(0, chord_deg, 0);
	}



	void slerp(GameObject obj)
	{
		obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, target.rotation, timeCount); // from , to , time
		timeCount = timeCount + Time.deltaTime;
	}

}


