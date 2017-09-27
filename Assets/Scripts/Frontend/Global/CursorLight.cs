using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace Frontend.Global
{
    public class CursorLight : MonoBehaviour
    {
        public GameObject Light;

        private void Start()
        {
            GazeManager.Instance.FocusedObjectChanged += (previousObject, newObject) =>
            {
                var hitObject = GazeManager.Instance.HitObject;
                if (hitObject != null && (hitObject == gameObject || hitObject.transform.IsChildOf(transform)))
                {
                    Light.SetActive(true);
                }
                else
                {
                    Light.SetActive(false);
                }
            };
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            Light.transform.position = GazeManager.Instance.HitPosition - GazeManager.Instance.GazeNormal * 0.01f;
        }
    }
}