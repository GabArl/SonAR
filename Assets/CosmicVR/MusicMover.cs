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
// one copied tone too much??

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
	private float toneAngle = 30f;

	public GameObject keyboard;
	public List<Button> key_buttons = new List<Button>();
	private float lastInputTime, tapsPerSecond;
	private List<float> taps = new List<float>();
	public Text bpmText;

	public AK.Wwise.Event startEvent, stopEvent, longStopEvent, toneIdle_start, toneIdle_stop, start_lastChord, stop_lastChord;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone, rtpc_step;

	public AK.Wwise.RTPC length_to_origin;
	public AK.Wwise.RTPC length_to_last;

	private Vector3 origin, self, last;


	//METRO
	public GameObject metro_semitone;
	public List<GameObject> lanes_list, steps_list;

	private int current_semitone, current_step, metro_chord;
	private int stepCountMax = 4;
	private float timeSinceTick, timeSinceStep, lastTickTime; // timeSinceStep != chord_step...

	private bool[][] sequencer = new bool[12][4]; // reicht bool?




	void Start()
	{
		Create();
		lanes_list[0].transform.localPosition += new Vector3(0f, 0.003f, 0f);
		steps_list[stepCountMax - 1].transform.localPosition += new Vector3(0f, 0.003f, 0f);


		foreach (Button keybutton in keyboard.GetComponentsInChildren<Button>())
		{
			key_buttons.Add(keybutton);
		}
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
		if (!tonesMoving && !chordsMoving && hasInput && Time.time - lastInputTime > 3)
		{
			ResetTones();
		}
		UpdateLines();
		BPM();
		Metro();
	}

	private void Metro()
	{
		timeSinceTick += Time.deltaTime;
		timeSinceStep += Time.deltaTime;
		float maxBPM = 120f;
		float maxFactor = 0.8f;
		float maxTime = 1f;
		float bpm_factor = Mathf.Lerp(0f, maxFactor, Mathf.InverseLerp(0f, maxBPM, tapsPerSecond));

		if (timeSinceStep > lastTickTime / stepCountMax)
		{
			timeSinceStep = 0;
			current_step--;
			metro_semitone.transform.localPosition = new Vector3(
				((diameter / stepCountMax) * current_step) * 2 * Mathf.Sin(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)),
				metro_semitone.transform.localPosition.y,
				((diameter / stepCountMax) * current_step) * 2 * Mathf.Cos(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)));
		}

		if (timeSinceTick >= maxTime - bpm_factor)
		{
			lastTickTime = timeSinceTick;
			timeSinceTick = 0;
			timeSinceStep = 0;
			current_semitone++;
			current_step = stepCountMax;


			if (current_semitone == 12)
			{
				if (metro_chord == 0)
				{
					steps_list[stepCountMax - 1].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
				}
				else
				{
				steps_list[metro_chord].transform.localPosition += new Vector3(0f, 0.003f, 0f);

				current_semitone = 0;
				metro_chord++;
					metro_chord = 0;
			}

			metro_semitone.transform.localPosition = new Vector3(
				diameter * 2 * Mathf.Sin(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)),
				metro_semitone.transform.localPosition.y,
				diameter * 2 * Mathf.Cos(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)));

			if (current_semitone == 0)
			{
				lanes_list[11].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
			}
			else
			{
				lanes_list[current_semitone - 1].transform.localPosition -= new Vector3(0f, 0.003f, 0f);

			}
			lanes_list[current_semitone].transform.localPosition += new Vector3(0f, 0.003f, 0f);

		}
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
		origin = startPoint.transform.position;
		currentChord = new Chord();
		currentChord.tones = new List<Tone>();

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
		lastInputTime = Time.time;
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
			tempTone = currentChord.tones[i];
			self = tempTone.obj.transform.localPosition;

			if (i == 0)
				last = self;
			else
				last = currentChord.tones[i - 1].obj.transform.localPosition;

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
		count_moving_chords = chords.Count;
		chords.Add(CopyChord(currentChord));
		keys.Clear();

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
		if (chord.step <= stepCountMax) // hier passiert irgendwas komisches lol, ein chords zu viel wenn true
		{
			AkSoundEngine.PostEvent(stop_lastChord.Id, gameObject);
			foreach (Tone tone in chord.tones)
			{
				tone.targetRotation = tone.anchor.transform.localRotation;
				// tone.targetRotation *= Quaternion.Euler(0, 0, -1);
				tone.obj.transform.localPosition = Vector3.Scale(tone.obj.transform.localPosition, new Vector3(1.3f, 1f, 1f));
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
			chord.step++;

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
	public int step = 0;
}
