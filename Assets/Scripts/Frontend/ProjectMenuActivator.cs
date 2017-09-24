using Core;
using HoloToolkit.Unity;
using UnityEngine;
using UniRx;

namespace Frontend
{
    public class ProjectMenuActivator : MonoBehaviour
    {
        public ApplicationManager AppManager;
        public GameObject ProjectMenu;
            
        private void Start()
        {
            AppManager.AppState.ProjectMenu.IsActive.Subscribe(ProjectMenu.SetActive);
            AppManager.AppState.ProjectMenu.IsTagalong.Subscribe(tagalong =>
                ProjectMenu.GetComponent<Tagalong>().enabled = tagalong);
        }
    }
}