using Core;
using HoloToolkit.Unity;
using UnityEngine;
using UniRx;

namespace Frontend
{
    public class ProjectMenu : MonoBehaviour
    {
        public ApplicationManager AppManager;
            
        private void Start()
        {
            AppManager.AppState.UiElements.ProjectMenu.IsActive.Subscribe(transform.GetChild(0).gameObject.SetActive);
            AppManager.AppState.UiElements.ProjectMenu.IsTagalong.Subscribe(tagalong =>
                gameObject.GetComponent<Tagalong>().enabled = tagalong);
        }
    }
}