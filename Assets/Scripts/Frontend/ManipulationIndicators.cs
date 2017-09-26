using System.IO;
using Core;
using UniRx;
using UnityEngine;

namespace Frontend
{
    public class ManipulationIndicators : MonoBehaviour
    {
        public ApplicationManager AppManager;

        private void Start()
        {
            AppManager.AppState.ManipulationIndicators.IsActive.Subscribe(SetActive);
            AppManager.AppState.ManipulationIndicators.Icons.Subscribe(SetIcons);
        }

        private void SetActive(bool isActive)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
            transform.GetChild(1).gameObject.SetActive(isActive);
        }

        private void SetIcons(ManipulationIndicatorIcons icons)
        {
            transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite =
                Resources.Load<Sprite>(Path.Combine("Sprites", icons.Left));
            transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite =
                Resources.Load<Sprite>(Path.Combine("Sprites", icons.Right));
        }
    }
}