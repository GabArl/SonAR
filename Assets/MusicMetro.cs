﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// metro range
// retrigger sound when input

public class MusicMetro : MonoBehaviour
{
	public GameObject metro_object;
	public List<GameObject> lanes_list, steps_list;

	private float timeSinceTick;
	private float tapsPerSecond;
	public float lastInputTime;
	private List<float> taps = new List<float>();
	public AK.Wwise.Event tickEvent, stepEvent_start, stepEvent_stop, semitoneEvent;
	public AK.Wwise.RTPC rtpc_step, rtpc_semitone;
	public AK.Wwise.Switch tickDesign;

	private int max_tick_count = 4, max_semi_count = 12, max_chord_count = 4;
	private float diameter;
	private float toneAngle;

	float maxBPM = 120f;
	float maxFactor = 0.5f;
	float maxTime = 1f / 4f;
	float bpm_factor;

	public Text bpmText;
	public Text sequencerText;

	public bool[,] sequencer = new bool[12, 4];

	[Range(0, 11)]
	public int from_semi_ = 0;
	private int from_semi
	{
		get
		{
			return from_semi_;
		}
		set
		{
			if (from_semi_ >= to_semi_)
				from_semi_ = to_semi_ - 1;
			else from_semi_ = value;

		}
	}

	[Range(0, 11)]
	public int to_semi_ = 11;
	private int to_semi
	{
		get
		{
			return to_semi_;
		}
		set
		{
			if (to_semi_ <= from_semi_)
				to_semi_ = from_semi_ + 1;
			else to_semi_ = value;
		}
	}

	public enum MetroReadMethod { Sonar, Sequence };
	public MetroReadMethod activeReadMethod = MetroReadMethod.Sequence;

	private enum MetroDirectionCycle : int { Clockwise = 1, CounterClockwise = -1 };
	public int activeDirectionCycle = (int)MetroDirectionCycle.Clockwise;

	private enum MetroDirectionSequence : int { Outwards = 1, Inwards = -1 };
	public int activeDirectionSequence = (int)MetroDirectionSequence.Inwards;

	private enum MetroDirectionChord : int { Outwards = 1, Inwards = -1 };
	public int activeDirectionChord = (int)MetroDirectionChord.Outwards;

	public int current_tick_;
	private int current_tick
	{
		get
		{
			return current_tick_;
		}
		set
		{
			if (value < 0 && activeDirectionSequence == (int)MetroDirectionSequence.Inwards)
			{
				current_tick_ = max_tick_count - 1;
				current_semitone += activeDirectionCycle;
			}
			else if (value >= max_tick_count && activeDirectionSequence == (int)MetroDirectionSequence.Outwards)
			{
				current_tick_ = 0;
				current_semitone += activeDirectionCycle;
			}
			else
			{
				current_tick_ = value;
			}
			ChangeTick();
		}
	}

	public int current_semitone_;
	private int last_semitone, next_semitone;
	private int current_semitone
	{
		get
		{
			return current_semitone_;
		}
		set
		{
			if ((value <= from_semi || value <= 0) && activeDirectionCycle == (int)MetroDirectionCycle.CounterClockwise)
			{
				last_semitone = value + 1;
				if (value == 0)
				{
					current_semitone_ = value;
					next_semitone = max_semi_count - 1;
				}
				else if (value == from_semi)
				{
					current_semitone_ = value;
					next_semitone = to_semi;
				}
				else
				{
					current_semitone_ = to_semi;
					next_semitone = value + to_semi;
					current_chord += activeDirectionChord;

				}
			}
			else if ((value >= to_semi || value >= max_semi_count - 1) && activeDirectionCycle == (int)MetroDirectionCycle.Clockwise)
			{
				last_semitone = value - 1;

				if (value == max_semi_count - 1)
				{
					current_semitone_ = value;
					next_semitone = 0;
				}
				else if (value == to_semi)
				{
					current_semitone_ = value;
					next_semitone = from_semi;
				}
				else
				{
					current_semitone_ = from_semi;
					next_semitone = value - from_semi;
					current_chord += activeDirectionChord;
				}
			}
			else
			{
				current_semitone_ = value;
				last_semitone = value - activeDirectionCycle;
				next_semitone = value + activeDirectionCycle;
			}
			ChangeSemitone();
		}
	}

