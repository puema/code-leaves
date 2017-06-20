using UnityEngine;

public class AddTree : MonoBehaviour
{

	public GameObject Tree;
	
	// Use this for initialization
	void Start ()
	{
		Vector3 position = new Vector3(-0.013f, 0.0076f, 2.029f);
		Quaternion quaternion = new Quaternion();
		Instantiate(Tree, position, quaternion).transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
	}
	
}
