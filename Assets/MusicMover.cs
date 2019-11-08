using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMover : MonoBehaviour
{
	private List<Tone> tones = new List<Tone>();
	private List<int> chord = new List<int>();
	public LineRenderer line = new LineRenderer();
	private GameObject start;

	public List<Tone> objectsMoving = new List<Tone>();

	public float diameter = 4;
	public Material materialObj, materialLine;
	public Mesh mesh;
	public Vector3 scale;
	private bool chordFull = false;

	private int numberOfNotes = 4;

	private float chordLength0, chordLength1, chordLength2;

	public AK.Wwise.Event startEvent, stopEvent, longStopEvent;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone;

	[Range(0f, 200f)]
	public float turningRate = 100f;


	void Start()
	{
		Create();

		line.enabled = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.1f;
		line.material = materialLine;
	}

	[ContextMenu("Create Objects")]
	public void Create()
	{
		line.positionCount = numberOfNotes;
		Vector3 pos = new Vector3(diameter / 2f, 0.5f, 0);
		start = new GameObject();
		start.transform.position = pos;

		for (int i = 0; i < line.positionCount; i++)
		{
			tones.Add(new Tone(pos, gameObject));
			tones[i].obj.GetComponent<MeshFilter>().mesh = mesh;
			tones[i].obj.GetComponent<MeshRenderer>().material = materialObj;
			tones[i].obj.transform.localScale = scale;
			line.SetPosition(tones[i].number, tones[i].obj.transform.position);
		}
	}

	public void Update()
	{
		foreach (Tone tone in tones)
		{
			if (tone.isMoving)
			{
				tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate);
				line.SetPosition(tone.number, tone.obj.transform.position);
				if (tone.number == 0)
				{
					tone.chordLength = Vector3.Distance(start.transform.position, tone.obj.transform.position);
					AkSoundEngine.SetRTPCValue(
						rtpc_chordLength.Id,
						Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, diameter, tone.chordLength)),
						tone.obj);
				}
				else
				{
					tone.chordLength = Vector3.Distance(tone.obj.transform.position, tones[tone.number - 1].obj.transform.position);
					AkSoundEngine.SetRTPCValue(
					  rtpc_chordLength.Id,
					  Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, diameter, tone.chordLength)),
					  tone.obj);
				}

				if (tone.anchor.transform.localRotation == tone.targetRotation)
				{
					tone.isMoving = false;
					objectsMoving.Remove(tone);
					AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj);
					AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
				}
			}
		}
		if (chordFull && objectsMoving.Count == 0)
		{
			foreach (Tone tone in tones)
			{
				AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj);
				AkSoundEngine.PostEvent(longStopEvent.Id, tone.obj);
			}
			chord.Clear();
			chordFull = false;
		}
	}

	public void keyPressed(int keyNum)
	{
		if (chord.Count == numberOfNotes) // geht out-of-range wenn gespammt wird
		{
			foreach (Tone tone in tones)
			{
				AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
			}
		}
		addKeyToChord(keyNum);
	}

	private void addKeyToChord(int keyNum)
	{
		tones[chord.Count].targetRotation = Quaternion.Euler(0, keyNum * 30, 0);
		tones[chord.Count].semitone = keyNum;
		tones[chord.Count].isMoving = true;
		objectsMoving.Add(tones[chord.Count]);

		line.SetPosition(chord.Count, tones[chord.Count].obj.transform.position);
		AkSoundEngine.PostEvent(startEvent.Id, tones[chord.Count].obj);
		chord.Add(keyNum);

		if (chord.Count == numberOfNotes)
			chordFull = true;
	}
}

public class Tone
{
	public static int counter;
	public int number;
	public float semitone;
	public bool isMoving = false;
	public float chordLength;
	public GameObject anchor, obj;
	public Quaternion targetRotation = Quaternion.identity;
	public Vector3 scale;

	public Tone(Vector3 position, GameObject parent)
	{
		anchor = new GameObject();
		anchor.name = "anchor_" + counter;
		anchor.transform.SetParent(parent.transform);

		obj = new GameObject();
		obj.name = "tone_" + counter;
		obj.transform.SetParent(anchor.transform);
		obj.transform.localPosition = position;

		obj.AddComponent<AkGameObj>();
		obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshFilter>();

		number = counter;
		counter++;
	}
}
