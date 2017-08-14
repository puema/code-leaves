using Core;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend
{
    public class DragToRotate : MonoBehaviour, IManipulationHandler
    {
        public GameObject Target;
        public float RotateFactor = 10;

        private float OriginalRotation;

        private void Start()
        {
            if (Target == null) Target = gameObject;
        }

        public void OnManipulationStarted(ManipulationEventData eventData)
        {
            OriginalRotation = Target.transform.localEulerAngles.y;
            InputManager.Instance.PushModalInputHandler(gameObject);
            Rotate(eventData.CumulativeDelta.x);
        }

        public void OnManipulationUpdated(ManipulationEventData eventData)
        {
            Rotate(eventData.CumulativeDelta.x);
        }

        public void OnManipulationCompleted(ManipulationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
            InteractionManager.Instance.HandleFloorInteractionCompleted();
        }

        public void OnManipulationCanceled(ManipulationEventData eventData)
        {
            ApplyRotation(OriginalRotation);
        }

        private void Rotate(float delta)
        {
            var rotation = OriginalRotation - delta * RotateFactor;
            ApplyRotation(rotation);
        }

        private void ApplyRotation(float rotation)
        {
            Target.transform.localEulerAngles = Vector3.up * rotation;
        }
    }
}