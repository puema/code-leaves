using Frontend.Tree;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using ManipulationIndicators = Frontend.Global.ManipulationIndicators;

namespace Utilities
{
    public class DragToScale : MonoBehaviour, IManipulationHandler, IHoldHandler
    {
        public GameObject Target;
        public ManipulationIndicators ManipulationIndicators;
        
        public float ScaleFactor = 1;
        public float MinSize = 0.5f;
        public float MinScale = 0.01f;
        public float MaxScale = 10;

        private float originalScale;
        private Camera mainCamera;
        private bool manipulationStartedOnThisGameObject;

        private void Start()
        {
            if (Target == null) Target = gameObject;
            mainCamera = CameraCache.Main;
        }

        private void SetMinMaxScale()
        {
            MinScale = gameObject.SizeToScale(Axis.X, MinScale);
        }
        
        public void OnHoldStarted(HoldEventData eventData)
        {
            ManipulationIndicators.Position();
            ManipulationIndicators.ActivateIndicators();
        }

        public void OnHoldCompleted(HoldEventData eventData)
        {
            ManipulationIndicators.Deactivate();
        }

        public void OnHoldCanceled(HoldEventData eventData)
        {
            ManipulationIndicators.Deactivate();
        }

        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            manipulationStartedOnThisGameObject = true;
            originalScale = Target.transform.localScale.x;

            SetMinMaxScale();

            ManipulationIndicators.Position();
            ManipulationIndicators.ActivateHand();
            ManipulationIndicators.ActivateIndicators();

            InputManager.Instance.PushModalInputHandler(gameObject);
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            if (!manipulationStartedOnThisGameObject) return;
            var direction = mainCamera.transform.InverseTransformDirection(eventData.CumulativeDelta);
            ManipulationIndicators.UpdateHandPosition(new Vector3(direction.x, 0, 0));
            Scale(direction.x);
        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            if (!manipulationStartedOnThisGameObject) return;
            ManipulationIndicators.Deactivate();
            manipulationStartedOnThisGameObject = false;
            InputManager.Instance.PopModalInputHandler();
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            if (!manipulationStartedOnThisGameObject) return;
            ManipulationIndicators.Deactivate();
            manipulationStartedOnThisGameObject = false;
            InputManager.Instance.PopModalInputHandler();
            
            ApplyScale(originalScale);
        }

        private void Scale(float delta)
        {
            var scale = Mathf.Clamp(originalScale + delta * ScaleFactor, MinScale, MaxScale);
            ApplyScale(scale);
        }

        private void ApplyScale(float scale)
        {
            Target.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}