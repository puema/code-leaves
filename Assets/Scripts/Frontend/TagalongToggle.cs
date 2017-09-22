using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
    public class TagalongToggle : MonoBehaviour, IInputClickHandler
    {
        public GameObject Window;
        public GameObject Icon;
        public Sprite DisableIcon;
        public Sprite EnableIcon;

        private void Start()
        {
            transform.parent.GetComponentInChildren<HandDraggable>().StartedDragging += DisableTagalong;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            ToggleTagalong();
        }

        private void ToggleTagalong()
        {
            Window.GetComponent<Tagalong>().enabled ^= true;
            Icon.GetComponent<SpriteRenderer>().sprite =
                Window.GetComponent<Tagalong>().enabled ? DisableIcon : EnableIcon;
        }

        private void DisableTagalong()
        {
            Window.GetComponent<Tagalong>().enabled = false;
            Icon.GetComponent<SpriteRenderer>().sprite = EnableIcon;
        }
    }
}