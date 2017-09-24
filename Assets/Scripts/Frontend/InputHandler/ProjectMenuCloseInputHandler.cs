using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.InputHandler
{
	public class ProjectMenuCloseInputHandler : MonoBehaviour, IInputClickHandler
	{
		public ApplicationManager AppManager;
		
		public void OnInputClicked(InputClickedEventData eventData)
		{
			AppManager.AppState.ProjectMenu.IsActive.Value = false;
		}
	}
}
