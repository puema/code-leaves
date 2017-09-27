using Core;
using HoloToolkit.Unity;
using UniRx;

namespace Frontend
{
    public class ForestManipulationModeSwitcher : Singleton<ForestManipulationModeSwitcher>
    {
        private void Start()
        {
            ApplicationManager.Instance.AppState.UiElements.ForestManipulationMode.Subscribe(mode =>
            {
                ResetInteractionMode();
                
                switch (mode)
                {
                    case ManipulationMode.Scale:
                        GetComponent<DragToScale>().enabled = true;
                        break;
                    case ManipulationMode.Rotate:
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