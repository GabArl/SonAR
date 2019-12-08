using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO
// make angular speed uniform
// fallback no arcore / no camera allowed
// cleanup gameobjects reaching top
// Time.timeScale
// Tone on 0 not moving

public class MusicMover : MonoBehaviour
{
	private List<int> keys = new List<int>();
	private Chord currentChord = new Chord();
	private List<Chord> chords = new List<Chord>();
	private GameObject start;

	public GameObject keyboard;
	public List<Button> key_buttons = new List<Button>();

	//private List<Chord> chords = new List<Chord>();

	private int count_moving_tones, count_moving_chords;

	public float diameter = 4;
	public Material materialObj, materialHighlight, materialLine;
	public Mesh mesh;
	public Vector3 scale;
	private bool chordFull = false, tonesMoving = false, chordsMoving = false;

	private float lastInputTime;
	private bool hasInput = false;
	private bool startFromZero = false;

	private float toneAngle = 30f;
	public int keyOfScale = 0, numberOfNotes = 4;

	public AK.Wwise.Event startEvent, stopEvent, longStopEvent;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone;

	[Range(20f, 300f)]
	public float turningRate = 150f;

	private float tapsPerSecond;
	private List<float> taps = new List<float>();
	public Text bpmText;

	public AK.Wwise.RTPC length_to_origin;
	public AK.Wwise.RTPC length_to_last;


	void Start()
	{
		Create();

		foreach (Button keybutton in keyboard.GetComponentsInChildren<Button>())
		{
			key_buttons.Add(keybutton);
		}
	}

	void Update()
	{
		MoveTones();
		MoveChords();

		if (chordFull && !tonesMoving && !chordsMoving)
		{
			FinishChord();
			ResetTones();
		}
		if (!tonesMoving && !chordsMoving && hasInput && Time.time - lastInputTime > 3)
		{
			ResetTones();
		}

		UpdateLines();
		BPM();
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
		currentChord = new Chord();
		currentChord.tones = new List<Tone>();

		for (int i = 0; i < 30; i++)
			taps.Add(Time.timeSinceLevelLoad);

		Vector3 pos = new Vector3(diameter / 2f, 0.5f, 0);
		start = new GameObject();
		start.transform.position = pos;

		for (int i = 0; i < numberOfNotes; i++)
		{
			currentChord.tones.Add(CreateTone());
		}
	}
	private Tone CreateTone()
	{
		Tone tone = new Tone(start.transform.localPosition, gameObject);
		tone.obj.GetComponent<MeshFilter>().mesh = mesh;
		tone.obj.GetComponent<MeshRenderer>().material = materialObj;
		tone.obj.transform.localScale = scale;
		tone.obj.SetActive(false);
		tone.line.material = materialLine;
		tone.line.positionCount = numberOfNotes;
		return tone;
	}

