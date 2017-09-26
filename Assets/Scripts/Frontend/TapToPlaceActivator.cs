using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UniRx;

namespace Frontend
{
    public class TapToPlaceActivator : MonoBehaviour
    {
        public ApplicationManager AppManager;

        private void Start()
        {
            AppManager.AppState.IsPlacing.Subscribe(isPlacing =>
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