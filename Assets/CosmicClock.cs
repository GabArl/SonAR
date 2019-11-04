using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmicClock : MonoBehaviour
{
	public bool doUpdate = true;
	public bool doTick = false;

	[Range(0, 59)]
	public int sec = 0;
	[Range(0, 59)]
	public int min = 0;
	[Range(0, 23)]
	public int hour = 0;
	private string day;

	private GameObject anchorSec, anchorMin, anchorHour;
	private GameObject objSec, objMin, objHour;

	public Mesh mesh;
	public AK.Wwise.Event e_sec, e_min, e_hour;
	public AK.Wwise.RTPC rtpc_sec, rtpc_min, rtpc_hour;


	public int Second
	{
		get
		{
			return sec;
		}

		set
		{
			if (sec == value)
				return;
			sec = value;
			ClockSecond();
			//sec.text = "Damage: " + sec;
		}
	}
	public int Minute
	{
		get
		{
			return min;
		}

		set
		{
			if (min == value)
				return;
			min = value;
			ClockMinute();
		}
	}
	public int Hour
	{
		get
		{
			return hour;
		}

		set
		{
			if (hour == value)
				return;
			hour = value;
			ClockHour();
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		CreateClock();
		e_hour.Post(objHour);
	}

	[ContextMenu("Create Clock")]
	public void CreateClock()
	{
		anchorSec = new GameObject();
		anchorSec.transform.SetParent(transform);
		anchorSec.name = "anchorSec";
		objSec = new GameObject();
		objSec.transform.SetParent(anchorSec.transform);
		objSec.transform.localPosition = new Vector3(1f, 0f, 0f);
		objSec.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		objSec.name = "objSec";
		objSec.AddComponent<MeshRenderer>();
		objSec.AddComponent<MeshFilter>();
		objSec.GetComponent<MeshFilter>().mesh = mesh;

		anchorMin = new GameObject();
		anchorMin.transform.SetParent(transform);
		anchorMin.name = "anchorMin";
		objMin = new GameObject();
		objMin.transform.SetParent(anchorMin.transform);
		objMin.transform.localPosition = new Vector3(0.5f, 0f, 0f);
		objMin.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
		objMin.name = "objMin";
		objMin.AddComponent<MeshRenderer>();
		objMin.AddComponent<MeshFilter>();
		objMin.GetComponent<MeshFilter>().mesh = mesh;

		anchorHour = new GameObject();
		anchorHour.transform.SetParent(transform);
		anchorHour.name = "anchorHour";
		objHour = new GameObject();
		objHour.transform.SetParent(anchorHour.transform);
		objHour.transform.localPosition = new Vector3(0.3f, 0f, 0f);
		objHour.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		objHour.name = "objHour";
		objHour.AddComponent<MeshRenderer>();
		objHour.AddComponent<MeshFilter>();
		objHour.GetComponent<MeshFilter>().mesh = mesh;
	}

	private void ClockSecond()
	{
		anchorSec.transform.localRotation = Quaternion.Euler(new Vector3(0, Second * 6, 20));
		AkSoundEngine.SetRTPCValue(rtpc_sec.Id, Second);
		e_sec.Post(objSec);
	}
	private void ClockMinute()
	{
		anchorMin.transform.localRotation = Quaternion.Euler(new Vector3(0, Minute * 6, 10));
		AkSoundEngine.SetRTPCValue(rtpc_min.Id, Minute);
		e_min.Post(objMin);
	}
	private void ClockHour()
	{
		anchorHour.transform.localRotation = Quaternion.Euler(new Vector3(0, Hour * 15, 0));
		AkSoundEngine.SetRTPCValue(rtpc_hour.Id, Hour);
	}

	public void Tick()
	{
		if (anchorSec == null) return;
		else Second = System.DateTime.UtcNow.Second;
		if (anchorMin == null) return;
		else Minute = System.DateTime.UtcNow.Minute;
		if (anchorHour == null) return;
		else Hour = System.DateTime.UtcNow.Hour;
		//if (anchorDay == null) return;
		day = System.DateTime.UtcNow.DayOfWeek.ToString();
	}

	// Update is called once per frame
	void Update()
	{
		if (doUpdate) Tick();
		if (doTick)
		{
			doTick = false;
			ClockSecond();
			ClockMinute();
			ClockHour();
		}
	}
}
