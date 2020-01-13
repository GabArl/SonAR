using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO
// make angular speed uniform / instant
// redesign inner/first notes
// fallback no arcore / no camera allowed
// dont cast shadows
// repair lines
// fix ARCore
// play chords when stepping
// expose params and enable designs

public class MusicMover : MonoBehaviour
{
	private List<int> keys = new List<int>();
	private Chord currentChord = new Chord();
	private List<Chord> chords = new List<Chord>();
	private int count_moving_tones, count_moving_chords;
	private bool chordFull = false, tonesMoving = false, chordsMoving = false;
	private bool hasInput = false, startFromZero = false;

	public Material materialObj, materialHighlight, materialLine;
	public Mesh mesh;
	public Vector3 scale;
	public GameObject middlePoint;
	public GameObject startPoint;

	[Range(0f, 2f)]
	public float angle_tolerance = 0.1f;
	[Range(20f, 300f)]
	public float turningRate = 150f;
	public float diameter = 4;
	public int keyOfScale = 0, numberOfNotes = 4;
	public float toneAngle = 30f;

	public GameObject keyboard;
	public List<Button> key_buttons = new List<Button>();

	public AK.Wwise.Event startEvent, stopEvent, longStopEvent, toneIdle_start, toneIdle_stop, start_lastChord, stop_lastChord, tickEvent;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone;
	public AK.Wwise.RTPC length_to_origin;
	public AK.Wwise.RTPC length_to_last;

	private Vector3 origin, self, last;

	private int stepCountMax = 4;

	public MusicMetro metro;

	void Start()
	{
		Create();

		foreach (Button keybutton in keyboard.GetComponentsInChildren<Button>())
		{
			key_buttons.Add(keybutton);
		}
		key_buttons[12].enabled = false; // weil metro/sequencer nur 0-11 geht momentan...
	}

	private void OnDestroy()
	{
		AkSoundEngine.StopAll();
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
		if (!tonesMoving && !chordsMoving && hasInput && Time.time - metro.lastInputTime > 3)
		{
			ResetTones();
		}
		if (hasInput)
		{
			UpdateLines();
		}
		metro.UpdateMetro();

	}

