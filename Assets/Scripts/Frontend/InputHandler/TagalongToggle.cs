using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UniRx;

namespace Frontend
{
    public class TagalongToggle : MonoBehaviour, IInputClickHandler
    {
        public ApplicationManager AppManager;
        public GameObject Window;
        public GameObject Icon;
        public Sprite DisableIcon;
        public Sprite EnableIcon;

        private void Start()
        {
            transform.parent.GetComponentInChildren<HandDraggable>().StartedDragging +=
                InteractionManager.Instance.HandleProjectMenuHandDrag;
            AppManager.AppState.UiElements.ProjectMenu.IsTagalong.Subscribe(SetTagalong);
        }

        private void SetTagalong(bool active)
        {
            Window.GetComponent<Tagalong>().enabled = active;
            Icon.GetComponent<SpriteRenderer>().sprite = active ? DisableIcon : EnableIcon;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleProjectMenuTagalongToggle();
        }
    }
}