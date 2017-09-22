using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
	public class CloseButton : MonoBehaviour, IInputClickHandler
	{
		public GameObject Window;
		
		public void OnInputClicked(InputClickedEventData eventData)
		{
			Window.SetActive(false);
		}
	}
}
