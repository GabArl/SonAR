using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CosmicVR;

public class AudioSystem
{
	
	AudioSystemImpl impl = new AudioSystemImplWwise();

	public AudioSystem() {
	}

	public void Post(CosmicVR.SoniLog.SonificationType sType) {
		SoniLog.Message(sType);
	}
}
