using Core;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;

namespace Frontend.Global
{
    public class GazeText : Singleton<GazeText>
    {
        public ApplicationManager AppManager;

        private void Start()
        {
            AppManager.AppState.UiElements.GazeText.IsActive.Subscribe(SetActive);
            AppManager.AppState.UiElements.GazeText.Text.Subscribe(SetText);
        }

        private void SetActive(bool isActive)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
        }

        private void SetText(string text)
        {
            transform.GetChild(0).GetComponent<TextMesh>().text = text;
        }
    }
}