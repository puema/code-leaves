using System;
using Core;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;

namespace Frontend.AppMenu
{
    public class AppMenu : MonoBehaviour
    {
        public ApplicationManager AppManager;
        public GameObject ProjectSelection;
        public GameObject Settings;

        private Core.AppMenu appMenu;

        private void Start()
        {
            appMenu = AppManager.AppState.UiElements.AppMenu;
            appMenu.IsActive.Subscribe(transform.GetChild(0).gameObject.SetActive);
            appMenu.IsTagalong.Subscribe(tagalong =>
                gameObject.GetComponent<Tagalong>().enabled = tagalong);
            appMenu.Page.Subscribe(SetPage);
        }

        private void SetPage(AppMenuPage page)
        {
            ResetPages();
            
            switch (page)
            {
                case AppMenuPage.ProjectSelection:
                    ProjectSelection.SetActive(true);
                    ProjectSelection.transform.localPosition = Vector3.zero;
                    break;
                case AppMenuPage.Settings:
                    Settings.SetActive(true);
                    Settings.transform.localPosition = Vector3.zero;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }

        private void ResetPages()
        {
            ProjectSelection.SetActive(false);
            Settings.SetActive(false);
        }
    }
}