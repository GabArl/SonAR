using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerFiller : MonoBehaviour
{
	[Range(1, 10)]
	public float globeSize;
	private Vector3 globeVector;


	public Mesh globalMesh;

	public Layer[] layers;
	private List<GameObject> layerObjects = new List<GameObject>();
	private List<GameObject> anchorObjects = new List<GameObject>();
	private List<GameObject> sourceObjects = new List<GameObject>();
	
	

	private void Start()
	{
		Emitter e;
		foreach (GameObject obj in anchorObjects)
		{
			e = obj.GetComponent<Emitter>();
			AkSoundEngine.SetSwitch(e.audioSwitch.GroupId, e.audioSwitch.Id, gameObject);
			e.audioEvent.Post(obj); // TODO: synchronized with all other posts?
		}
	}

	[ContextMenu("Generate Entities")]
	private void Fill()
	{
		// clear();
		layerObjects.Clear();
		anchorObjects.Clear();
		sourceObjects.Clear();
		// Instantiate().

		int layerNum = 1;
		int sourceNum = 1;
		globeVector = new Vector3(globeSize, 0, 0);

		foreach (Layer layer in layers)
		{
			GameObject tempLayer = new GameObject();

			tempLayer.name = "Layer_" + layerNum + "_elv" + layer.elevation + "_" + layer.sources.Length + "src";
			tempLayer.transform.SetParent(gameObject.transform, false);

			sourceNum = 0;
			foreach (Source source in layer.sources)
			{
				GameObject tempAnchor = new GameObject();
				GameObject tempSource = new GameObject();

				tempAnchor.name = layerNum + "_azm" + source.azimuth + "_" + sourceNum;
				tempSource.name = layerNum + "_src_" + sourceNum + "-" + source.trackCategory.Name + "" + source.trackNumber;


				tempSource.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

				tempSource.AddComponent<AkGameObj>();
				tempSource.AddComponent<Emitter>();
				tempSource.GetComponent<Emitter>().audioEvent = source.trackCategory;
				tempSource.GetComponent<Emitter>().audioSwitch = source.trackNumber;

				

				tempSource.AddComponent<MeshRenderer>();
				tempSource.AddComponent<MeshFilter>();
				tempSource.GetComponent<MeshFilter>().mesh = globalMesh;

				tempAnchor.transform.SetParent(tempLayer.transform, false);
				tempSource.transform.SetParent(tempAnchor.transform, false);

				tempAnchor.transform.localRotation = Quaternion.Euler(0, source.azimuth, layer.elevation);
				tempSource.transform.localPosition = new Vector3(globeSize, 0, 0);

				anchorObjects.Add(tempAnchor);
				sourceObjects.Add(tempSource);
				sourceNum++;
			}
			layerObjects.Add(tempLayer);
			layerNum++;
		}
	}
}

[System.Serializable]
public class Source
{
	public float azimuth;
	public AK.Wwise.Event trackCategory;
	public AK.Wwise.Switch trackNumber;
}

[System.Serializable]
public class Layer
{
	public float elevation;
	public Source[] sources;
}
