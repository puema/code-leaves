using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;

namespace Frontend
{
    public class TapToMenu : MonoBehaviour, IInputClickHandler
    {
        public GameObject Menu;

        private void Start()
        {
            ApplicationManager.Instance.AppState.FloorInteractionMode.Subscribe(mode =>
            {
                if (mode != FloorInteractionMode.TapToMenu)
                    Menu.SetActive(false);
            });
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            Menu.SetActive(!Menu.activeSelf);

            if (Menu.activeSelf)
            {
                Menu.transform.position = GazeManager.Instance.HitPosition +
                                          Vector3.up * Menu.GetComponent<BoxCollider>().bounds.extents.y;
                Menu.transform.GetChild(0).GetComponent<Interpolator>().SetTargetPosition(new Vector3(-0.05f, 0, 0));
                Menu.transform.GetChild(1).GetComponent<Interpolator>().SetTargetPosition(new Vector3(0, 0.03f, 0));
                Menu.transform.GetChild(2).GetComponent<Interpolator>().SetTargetPosition(new Vector3(0.05f, 0, 0));
                InputManager.Instance.PushFallbackInputHandler(gameObject);
            }
            else
            {
                Menu.transform.GetChild(0).transform.localPosition = Vector3.zero;
                Menu.transform.GetChild(1).transform.localPosition = Vector3.zero;
                Menu.transform.GetChild(2).transform.localPosition = Vector3.zero;
                InputManager.Instance.PopFallbackInputHandler();
            }
        }
    }
}