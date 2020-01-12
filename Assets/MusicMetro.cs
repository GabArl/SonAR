using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicMetro : MonoBehaviour
{
	public GameObject metro_object;
	public List<GameObject> lanes_list, steps_list;

	private int current_semitone, current_step, metro_chord;
	private float timeSinceTick;
	private float tapsPerSecond;
	public float lastInputTime;
	private List<float> taps = new List<float>();
	public AK.Wwise.Event tickEvent;
	public AK.Wwise.RTPC rtpc_step;
	public AK.Wwise.Switch tickDesign;

	private int stepCountMax;
	private float diameter;
	private float toneAngle;

	float maxBPM = 120f;
	float maxFactor = 0.5f;
	float maxTime = 1f / 4f;
	float bpm_factor;

	public Text bpmText;
	public Text sequencerText;

	public bool[,] sequencer = new bool[12, 4];


	public void SetParams(int stepCountMax_, float diameter_, float toneAngle_)
	{
		stepCountMax = stepCountMax_;
		diameter = diameter_;
		toneAngle = toneAngle_;
	}

	private void Start()
	{
		lanes_list[0].transform.localPosition += new Vector3(0f, 0.003f, 0f);
		steps_list[stepCountMax - 1].transform.localPosition += new Vector3(0f, 0.003f, 0f);

		for (int i = 0; i < sequencer.GetLength(0); i++)
			for (int j = 0; j < sequencer.GetLength(1); j++)
				sequencer[i, j] = false;
	}

	public void UpdateMetro()
	{
		BPM();
		Metro();

		sequencerText.text = "";
		for (int i = 0; i < sequencer.GetLength(0); i++)
		{
			sequencerText.text += "\n" + i + ": ";
			for (int j = 0; j < sequencer.GetLength(1); j++)
			{
				if (sequencer[i, j]) sequencerText.text += "o ";
				else sequencerText.text += "x ";

			}
		}
	}

	private void Metro()
	{
		timeSinceTick += Time.deltaTime;
		bpm_factor = Mathf.Lerp(0f, maxFactor, Mathf.InverseLerp(0f, maxBPM, tapsPerSecond));

		if (timeSinceTick >= maxTime - bpm_factor)
		{
			timeSinceTick = 0;

			if (current_step <= 0)
			{
				current_semitone++;
				current_step = stepCountMax;
				TickSemitone();
			}
			if (current_step > 0)
				TickStep();
		}
	}

	private void TickStep()
	{
		metro_object.transform.localPosition = new Vector3(
			((diameter / stepCountMax) * current_step) * 2 * Mathf.Sin(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)),
			metro_object.transform.localPosition.y,
			((diameter / stepCountMax) * current_step) * 2 * Mathf.Cos(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)));

		current_step--;

		if (current_step >= 0 && sequencer[current_semitone, current_step] == true)
		{
			AkSoundEngine.SetSwitch(tickDesign.GroupId, tickDesign.Id, metro_object.gameObject);
			AkSoundEngine.SetRTPCValue(rtpc_step.Id, current_step, metro_object.gameObject);
			AkSoundEngine.PostEvent(tickEvent.Id, metro_object.gameObject);
		}
	}

	private void TickSemitone()
	{
		if (current_semitone == 12)
		{
			if (metro_chord == 0)
			{
				steps_list[stepCountMax - 1].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
			}
			else
			{
				steps_list[metro_chord - 1].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
			}

			steps_list[metro_chord].transform.localPosition += new Vector3(0f, 0.003f, 0f);

			current_semitone = 0;
			metro_chord++;
			if (metro_chord == stepCountMax)
				metro_chord = 0;
		}

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

	public void AddInputTap()
	{
		taps.Add(Time.timeSinceLevelLoad);
		lastInputTime = Time.time;
	}
}
