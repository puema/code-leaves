using System.ComponentModel;
using Core;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UniRx;

namespace Frontend
{
    public class FloorInputSwitcher : Singleton<FloorInputSwitcher>
    {
        private void Start()
        {
            ApplicationManager.Instance.AppState.FloorInteractionMode.Subscribe(mode =>
            {
                ResetInteractionMode();

                switch (mode)
                {
                    case FloorInteractionMode.DragToScale:
                        GetComponent<DragToScale>().enabled = true;
                        break;
                    case FloorInteractionMode.DragToRotate:
                        GetComponent<DragToRotate>().enabled = true;
                        break;
                    case FloorInteractionMode.TapToPlace:
                        var tapToPlace = GetComponentInChildren<TapToPlace>();
                        tapToPlace.enabled = true;
                        tapToPlace.IsBeingPlaced = true;
                        break;

                    case FloorInteractionMode.TapToMenu:
                        break;
                    default:
                        ResetInteractionMode();
                        break;
                }
            });
        }

        private void ResetInteractionMode()
        {
            GetComponent<DragToScale>().enabled = false;
            GetComponent<DragToRotate>().enabled = false;
            GetComponentInChildren<TapToPlace>().enabled = false;
        }
    }
}