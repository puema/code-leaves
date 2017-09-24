using Core;
using HoloToolkit.Unity;
using UniRx;

namespace Frontend
{
    public class ForestManipulationModeSwitcher : Singleton<ForestManipulationModeSwitcher>
    {
        private void Start()
        {
            ApplicationManager.Instance.AppState.ForestManipulationMode.Subscribe(mode =>
            {
                ResetInteractionMode();
                
                switch (mode)
                {
                    case ForestManipulationMode.DragToScale:
                        GetComponent<DragToScale>().enabled = true;
                        break;
                    case ForestManipulationMode.DragToRotate:
                        GetComponent<DragToRotate>().enabled = true;
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
        }
    }
}