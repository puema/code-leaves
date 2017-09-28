using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;
using Utilities;

namespace Frontend.Global
{
    public class GazeText : Singleton<GazeText>
    {
        public ApplicationManager AppManager;
        public float Offset = 0.01f;

        private bool isActive;

        private void Start()
        {
            AppManager.AppState.UiElements.GazeText.IsActive.Subscribe(SetActive);
            AppManager.AppState.UiElements.GazeText.Text.Subscribe(SetText);
        }

        private void SetActive(bool isActive)
        {
            this.isActive = isActive;
            transform.GetChild(0).gameObject.SetActive(isActive);
        }

        private void SetText(string text)
        {
            transform.GetChild(0).GetComponent<TextMesh>().text = text;
        }

        private void Update()
        {
            if (!isActive) return;
            var hitPosition = GazeManager.Instance.HitPosition;
            var offsetDirection = CameraCache.Main.transform.up;
            var newPosition = hitPosition + offsetDirection * (gameObject.GetSize(Axis.Y) / 2 + Offset);
            transform.position = newPosition;
        }
    }
}