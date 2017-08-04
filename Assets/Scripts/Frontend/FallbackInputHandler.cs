using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class FallbackInputHandler : MonoBehaviour, IInputClickHandler
{
    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        InteractionManager.Instance.HandleEmptyClick();
    }
}