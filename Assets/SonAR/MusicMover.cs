using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO

// fallback no arcore / no camera allowed
// dont cast shadows
// clean switches, make globals
// enable camera feed
// disable tick modes when in read:group
// tweak speed
// from-to objects

public class MusicMover : MonoBehaviour
{
	public MusicMetro metro;
	public GameObject middlePoint, startPoint, pushPoint, endPoint;

	public GameObject keyboard;
	private List<int> inputKeys = new List<int>();
	private List<Button> key_buttons = new List<Button>();
	private Chord currentChord = new Chord();
	private List<Chord> chords = new List<Chord>();
	private int count_moving_tones, count_moving_chords;
	private bool chordFull = false, tonesMoving = false, chordsMoving = false;
	private bool hasInput = false;

	[Range(20f, 300f)]
	public float turningRate = 300f;
	private int numberOfNotes = 3;
	private float toneAngle = 360f / 12f;
	private float tolerance_angle = 0.1f;
	private float tolerance_position = 0.005f;
	private float inner_radius;
	private float step_length;
	private readonly int stepCountMax = 4;

	public Material materialObj, materialHighlight, materialLine;
	public Mesh mesh;
	public Vector3 scale;

	public AK.Wwise.Event startEvent, stopEvent, moveChord_stop, toneIdle_start, start_lastChord, stop_lastChord;
	public AK.Wwise.Event stepEvent_start, stepEvent_stop;
	public AK.Wwise.RTPC rtpc_chordLength, rtpc_semitone, rtpc_tickSemi;
	public AK.Wwise.RTPC length_to_origin;
	public AK.Wwise.RTPC length_to_last;

	private string chord_design = "one";
	public string tick_design = "one";

	private GameObject ar_plane_discovery, ar_plane_generator, ar_point_cloud;

	void Start()
	{
		Create();
	}


	private void OnDestroy()
	{
		AkSoundEngine.StopAll();
		Destroy(transform.parent.gameObject);

		ar_plane_discovery.SetActive(true);
		ar_plane_generator.SetActive(true);
		ar_point_cloud.SetActive(true);
		GameObject.Find("ARCore Device").GetComponent<GoogleARCore.ARCoreSession>().SessionConfig.PlaneFindingMode = GoogleARCore.DetectedPlaneFindingMode.Horizontal;
		GameObject.Find("SonAR Controller").GetComponent<GoogleARCore.Examples.HelloAR.HelloARController>().m_hasObject = false;
	}

	void Update()
	{
		MoveTones();
		MoveChords();

		if (!tonesMoving && !chordsMoving)
		{
			if (chordFull)
				FinishChord();

			if (hasInput && Time.time - metro.lastInputTime > 3)
				ResetTones();
		}
		metro.UpdateMetro();
		UpdateLines();
	}

	[ContextMenu("Create Objects")]
	public void Create()
	{
		// Prepare environment

		GameObject.Find("ARCore Device").GetComponent<GoogleARCore.ARCoreSession>().SessionConfig.PlaneFindingMode = GoogleARCore.DetectedPlaneFindingMode.Disabled;
		GameObject.Find("SonAR Controller").GetComponent<GoogleARCore.Examples.HelloAR.HelloARController>().m_hasObject = true;
		ar_plane_discovery = GameObject.Find("PlaneDiscovery");
		ar_plane_generator = GameObject.Find("Plane Generator");
		ar_point_cloud = GameObject.Find("Point Cloud");
		ar_plane_generator.SetActive(false);
		ar_point_cloud.SetActive(false);

		// Prepare object

		transform.parent.transform.localRotation =
			Quaternion.EulerRotation(
				transform.parent.transform.localRotation.x,
				GameObject.Find("ARCore Device").transform.localRotation.y,
				transform.parent.transform.localRotation.z);
		transform.parent.transform.localRotation *= Quaternion.Euler(0f, -90f, 0f); // Rotate against offset and towards user

		inner_radius = Vector3.Distance(middlePoint.transform.position, startPoint.transform.position);
		step_length = ((endPoint.transform.localPosition.x - pushPoint.transform.localPosition.x) / (stepCountMax - 1));

		// Prepare input

		metro.SetParams(stepCountMax, step_length, pushPoint.transform.localPosition.x, toneAngle);

		currentChord = new Chord();
		currentChord.tones = new List<Tone>();

		for (int i = 0; i < numberOfNotes; i++)
		{
			currentChord.tones.Add(CreateTone());
		}

		foreach (Button keybutton in keyboard.GetComponentsInChildren<Button>())
		{
			key_buttons.Add(keybutton);
		}
		key_buttons[12].enabled = false; // For now the sequencer only handles 0-11.
	}

