using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visuals : MonoBehaviour
{
	private Color color;
	private float size;
	private uint playId = 0;

	public GameObject listener;

	private int outBuffer;
	private int isBuffering;

	private float percentBuffered;
	private int memoryFullBool;

	uint[] array = new uint[3];
	uint reffo = 420;

	float value;

	public AK.Wwise.RTPC rtpc;
	private float rtpc_value;

	private AkGameObj akObject;
	public AK.Wwise.Event audioEvent;
	public uint track_number;
	private AK.Wwise.Switch track_switch;

	// Start is called before the first frame update
	void Start()
    {
		//AkCallbackType.AK_CallbackBits;

		akObject = GetComponent<AkGameObj>();
		//playId = AkSoundEngine.PostEvent(audioEvent.Id, gameObject);
		AkSoundEngine.SetSwitch(track_switch.Id, track_number, gameObject);
		playId = audioEvent.Post(gameObject);
	}	
	
    // Update is called once per frame
    void Update()
    {

		//AkSoundEngine.GetPlayingIDsFromGameObject(gameObject, ref reffo, array);

		AkSoundEngine.RegisterPluginDLL("dll_name","dll_path"); // can ommit path, but how do i reach it still?
		//AkSoundEnginePINVOKE.CSharp_AkSourceSettings_pMediaMemory_get();
		
		int valueType = 1;
		AkSoundEngine.GetRTPCValue(rtpc.Id, gameObject, playId, out value, ref valueType);
		



		//playId = audioEvent.Id;
		Debug.Log("playId: " + playId + "   gameObject: " + gameObject.name + "   value: " + value);
		


		//Debug.Log(AkSoundEngine.GetPlayingIDsFromGameObject(gameObject));
		AkSoundEngine.GetSourceStreamBuffering(playId, out outBuffer, out isBuffering);
		Debug.Log("outBuffer  " + outBuffer + "   isBuffering   " + isBuffering);

		//AkSoundEngine.IsGameObjectRegistered();
		AkSoundEngine.PinEventInStreamCache(audioEvent.Id, 'a', 'i');
		AkSoundEngine.GetBufferStatusForPinnedEvent(audioEvent.Id, out percentBuffered, out memoryFullBool);
		Debug.Log("percentBuffered  " + percentBuffered + "   memoryFullBool   " + memoryFullBool);

		//AkSoundEngine.PostEvent(); +CALLBACK?

		//AkSoundEngine.RegisterGameObj(); &&  AkSoundEngine.PreGameObjectAPICall(gameObject, AkSoundEngine.GetAkGameObjectID(gameObject)); //registriert AkObject anhand von gameObject mit und ohne hashCode?
		//AkSoundEngine.PrepareEvent(); // ich glaube hier wird echt Speicher reserviert und so
		//AkSoundEngine.QueryAudioObjectIDs(); // kp man
		//AkSoundEngine.RegisterEmitter(); // spatial audio?


		//AkSoundEngine.StringFromIntPtrString();
		//AkSoundEnginePINVOKE.CSharp_AkSourceSettings_pMediaMemory_get();
		//Debug.Log("buffertick:  " + AkSoundEnginePINVOKE.CSharp_GetBufferTick());


		//GetComponent<ParticleSystem>().startSize = 0; <<-- läuft!
	}
}
