using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
//make angular speed uniform
//scale / bend objects with elevation
//kill old int_chord
//on full chord: one jumps to start
//line:148 out of range if spammed

public class MusicMover : MonoBehaviour
{
	private List<int> keys = new List<int>();
	private Chord currentChord = new Chord();
	private List<Chord> chords = new List<Chord>();
	public LineRenderer line = new LineRenderer();
	private GameObject start;

	public GameObject keyboard;
	public List<Button> key_buttons = new List<Button>();

	//private List<Chord> chords = new List<Chord>();

	public List<Tone> objectsMoving = new List<Tone>();

	public float diameter = 4;
	public Material materialObj, materialHighlight, materialLine;
	public Mesh mesh;
	public Vector3 scale;
	private bool chordFull = false;
	private bool tonesMoving = false;
	private bool chordsMoving = false;

	private float lastInputTime;
	private bool hasInput = false;
	private bool startFromZero = false;
	[Range(0, 11)]
	public int keyOfScale = 0;
	private int toneAngle = 30;

	private int numberOfNotes = 4;

	private float chordLength0, chordLength1, chordLength2;

	public AK.Wwise.Event startEvent, stopEvent, longStopEvent;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone;

	[Range(20f, 300f)]
	public float turningRate = 150f;

	private float tapsPerSecond;
	private List<float> taps = new List<float>();
	public Text bpmText;

	public GameObject camera;

	void Start()
	{
		Create();

		foreach (Button keybutton in keyboard.GetComponentsInChildren<Button>())
		{
			key_buttons.Add(keybutton);
		}

		line.enabled = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.1f;
		line.material = materialLine;
	}

	private void BPM()
	{
		for (int i = 0; i < taps.Count; i++)
		{
			if (taps[i] <= Time.timeSinceLevelLoad - 60)
			{
				taps.Remove(taps[i]);
			}
		}
		tapsPerSecond = taps.Count;
		bpmText.text = "BPM: " + tapsPerSecond;
	}

	[ContextMenu("Create Objects")]
	public void Create()
	{
		for (int i = 0; i < 30; i++)
			taps.Add(Time.timeSinceLevelLoad);

		line.positionCount = numberOfNotes;
		Vector3 pos = new Vector3(diameter / 2f, 0.5f, 0);
		start = new GameObject();
		start.transform.position = pos;

		for (int i = 0; i < numberOfNotes; i++)
		{
			Tone t = CreateTone();
			currentChord.tones.Add(t);
			line.SetPosition(currentChord.tones[i].number, currentChord.tones[i].obj.transform.position);
		}
	}

	private Tone CreateTone()
	{
		Tone tone = new Tone(start.transform.localPosition, gameObject);
		tone.obj.GetComponent<MeshFilter>().mesh = mesh;
		tone.obj.GetComponent<MeshRenderer>().material = materialObj;
		tone.obj.transform.localScale = scale;
		tone.obj.SetActive(false);
		return tone;
	}
	private Chord CopyChord(Chord chord)
	{
		Chord copyChord = new Chord();
		foreach (Tone toneIn in chord.tones)
		{
			Tone copyTone = new Tone(start.transform.localPosition, gameObject);
			copyTone.semitone = toneIn.semitone;
			copyTone.anchor.transform.localPosition = toneIn.anchor.transform.localPosition;
			copyTone.anchor.transform.localRotation = toneIn.anchor.transform.localRotation;
			copyTone.obj.transform.localPosition = toneIn.obj.transform.localPosition;
			copyTone.obj.transform.localRotation = toneIn.obj.transform.localRotation;
			copyTone.obj.GetComponent<MeshFilter>().mesh = mesh;
			copyTone.obj.GetComponent<MeshRenderer>().material = materialObj;
			copyTone.obj.transform.localScale = scale;
			copyChord.tones.Add(copyTone);
		}
		return copyChord;

	}

	private void ResetTones()
	{
		lastInputTime = 0;
		hasInput = false;
		chordFull = false;
		tonesMoving = false;
		keys.Clear();
		foreach (Tone tone in currentChord.tones)
		{
			tone.isMoving = false;
			tone.obj.SetActive(false);
		}
		foreach (Button keybutton in key_buttons)
		{
			keybutton.interactable = true;
		}
	}