	private Tone CreateTone()
	{
		Tone tone = new Tone(gameObject);
		tone.anchor.transform.position = middlePoint.gameObject.transform.position;
		tone.obj.GetComponent<MeshFilter>().mesh = mesh;
		tone.obj.GetComponent<MeshRenderer>().material = materialObj;
		tone.obj.transform.position = middlePoint.transform.position;
		tone.obj.transform.localPosition = startPoint.transform.localPosition;
		tone.obj.transform.localScale = scale;
		tone.obj.SetActive(false);
		tone.line.material = materialLine;
		tone.line.positionCount = 2;
		return tone;
	}

	private void MoveChords()
	{
		if (!chordsMoving) return;

		foreach (Chord chord in chords)
		{
			if (!chord.isMoving) continue;
			if (count_moving_chords > stepCountMax) count_moving_chords = stepCountMax;

			foreach (Tone tone in chord.tones)
			{
				if (Vector3.Distance(tone.obj.transform.localPosition, tone.targetPosition) <= tolerance_position)
				{
					chord.movingTones--;
					tone.obj.transform.localPosition = tone.targetPosition;
					AkSoundEngine.PostEvent(moveChord_stop.Id, tone.obj);
				}
				else
					tone.obj.transform.localPosition = Vector3.Lerp(tone.obj.transform.localPosition, tone.targetPosition, Time.deltaTime * turningRate / 15);
			}
			if (chord.movingTones <= 0)
			{
				count_moving_chords--;
				chord.isMoving = false;
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

				if (Quaternion.Angle(tone.anchor.transform.localRotation, tone.targetRotation) < tolerance_angle)
				{
					tone.anchor.transform.localRotation = tone.targetRotation;
					count_moving_tones--;
					tone.isMoving = false;
					AkSoundEngine.PostEvent(stopEvent.Id, tone.obj);
				}
				else
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
		inputKeys.Clear();
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
		if (!hasInput) return;

		Tone tempTone;
		Vector3 self, last;

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

				tempTone.length_to_origin = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, inner_radius * 2, Vector3.Distance(startPoint.transform.position, self)));
				tempTone.length_to_last = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, inner_radius * 2, Vector3.Distance(last, self)));


				if (metro.activeChordMode == MusicMetro.MetroChordMode.Math)
				{
					tempTone.line.SetPosition(0, last);
					tempTone.line.SetPosition(1, self);
					tempTone.length_to_origin = tempTone.length_to_last; // Dirty workaround, need rethinking.
				}
				else
				{
					tempTone.line.SetPosition(0, startPoint.transform.position);
					tempTone.line.SetPosition(1, self);
					tempTone.length_to_last = 0;

					//tempTone.line.SetPosition(2, last);
				}


				AkSoundEngine.SetRTPCValue(length_to_origin.Id, tempTone.length_to_origin, currentChord.tones[i].obj);
				//AkSoundEngine.SetRTPCValue(length_to_last.Id, tempTone.length_to_last, currentChord.tones[i].obj);

				currentChord.tones[i] = tempTone;
			}
		}
	}

	private void FinishChord()
	{
		// Update system

		inputKeys.Clear();
		chordFull = false;
		chordsMoving = true;
		count_moving_chords = chords.Count;

		foreach (Button button in key_buttons)
			button.interactable = false;

		// Update Chords

		chords.Add(CopyChord(currentChord));

		for (int i = 0; i < chords.Count; i++) // for-loop instead of foreach because Destroy()
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
		ResetTones();
	}

	private Chord CopyChord(Chord chord)
	{
		Chord copyChord = new Chord();
		foreach (Tone tone in chord.tones)
		{
			Tone copyTone = new Tone(gameObject);
			copyTone.semitone = tone.semitone;
			copyTone.anchor.transform.localPosition = tone.anchor.transform.localPosition;
			copyTone.anchor.transform.localRotation = tone.anchor.transform.localRotation;
			copyTone.obj.transform.position = tone.obj.transform.position;
			copyTone.obj.transform.localPosition = tone.obj.transform.localPosition;
			copyTone.obj.transform.localRotation = tone.obj.transform.localRotation;
			copyTone.obj.GetComponent<MeshFilter>().mesh = mesh;
			copyTone.obj.GetComponent<MeshRenderer>().material = materialObj;
			copyTone.obj.transform.localScale = scale;
			copyTone.length_to_origin = tone.length_to_origin;
			copyTone.length_to_last = tone.length_to_last;
			AkSoundEngine.SetRTPCValue(length_to_origin.Id, copyTone.length_to_origin, copyTone.obj);
			AkSoundEngine.SetRTPCValue(length_to_last.Id, copyTone.length_to_last, copyTone.obj);

			copyTone.line.enabled = false;
			copyChord.tones.Add(copyTone);
		}
		return copyChord;
	}

	private bool PushChord(Chord chord)
	{
		if (chord.step >= stepCountMax - 1) return false;

		foreach (Button keybutton in key_buttons)
		{
			keybutton.interactable = true;
		}

		// Update chord

		chord.step++;
		chord.isMoving = true;
		chord.movingTones = chord.tones.Count;
		chord.tones.Sort((t1, t2) => t1.semitone.CompareTo(t2.semitone)); // Sort for base tone
		chord.tones[0].obj.GetComponent<MeshRenderer>().material = materialHighlight;

		AkSoundEngine.PostEvent(stop_lastChord.Id, gameObject);

		// Update tones and sequencer

		foreach (Tone tone in chord.tones)
		{
			if (chord.step != 0)
				metro.sequencer[tone.semitone, chord.step - 1] = false;

			metro.sequencer[tone.semitone, chord.step] = true;
			tone.targetPosition = new Vector3(
				(step_length * chord.step) + pushPoint.transform.localPosition.x,
				tone.obj.transform.localPosition.y,
				tone.obj.transform.localPosition.z);

			Vector3 scale = tone.obj.transform.localScale;
			tone.obj.transform.localScale = new Vector3(scale.x, scale.y * 0.7f, scale.z * 0.8f);
			//	AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj.gameObject);
			//	AkSoundEngine.PostEvent(toneIdle_start.Id, tone.obj.gameObject);

			AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, gameObject); // set rtpc_semi to base tone for distant keytone?
			AkSoundEngine.PostEvent(start_lastChord.Id, gameObject);
		}

		PlayChord(metro.current_chord_);
		return true;
	}

	private void DestroyChord(Chord chord)
	{
		foreach (Tone tone in chord.tones)
		{
			metro.sequencer[tone.semitone, chord.step] = false;
			AkSoundEngine.StopAll(tone.obj.gameObject);
			Destroy(tone.obj);
			Destroy(tone.anchor);
		}
		chord.tones.Clear();
		chords.Remove(chord);
	}

	public void AddKeyToChord(int keyNum)
	{
		if (chordFull) return;


		// Update system

		hasInput = true;
		tonesMoving = true;
		count_moving_tones++;
		metro.AddInputTap();

		// Update tone

		Tone tone = currentChord.tones[inputKeys.Count];
		tone.obj.SetActive(true);
		tone.isMoving = true;
		tone.semitone = keyNum;
		tone.targetRotation = Quaternion.Euler(0, keyNum * toneAngle, 0);

		if (metro.activeChordMode == MusicMetro.MetroChordMode.Math)
		{
			if (inputKeys.Count == 0)
				tone.anchor.transform.localRotation = Quaternion.Euler(0, 0, 0);
			else
				tone.anchor.transform.localRotation = currentChord.tones[inputKeys.Count - 1].anchor.transform.localRotation;
		}
		else
		{
			tone.anchor.transform.localRotation = Quaternion.Euler(0, 0, 0);
		}

		currentChord.tones[inputKeys.Count] = tone;

		AkSoundEngine.SetSwitch("chord_design", chord_design, currentChord.tones[inputKeys.Count].obj);
		AkSoundEngine.PostEvent(startEvent.Id, currentChord.tones[inputKeys.Count].obj);

		// Update input

		inputKeys.Add(keyNum);
		key_buttons.Find(x => x.name.Contains(keyNum.ToString())).interactable = false;

		if (inputKeys.Count == numberOfNotes)
		{
			chordFull = true;
			foreach (Button button in key_buttons)
				button.interactable = false;
		}
	}

	#region [Audio related]

	public void PlayChord(int current_chord_) // Move to own class?
	{
		if (chords.Count > current_chord_)
		{
			int inverseChord = chords.Count - current_chord_ - 1; // Because chord objects are saved in reversed order.

			for (int i = 0; i < chords.Count; i++)
			{
				foreach (Tone tone in chords[i].tones)
				{
					AkSoundEngine.PostEvent(stepEvent_stop.Id, tone.obj.gameObject);

					if (i == inverseChord)
					{
						if (metro.activeChordMode == MusicMetro.MetroChordMode.Math)
						{
							AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, 0, tone.obj.gameObject);
							AkSoundEngine.SetRTPCValue(length_to_origin.Id, tone.length_to_origin, tone.obj.gameObject);

						}
						else if (metro.activeChordMode == MusicMetro.MetroChordMode.Music)
						{
							AkSoundEngine.SetRTPCValue(length_to_origin.Id, 0, tone.obj.gameObject);
							AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, tone.semitone, tone.obj.gameObject);
						}
						AkSoundEngine.SetSwitch("chord_design", chord_design, tone.obj.gameObject); // Could it be stacked into one AkObject?
						AkSoundEngine.PostEvent(stepEvent_start.Id, tone.obj.gameObject);
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < chords.Count; i++)
			{
				foreach (Tone tone in chords[i].tones)
					AkSoundEngine.PostEvent(stepEvent_stop.Id, tone.obj.gameObject);
			}
		}
	}

	public void PlayTick(int current_semitone_, int current_chord_, AK.Wwise.RTPC rtpc_step_, AK.Wwise.Event semitone_event_) // This is not good practice and needs rethinking.
	{
		int inverseChord = chords.Count - current_chord_ - 1; // Because chord objects are saved in reversed order.

		foreach (Tone tone in chords[inverseChord].tones)
		{
			if (tone.semitone == current_semitone_)
			{
				AkSoundEngine.SetSwitch("tick_design", tick_design, tone.obj.gameObject); // Could it be stacked into one AkObject?
				AkSoundEngine.SetRTPCValue(rtpc_tickSemi.Id, current_semitone_, tone.obj.gameObject);
				AkSoundEngine.SetRTPCValue(rtpc_step_.Id, current_chord_, tone.obj.gameObject);
				AkSoundEngine.PostEvent(semitone_event_.Id, tone.obj.gameObject);
			}
		}
	}
	#endregion

	#region [UI functions]

	public void ResetEnvironment()
	{
		Destroy(this);
	}

	public void SetChordDesign(Dropdown dropdown_)
	{
		switch (dropdown_.value)
		{
			case 0:
				chord_design = "one";
				break;
			case 1:
				chord_design = "two";
				break;
			case 2:
				chord_design = "three";
				break;
		}
	}
	public void SetNumberOfNotes(Slider slider_)
	{
		numberOfNotes = (int)slider_.value;
		slider_.transform.GetChild(slider_.transform.childCount - 1).GetComponentInChildren<Text>().text = slider_.value.ToString();
		currentChord.tones.Clear();
		for (int i = 0; i < numberOfNotes; i++)
		{
			currentChord.tones.Add(CreateTone());
		}
		for (int i = 0; i < chords.Count; i++) // for-loop instead of foreach because Destroy()
		{
			DestroyChord(chords[i]);
			i--;
		}
	}
	public void SetTurnSpeed(Slider slider_)
	{
		turningRate = slider_.value;
	}
}

#endregion

public class Tone
{
	public static int counter;
	public int number;
	public int semitone;
	public float chordLength;
	public bool isMoving, linesActive;

	public GameObject anchor, obj;
	public Quaternion targetRotation = Quaternion.identity;
	public Vector3 targetPosition;
	public Vector3 scale;
	public LineRenderer line;

	public float length_to_origin;
	public float length_to_last;

	public Tone(GameObject parent)
	{
		anchor = new GameObject();
		anchor.name = "anchor_" + counter;
		anchor.transform.SetParent(parent.transform);

		obj = new GameObject();
		obj.name = "tone_" + counter;
		obj.transform.SetParent(anchor.transform);

		obj.AddComponent<AkGameObj>();
		obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshFilter>();

		obj.AddComponent<LineRenderer>();
		line = obj.GetComponent<LineRenderer>();
		line.enabled = true;
		line.startWidth = 0.007f;
		line.endWidth = 0.01f;

		number = counter;
		counter++;
	}
}
public class Chord
{
	public List<Tone> tones = new List<Tone>();
	public int step = -1;
	public int movingTones;
	public bool isMoving = false;
}