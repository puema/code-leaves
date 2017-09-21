using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;

namespace Frontend
{
    public class FloorMenu : MonoBehaviour, IInputClickHandler
    {
        public GameObject Menu;

        // Shifts the menu up, so it doesn't collide with the floor
        private readonly Vector3 yShift = Vector3.up * 0.03f;

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
                if (Vector3.Distance(GazeManager.Instance.HitPosition, GazeManager.Instance.GazeOrigin) < 2)
                {
                    Debug.Log(GazeManager.Instance.HitPosition);
                    Debug.Log(yShift);
                    Menu.transform.position = GazeManager.Instance.HitPosition + yShift;
                }
                else
                {
                    Menu.transform.position =
                        GazeManager.Instance.GazeOrigin + GazeManager.Instance.GazeNormal * 2 + yShift;
                }

                Menu.transform.GetChild(0).GetComponent<Interpolator>().SetTargetPosition(new Vector3(-0.07f, 0, 0));
                Menu.transform.GetChild(1).GetComponent<Interpolator>()
                    .SetTargetPosition(new Vector3(-0.025f, 0.03f, 0));
                Menu.transform.GetChild(2).GetComponent<Interpolator>()
                    .SetTargetPosition(new Vector3(0.025f, 0.03f, 0));
                Menu.transform.GetChild(3).GetComponent<Interpolator>().SetTargetPosition(new Vector3(0.07f, 0, 0));
                InputManager.Instance.PushFallbackInputHandler(gameObject);
            }
            else
            {
                Menu.transform.GetChild(0).transform.localPosition = Vector3.zero;
                Menu.transform.GetChild(1).transform.localPosition = Vector3.zero;
                Menu.transform.GetChild(2).transform.localPosition = Vector3.zero;
                Menu.transform.GetChild(3).transform.localPosition = Vector3.zero;
                InputManager.Instance.PopFallbackInputHandler();
            }
        }
    }
}