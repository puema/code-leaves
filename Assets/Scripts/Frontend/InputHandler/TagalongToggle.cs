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
        public GameObject AppMenu;
        public GameObject Icon;
        public Sprite DisableIcon;
        public Sprite EnableIcon;

        private void Start()
        {
            transform.parent.GetComponentInChildren<HandDraggable>().StartedDragging +=
                InteractionManager.Instance.HandleAppMenuHandDrag;
            AppManager.AppState.UiElements.AppMenu.IsTagalong.Subscribe(SetTagalong);
        }

        private void SetTagalong(bool active)
        {
            AppMenu.GetComponent<Tagalong>().enabled = active;
            Icon.GetComponent<SpriteRenderer>().sprite = active ? DisableIcon : EnableIcon;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            InteractionManager.Instance.HandleProjectMenuTagalongToggle();
        }
    }
}