using Core;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class AppMenu : MonoBehaviour
    {
        public ApplicationManager AppManager;
            
        private void Start()
        {
            AppManager.AppState.UiElements.AppMenu.IsActive.Subscribe(transform.GetChild(0).gameObject.SetActive);
            AppManager.AppState.UiElements.AppMenu.IsTagalong.Subscribe(tagalong =>
                gameObject.GetComponent<Tagalong>().enabled = tagalong);
        }
    }
}