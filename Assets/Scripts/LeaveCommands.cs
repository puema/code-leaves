using UnityEngine;

public class LeaveCommands : MonoBehaviour
{
	Vector3 originalPosition;
	private Color originalColor;

	// Use for initialization
	void Start()
	{
		// Grab the original local position of the sphere when the app starts.
		originalPosition = transform.localPosition;
		originalColor = GetComponent<MeshRenderer>().material.color;
	}

	// Called by GazeGestureManager when the user performs a Select gesture
	void OnSelect()
	{
		GetComponent<MeshRenderer>().material.color = Color.white;
	}
	
	// Called by GazeGestureManager when the user performs a Select gesture
	void OnDrop()
	{
		// Called by SpeechManager when the user says the "Drop leave" command
		if (!GetComponent<Rigidbody>())
		{
			var rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}
	}

	// Called by SpeechManager when the user says the "Reset forest" command
	void OnReset()
	{
		// If the sphere has a Rigidbody component, remove it to disable physics.
		var rigidbody = GetComponent<Rigidbody>();
		if (rigidbody != null)
		{
			DestroyImmediate(rigidbody);
		}

		// Put the sphere back into its original local position.
		transform.localPosition = originalPosition;
		
		// Set original color
		GetComponent<MeshRenderer>().material.color = originalColor;
	}

}
