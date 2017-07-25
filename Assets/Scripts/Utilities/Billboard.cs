using UnityEngine;

public class Billboard : MonoBehaviour {

	public Camera camera;

	private void Start()
	{
		camera = Camera.main;
	}

	// Update is called once per frame
	void Update () {
		transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward,
			camera.transform.rotation * Vector3.up);
	}
}