	public int current_chord_;
	private int last_chord, next_chord;
	private int current_chord
	{
		get
		{
			return current_chord_;
		}
		set
		{
			if (value <= 0 && activeDirectionChord == (int)MetroDirectionChord.Inwards)
			{
				if (value == 0) current_chord_ = value;
				else current_chord_ = max_chord_count - 1;

				last_chord = value + 1;
				next_chord = value + max_chord_count - 1;
			}
			else if (value >= max_chord_count - 1 && activeDirectionChord == (int)MetroDirectionChord.Outwards)
			{
				if (value == max_chord_count - 1) current_chord_ = value;
				else current_chord_ = 0;

				last_chord = value - 1;
				next_chord = value - max_chord_count - 1;
			}
			else
			{
				current_chord_ = value;
				last_chord = value - activeDirectionChord;
				next_chord = value + activeDirectionChord;
			}
			ChangeChord();
		}
	}
	private void Start()
	{
		lanes_list[0].transform.localPosition += new Vector3(0f, 0.003f, 0f);
		steps_list[0].transform.localPosition += new Vector3(0f, 0.003f, 0f);

		for (int i = 0; i < sequencer.GetLength(0); i++)
			for (int j = 0; j < sequencer.GetLength(1); j++)
				sequencer[i, j] = false;
	}

	public void SetParams(int max_chord_, float diameter_, float toneAngle_)
	{
		max_chord_count = max_chord_;
		diameter = diameter_;
		toneAngle = toneAngle_;
	}

	public void UpdateMetro()
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

		timeSinceTick += Time.deltaTime;
		bpm_factor = Mathf.Lerp(0f, maxFactor, Mathf.InverseLerp(0f, maxBPM, tapsPerSecond));

		if (timeSinceTick >= maxTime - bpm_factor)
		{
			timeSinceTick = 0;

			if (activeReadMethod == MetroReadMethod.Sequence)
				current_tick += activeDirectionSequence;
			if (activeReadMethod == MetroReadMethod.Sonar)
				current_semitone += activeDirectionCycle;
		}

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

	private void ChangeTick()
	{
		metro_object.transform.localPosition = new Vector3(
			((diameter / max_chord_count) * (current_tick + 1)) * 2 * Mathf.Sin(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)),
			metro_object.transform.localPosition.y,
			((diameter / max_chord_count) * (current_tick + 1)) * 2 * Mathf.Cos(Mathf.Deg2Rad * ((toneAngle * current_semitone) + 90)));

		if (sequencer[current_semitone, current_tick] == true)
		{
			AkSoundEngine.SetSwitch(tickDesign.GroupId, tickDesign.Id, metro_object.gameObject);
			AkSoundEngine.SetRTPCValue(rtpc_step.Id, current_tick, metro_object.gameObject);
			AkSoundEngine.PostEvent(tickEvent.Id, metro_object.gameObject);
		}
	}

	private void ChangeSemitone()
	{
		lanes_list[last_semitone].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
		lanes_list[current_semitone].transform.localPosition += new Vector3(0f, 0.003f, 0f);
	}

	private void ChangeChord()
	{
		steps_list[last_chord].transform.localPosition -= new Vector3(0f, 0.003f, 0f);
		steps_list[current_chord].transform.localPosition += new Vector3(0f, 0.003f, 0f);

		AkSoundEngine.PostEvent(stepEvent_stop.Id, gameObject);
		for (int i = 0; i < sequencer.GetLength(0); i++)
		{
			if (sequencer[i, current_chord])
			{
				AkSoundEngine.SetRTPCValue(rtpc_semitone.Id, i, gameObject);
				AkSoundEngine.PostEvent(stepEvent_start.Id, gameObject);
			}
		}
	}

	public void AddInputTap()
	{
		taps.Add(Time.timeSinceLevelLoad);
		lastInputTime = Time.time;
	}
}