	public void MoveChords()
	{
		foreach (Chord chord in chords)
		{
			foreach (Tone tone in chord.tones)
			{
				tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate * 3);

				if (tone.anchor.transform.localRotation == tone.targetRotation)
				{
					chordsMoving = false;
				}
			}
		}
	}
	public void MoveTones()
	{
		foreach (Tone tone in currentChord.tones)
		{
			tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate);
			line.SetPosition(tone.number, tone.obj.transform.position);

			if (tone.anchor.transform.localRotation == tone.targetRotation)
			{
				tonesMoving = false;
			}
		}
	}
	private bool PushChord(Chord chord)
	{
		if (chord.tones[0].anchor.transform.localEulerAngles.z + 10 <= 80) // <--- out of range
		{
			Tone baseTone = new Tone(start.gameObject.transform.position, gameObject);
			foreach (Tone tone in chord.tones)
			{
				tone.targetRotation = tone.anchor.transform.localRotation;
				tone.targetRotation *= Quaternion.Euler(0, 0, 10);
				Vector3 scale = tone.obj.transform.localScale;
				tone.obj.transform.localScale = new Vector3(scale.x, scale.y * 0.7f, scale.z);
			}
			chord.tones.Sort((t1, t2) => t1.semitone.CompareTo(t2.semitone));
			chord.tones[0].obj.GetComponent<MeshRenderer>().material = materialHighlight;

			foreach (Button keybutton in key_buttons)
			{
				keybutton.interactable = true;
			}
			return true;
		}
		else return false;
	}

	private void DestroyChord(Chord chord)
	{
		foreach (Tone tone in chord.tones)
		{
			Destroy(tone.obj);
			Destroy(tone.anchor);
		}
		chord.tones.Clear();
		chords.Remove(chord);
	}

	public void Update()
	{
		BPM();

		Input.gyro.enabled = true;
		
		//Input.location.lastData.
	
		//camera.transform.localPosition += Input.gyro.userAcceleration.normalized;
		//camera.transform.localRotation = Input.gyro.attitude;
		//camera.transform.localRotation *= Quaternion.Euler(0,90,90);
		//GoogleARCore.

		if (chordsMoving)
		{
			MoveChords();
		}
		if (tonesMoving)
		{
			MoveTones();
		}
		if (!tonesMoving && hasInput && Time.time - lastInputTime > 3)
		{
			ResetTones();
		}
		if (objectsMoving.Count == 0)
		{
			tonesMoving = false;

			if (chordFull)
			{
				chordsMoving = true;
				chordFull = false;
				chords.Add(CopyChord(currentChord));
				keys.Clear();

				for (int i = 0; i < chords.Count; i++) // for-loop because Destroy()
				{
					if (!PushChord(chords[i]))
						DestroyChord(chords[i]);
				}

				foreach (Tone tone in currentChord.tones)
				{
					AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj);
					AkSoundEngine.PostEvent(longStopEvent.Id, tone.obj);
					tone.obj.SetActive(false);
				}
			}
		}
		else
		{
			foreach (Tone tone in currentChord.tones)
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
						tone.chordLength = Vector3.Distance(tone.obj.transform.position, currentChord.tones[tone.number - 1].obj.transform.position);
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
		}
	}

	public void keyPressed(int keyNum)
	{
		if (keys.Count == numberOfNotes)
		{
			foreach (Tone tone in currentChord.tones)
			{
				AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
			}
		}
		AddKeyToChord(keyNum);  // <-- wonky stability, no catch from ChordFull / ToneInactive
	}

	private void AddKeyToChord(int keyNum)
	{
		taps.Add(Time.timeSinceLevelLoad);

		hasInput = true;
		lastInputTime = Time.time;
		currentChord.tones[keys.Count].semitone = keyNum;
		currentChord.tones[keys.Count].targetRotation = Quaternion.Euler(0, (keyNum + keyOfScale) * toneAngle, 0);
		currentChord.tones[keys.Count].isMoving = true;

		if (keys.Count == 0 || startFromZero)
			currentChord.tones[keys.Count].anchor.transform.localRotation = Quaternion.Euler(0, keyOfScale * toneAngle, 0);
		else
			currentChord.tones[keys.Count].anchor.transform.localRotation = currentChord.tones[keys.Count - 1].anchor.transform.localRotation;

		line.SetPosition(keys.Count, currentChord.tones[keys.Count].obj.transform.position);
		objectsMoving.Add(currentChord.tones[keys.Count]);
		currentChord.tones[keys.Count].obj.SetActive(true);
		AkSoundEngine.PostEvent(startEvent.Id, currentChord.tones[keys.Count].obj);
		tonesMoving = true;

		key_buttons.Find(x => x.name.Contains(keyNum.ToString())).interactable = false;
		//interactable = false;


		keys.Add(keyNum);
		if (keys.Count == numberOfNotes)
		{
			chordFull = true;
			return;
		}
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
public class Chord
{
	public List<Tone> tones = new List<Tone>();
	private float scale = 1f;
}
