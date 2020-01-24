using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomSpawner : MonoBehaviour
{
	public int numberAtoms = 1;
	public AK.Wwise.Event soundEvent;
	public AK.Wwise.Event loopEvent;
	public Material renderMat;
	public PhysicMaterial physMat;
	public Mesh mesh;

	private List<GameObject> atoms;

	public AK.Wwise.RTPC rtpc_vel_x;
	public AK.Wwise.RTPC rtpc_vel_y;
	public AK.Wwise.RTPC rtpc_vel_z;

	public AK.Wwise.RTPC rtpc_pos_x;
	public AK.Wwise.RTPC rtpc_pos_y;
	public AK.Wwise.RTPC rtpc_pos_z;

	GameObject atom_group;

	// Start is called before the first frame update
	void Start()
	{
		Generate();
		
		foreach (GameObject atom in atoms)
		{
			AkSoundEngine.PostEvent(loopEvent.Id, atom);
		}
	}

	[ContextMenu("Post")]
	void Post() {
		Debug.LogError(atoms.Count);
	}

	[ContextMenu("Generate Atoms")]
	public void Generate()
	{
		atoms = new List<GameObject>();
		atom_group = new GameObject();
		atom_group.name = "_ATOMS";
		atom_group.transform.SetParent(gameObject.transform);
		atom_group.transform.localPosition = gameObject.transform.position;

		for (int i = 0; i < numberAtoms; i++)
		{
			GameObject temp = new GameObject();
			temp.name = "Atom_" + i;
			Vector3 local_scale = new Vector3(0.1f, 0.1f, 0.1f);
			Vector3 local_position = new Vector3(
				Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

			temp.transform.SetParent(atom_group.transform, false);
			temp.transform.localScale = local_scale;
			temp.transform.localPosition = local_position;

			temp.AddComponent<Rigidbody>();
			temp.GetComponent<Rigidbody>().useGravity = false;
			temp.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

			temp.AddComponent<SphereCollider>();
			temp.GetComponent<SphereCollider>().material = physMat;

			temp.AddComponent<MeshFilter>();
			temp.GetComponent<MeshFilter>().mesh = mesh;

			temp.AddComponent<MeshRenderer>();
			temp.GetComponent<MeshRenderer>().material = renderMat;

			temp.AddComponent<AtomBall>();
			AtomBall ab = temp.GetComponent<AtomBall>();
			ab.rtpc_pos_x = rtpc_pos_x;
			ab.rtpc_pos_y = rtpc_pos_y;
			ab.rtpc_pos_z = rtpc_pos_z;
			ab.rtpc_vel_x = rtpc_vel_x;
			ab.rtpc_vel_y = rtpc_vel_y;
			ab.rtpc_vel_z = rtpc_vel_z;

			temp.AddComponent<AkGameObj>();

			temp.AddComponent<Emitter>();
			temp.GetComponent<Emitter>().audioEvent = soundEvent;

			atoms.Add(temp);
		}
	}
}