	[ContextMenu("Create Objects")]
	public void Create()
	{
		origin = startPoint.transform.position;
		currentChord = new Chord();
		currentChord.tones = new List<Tone>();
		metro.SetParams(stepCountMax, diameter, toneAngle);


		//	for (int i = 0; i < 30; i++)
		//		taps.Add(Time.timeSinceLevelLoad);

		for (int i = 0; i < numberOfNotes; i++)
		{
			currentChord.tones.Add(CreateTone());
		}
	}
	private Tone CreateTone()
	{
		Tone tone = new Tone(startPoint.transform.position, gameObject);
		//	tone.anchor.transform.SetParent(gameObject.transform);
		tone.anchor.transform.localPosition = gameObject.transform.localPosition;
		tone.obj.GetComponent<MeshFilter>().mesh = mesh;
		tone.obj.GetComponent<MeshRenderer>().material = materialObj;
		tone.obj.transform.position = startPoint.transform.position;
		tone.obj.transform.localScale = scale;
		tone.obj.SetActive(false);
		tone.line.material = materialLine;
		tone.line.positionCount = 3;
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
				if (Quaternion.Angle(tone.anchor.transform.localRotation, tone.targetRotation) < angle_tolerance)
				{
					tone.anchor.transform.localRotation = tone.targetRotation;
					count_moving_tones--;
					tone.isMoving = false;
					AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
					continue;
				}
				tone.anchor.transform.localRotation = Quaternion.RotateTowards(tone.anchor.transform.localRotation, tone.targetRotation, Time.deltaTime * turningRate);
			}
		}
		if (count_moving_tones <= 0)
			tonesMoving = false;
	}
	private void ResetTones()
	{
		metro.lastInputTime = Time.time;
		hasInput = false;
		chordFull = false;
		tonesMoving = false;
		count_moving_tones = 0;
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
		Tone tempTone;

		for (int i = 0; i < currentChord.tones.Count; i++)
		{
			if (currentChord.tones[i].isMoving)
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
	}

	private void FinishChord()
	{
		chordFull = false;
		chordsMoving = true;
		count_moving_chords = chords.Count;
		chords.Add(CopyChord(currentChord));
		keys.Clear();

		for (int i = 0; i < chords.Count; i++) // for-loop because Destroy()
		{
			if (!PushChord(chords[i]))
			{
				DestroyChord(chords[i]);
				i--;
			}
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
		foreach (Tone tone in chord.tones)
		{
			Tone copyTone = new Tone(startPoint.transform.localPosition, gameObject);
			copyTone.semitone = tone.semitone;
			//	copyTone.anchor.transform.SetParent(gameObject.transform);
			copyTone.anchor.transform.localPosition = tone.anchor.transform.localPosition;
			copyTone.anchor.transform.localRotation = tone.anchor.transform.localRotation;
			copyTone.obj.transform.position = tone.obj.transform.position;
			copyTone.obj.transform.localPosition = tone.obj.transform.localPosition;
			copyTone.obj.transform.localRotation = tone.obj.transform.localRotation;
			copyTone.obj.GetComponent<MeshFilter>().mesh = mesh;
			copyTone.obj.GetComponent<MeshRenderer>().material = materialObj;
			copyTone.obj.transform.localScale = scale;
			copyTone.line.enabled = false;
			copyChord.tones.Add(copyTone);
		}
		return copyChord;

	}
	private bool PushChord(Chord chord)
	{
		if (chord.step >= stepCountMax - 1) return false; // hier passiert irgendwas komisches lol, ein chords zu viel wenn true

		chord.step++;
		AkSoundEngine.PostEvent(stop_lastChord.Id, gameObject);
		foreach (Tone tone in chord.tones)
		{
			if (chord.step != 0) metro.sequencer[tone.semitone, chord.step - 1] = false;
			//if (chord.step == 0) sequencer[tone.semitone, stepCountMax - 1] = false;
			metro.sequencer[tone.semitone, chord.step] = true;

			tone.targetRotation = tone.anchor.transform.localRotation;
			// tone.targetRotation *= Quaternion.Euler(0, 0, -1);

			//tone.obj.transform.localPosition = Vector3.Scale(tone.obj.transform.localPosition, new Vector3(1.3f, 1f, 1f));
			if (chord.step == 0)
				tone.obj.transform.localPosition += new Vector3(0.0445f - 0.02f, 0f, 0f); // outer starting point fehlt
			else tone.obj.transform.localPosition += new Vector3(diameter / 2, 0f, 0f);


			Vector3 scale = tone.obj.transform.localScale;
			tone.obj.transform.localScale = new Vector3(scale.x, scale.y * 0.7f, scale.z * 0.8f);
			AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj.gameObject);
			//AkSoundEngine.SetRTPCValue(rtpc_step.Id, chord.step, gameObject);
			AkSoundEngine.PostEvent(toneIdle_start.Id, tone.obj.gameObject);

			AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, gameObject);
			AkSoundEngine.PostEvent(start_lastChord.Id, gameObject);
		}
		chord.tones.Sort((t1, t2) => t1.semitone.CompareTo(t2.semitone));
		chord.tones[0].obj.GetComponent<MeshRenderer>().material = materialHighlight;

		foreach (Button keybutton in key_buttons)
		{
			keybutton.interactable = true;
		}

		return true;
	}
	private void DestroyChord(Chord chord)
	{
		foreach (Tone tone in chord.tones)
		{
			metro.sequencer[tone.semitone, chord.step] = false;
			AkSoundEngine.PostEvent(toneIdle_stop.Id, tone.obj.gameObject);
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
		metro.AddInputTap();
		hasInput = true;
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
	public int semitone;
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
		line.startWidth = 0.001f;
		line.endWidth = 0.003f;

		number = counter;
		counter++;
	}
}
public class Chord
{
	public List<Tone> tones = new List<Tone>();
	private float scale = 1f;
	public int step = -1;
}
