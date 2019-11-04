using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEvent : MonoBehaviour
{
	public AK.Wwise.Event audioEvent;
	public AK.Wwise.Switch audioSwitch;
	public uint track_number;

	private string switchGroup;
	private string switchState;
	
	void Start()
	{
		//switchGroup = "track_number";
		//switchState = "_" + track_number.ToString();
		//AkSoundEngine.SetSwitch(switchGroup, switchState, gameObject);
		AkSoundEngine.SetSwitch(audioSwitch.GroupId, audioSwitch.Id, gameObject);
		audioEvent.Post(gameObject);
	}
}
