using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class Checkbox : MonoBehaviour, IInputClickHandler
    {
        public Sprite Unchecked;
        public Sprite Checked;

        private bool isChecked;
        
        public void OnInputClicked(InputClickedEventData eventData)
        {
            isChecked ^= true;
            GetComponentInChildren<SpriteRenderer>().sprite = isChecked ? Checked : Unchecked;
        }
    }
}