using System.IO;
using Core;
using Frontend.InputHandler;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;

namespace Frontend
{
    public class ContextMenu : MonoBehaviour, IInputClickHandler
    {
        public ApplicationManager AppManager;
        public GameObject Button;
        public string SpriteFolder = "Sprites";
        public const float distanceToCamera = 1.5f;
        
        private Core.ContextMenu Menu;

        // Shifts the menu up
        private readonly Vector3 yShift = Vector3.up * 0.03f;

        private void Start()
        {
            Menu = AppManager.AppState.UiElements.ContexMenu;
            
            Menu.Buttons.Subscribe(SetButtons);

            Menu.IsActive.Subscribe(SetActive);
        }

        private void SetButtons(ContextMenuButton[] buttons)
        {
            var parent = transform.GetChild(0);
            foreach (var button in buttons)
            {
                var buttonObject = Instantiate(Button, parent);
                var icon = Resources.Load<Sprite>(Path.Combine(SpriteFolder, button.Icon));
                buttonObject.GetComponentInChildren<SpriteRenderer>().sprite = icon;
                var inputHandler = buttonObject.AddComponent<ContextButtonInputHandler>();
                inputHandler.Action = button.Action;
            }
        }

        private void SetActive(bool isActive)
        {
            var buttonTransforms = transform.GetChild(0);
            if (isActive)
            {
                if (Vector3.Distance(GazeManager.Instance.HitPosition, GazeManager.Instance.GazeOrigin) < 2)
                {
                    transform.position = GazeManager.Instance.HitPosition + yShift;
                }
                else
                {
                    transform.position =
                        GazeManager.Instance.GazeOrigin + GazeManager.Instance.GazeNormal * distanceToCamera + yShift;
                }

                buttonTransforms.GetChild(0).GetComponent<Interpolator>().SetTargetPosition(new Vector3(-0.07f, 0, 0));
                buttonTransforms.GetChild(1).GetComponent<Interpolator>()
                    .SetTargetPosition(new Vector3(-0.025f, 0.03f, 0));
                buttonTransforms.GetChild(2).GetComponent<Interpolator>()
                    .SetTargetPosition(new Vector3(0.025f, 0.03f, 0));
                buttonTransforms.GetChild(3).GetComponent<Interpolator>().SetTargetPosition(new Vector3(0.07f, 0, 0));
                
                buttonTransforms.gameObject.SetActive(true);
            }
            else
            {
                buttonTransforms.GetChild(0).transform.localPosition = Vector3.zero;
                buttonTransforms.GetChild(1).transform.localPosition = Vector3.zero;
                buttonTransforms.GetChild(2).transform.localPosition = Vector3.zero;
                buttonTransforms.GetChild(3).transform.localPosition = Vector3.zero;
                
                buttonTransforms.gameObject.gameObject.SetActive(false);
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            Menu.IsActive.Value = false;
        }
    }
}