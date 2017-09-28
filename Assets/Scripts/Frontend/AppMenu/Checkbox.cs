using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class Checkbox : MonoBehaviour
    {
        public Sprite Unchecked;
        public Sprite Checked;

        public void SetChecked(bool isChecked)
        {
            GetComponentInChildren<SpriteRenderer>().sprite = isChecked ? Checked : Unchecked;
        }
    }
}