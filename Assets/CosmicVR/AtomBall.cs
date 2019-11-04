using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomBall : MonoBehaviour
{
	private Vector3 lastFrameVelocity;
	private Vector3 lastVelocity;
	private Rigidbody rb;

	public AK.Wwise.RTPC rtpc_vel_x;
	public AK.Wwise.RTPC rtpc_vel_y;
	public AK.Wwise.RTPC rtpc_vel_z;

	public AK.Wwise.RTPC rtpc_pos_x;
	public AK.Wwise.RTPC rtpc_pos_y;
	public AK.Wwise.RTPC rtpc_pos_z;

	public delegate void AtomCollision();
	public event AtomCollision OnAtomCollision;

	// Start is called before the first frame update
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.AddRelativeForce(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), ForceMode.Impulse);
	}

	// Update is called once per frame
	void Update()
	{
		lastFrameVelocity = rb.velocity;

		AkSoundEngine.SetRTPCValue(rtpc_pos_x.Id, transform.localPosition.x, gameObject);
		AkSoundEngine.SetRTPCValue(rtpc_pos_y.Id, transform.localPosition.y, gameObject);
		AkSoundEngine.SetRTPCValue(rtpc_pos_z.Id, transform.localPosition.z, gameObject);
	}


	void FixedUpdate()
	{
		lastVelocity = rb.velocity;
	}

	private void OnCollisionEnter(Collision collision)
	{
		//OnAtomCollision?.Invoke();

		ContactPoint cp = collision.contacts[0];
		lastVelocity = Vector3.Reflect(lastVelocity, cp.normal);
		rb.velocity = lastVelocity;

		AkSoundEngine.SetRTPCValue(rtpc_vel_x.Id, rb.velocity.x, gameObject);
		AkSoundEngine.SetRTPCValue(rtpc_vel_y.Id, rb.velocity.y, gameObject);
		AkSoundEngine.SetRTPCValue(rtpc_vel_z.Id, rb.velocity.z, gameObject);

		GetComponent<Emitter>().audioEvent.Post(gameObject);
	}
}
