using Core;
using UnityEngine;
using UniRx;

namespace Frontend.AppMenu
{
    public class AppMenuBack : MonoBehaviour
    {
        public ApplicationManager AppManager;

        private void Start()
        {
            AppManager.AppState.UiElements.AppMenu.BackAvailable.Subscribe(SetActive);
        }

        private void SetActive(bool isActive)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
        }
    }
}