	private void MoveChords()
	{
		if (!chordsMoving) return;

		foreach (Chord chord in chords)
		{
			foreach (Tone tone in chord.tones)
			{
				if (tone.anchor.transform.localRotation == tone.targetRotation)
				{
					count_moving_chords--;
					AkSoundEngine.PostEvent(longStopEvent.Id, tone.obj);
					continue;
				}
				tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate);
			}
		}
		if (count_moving_chords <= 0)
			chordsMoving = false;
	}
	private void MoveTones()
	{
		if (!tonesMoving) return;

		foreach (Tone tone in currentChord.tones)
		{
			if (tone.isMoving)
			{
				if (tone.anchor.transform.localRotation == tone.targetRotation)
				{
					count_moving_tones--;
					tone.isMoving = false;					
					AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
					continue;
				}
				tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate);
			}
		}
		if (count_moving_tones == 0)
			tonesMoving = false;
	}
	private void ResetTones()
	{
		lastInputTime = Time.time;
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

	private void UpdateLines()
	{
		Vector3 origin = start.transform.position;
		Vector3 self;
		Vector3 last;
		Tone tempTone;

		for (int i = 0; i < currentChord.tones.Count; i++)
		{
			tempTone = currentChord.tones[i];
			self = tempTone.obj.transform.position;

			if (i == 0)
				last = self;
			else
				last = currentChord.tones[i - 1].obj.transform.position;

			tempTone.line.SetPosition(0, origin);
			tempTone.line.SetPosition(1, self);
			tempTone.line.SetPosition(2, last);

			tempTone.length_to_origin = Vector3.Distance(origin, self);
			tempTone.length_to_last = Vector3.Distance(last, self);

			AkSoundEngine.SetRTPCValue(
				 length_to_origin.Id,
				 Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, diameter, tempTone.length_to_origin)),
				 currentChord.tones[i].obj);

			AkSoundEngine.SetRTPCValue(
				 length_to_last.Id,
				 Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, diameter, tempTone.length_to_last)),
				 currentChord.tones[i].obj);

			currentChord.tones[i] = tempTone;
		}
	}

	private void FinishChord()
	{
		chordFull = false;
		chordsMoving = true;
		chords.Add(CopyChord(currentChord));
		keys.Clear();
		count_moving_chords = chords.Count;

		for (int i = 0; i < chords.Count; i++) // for-loop because Destroy()
		{
			if (!PushChord(chords[i]))
				DestroyChord(chords[i]);
		}

		foreach (Tone tone in currentChord.tones)
		{
			tone.obj.SetActive(false);
		}

		foreach (Button button in key_buttons)
			button.interactable = false;
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
				tone.obj.transform.localScale = new Vector3(scale.x, scale.y * 0.7f, scale.z * 0.8f);
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

	public void keyPressed(int keyNum)
	{
		if (!chordFull)
			AddKeyToChord(keyNum);
	}
	private void AddInputTap()
	{
		hasInput = true;
		taps.Add(Time.timeSinceLevelLoad);
		lastInputTime = Time.time;
	}
	public void AddKeyToChord(int keyNum)
	{
		Tone tone = currentChord.tones[keys.Count];

		tone.obj.SetActive(true);
		tone.isMoving = true;
		tone.semitone = keyNum;
		tone.targetRotation = Quaternion.Euler(0, (keyNum + keyOfScale) * toneAngle, 0);

		if (keys.Count == 0 || startFromZero)
			tone.anchor.transform.localRotation = Quaternion.Euler(0, keyOfScale * toneAngle, 0);
		else
			tone.anchor.transform.localRotation = currentChord.tones[keys.Count - 1].anchor.transform.localRotation;

		currentChord.tones[keys.Count] = tone;

		AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, currentChord.tones[keys.Count].semitone, currentChord.tones[keys.Count].obj);
		AkSoundEngine.PostEvent(startEvent.Id, currentChord.tones[keys.Count].obj);

		count_moving_tones++;
		tonesMoving = true;
		keys.Add(keyNum);
		AddInputTap();
		key_buttons.Find(x => x.name.Contains(keyNum.ToString())).interactable = false;

		if (keys.Count == numberOfNotes)
		{
			chordFull = true;
			foreach (Button button in key_buttons)
				button.interactable = false;
		}
	}
}

public class Tone
{
	public static int counter;
	public int number;
	public float semitone;
	public float chordLength;
	public bool isMoving, linesActive;

	public GameObject anchor, obj;
	public Quaternion targetRotation = Quaternion.identity;
	public Vector3 scale;
	public LineRenderer line;

	public float length_to_origin;
	public float length_to_last;

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

		obj.AddComponent<LineRenderer>();
		line = obj.GetComponent<LineRenderer>();
		line.enabled = true;
		line.startWidth = 0.01f;
		line.endWidth = 0.1f;

		number = counter;
		counter++;
	}
}
public class Chord
{
	public List<Tone> tones = new List<Tone>();
	private float scale = 1f;
}
