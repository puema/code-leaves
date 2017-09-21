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

        private Vector3 yShift;

        private void Start()
        {
            yShift = Vector3.up * Menu.GetComponent<BoxCollider>().bounds.extents.y;

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