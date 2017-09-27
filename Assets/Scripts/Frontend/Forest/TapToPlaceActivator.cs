using Core;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;

namespace Frontend.Forest
{
    public class TapToPlaceActivator : MonoBehaviour
    {
        public ApplicationManager AppManager;

        private void Start()
        {
            AppManager.AppState.UiElements.IsPlacing.Subscribe(isPlacing =>
            {
                var tapToPlace = GetComponent<TapToPlace>();
                if (!isPlacing)
                {
                    tapToPlace.enabled = false;
                }
                else
                {
                    tapToPlace.enabled = true;
                    tapToPlace.HandlePlacement();
                }
            });
        }
    }
}