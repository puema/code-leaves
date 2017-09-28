using Frontend.Global;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Utilities
{
    public class DragToRotate : MonoBehaviour, IManipulationHandler, IHoldHandler
    {
        public GameObject Target;
        public ManipulationIndicators ManipulationIndicators;

        public float RotateFactor = 100;

        private float originalRotation;
        private Camera mainCamera;
        private bool manipulationStartedOnThisGameObject;

        private void Start()
        {
            if (Target == null) Target = gameObject;
            mainCamera = CameraCache.Main;
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
            originalRotation = Target.transform.localEulerAngles.y;

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
            Rotate(direction.x);
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
            Debug.Log("Manipulation canceled");
            ManipulationIndicators.Deactivate();
            manipulationStartedOnThisGameObject = false;
            InputManager.Instance.PopModalInputHandler();
            
            ApplyRotation(originalRotation);
        }

        private void Rotate(float delta)
        {
            var rotation = originalRotation - delta * RotateFactor;
            ApplyRotation(rotation);
        }

        private void ApplyRotation(float rotation)
        {
            Target.transform.localEulerAngles = Vector3.up * rotation;
        }
    }
}