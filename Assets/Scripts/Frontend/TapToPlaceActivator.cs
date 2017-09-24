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
                GetComponent<TapToPlace>().enabled = isPlacing;
                GetComponent<TapToPlace>().IsBeingPlaced = isPlacing;
            });
        }
    }
}