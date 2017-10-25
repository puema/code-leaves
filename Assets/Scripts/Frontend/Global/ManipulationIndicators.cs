using System;
using Core;
using HoloToolkit.Unity.InputModule;
using UniRx;
using UnityEngine;

namespace Frontend.Global
{
    public interface IManipulationIndicators
    {
        void Position();
        void Activate();
        void Deactivate();
        void ActivateHand();
        void ActivateIndicators();
        void UpdateHandPosition(Vector3 position);
    }

    public class ManipulationIndicators : MonoBehaviour, IManipulationIndicators
    {
        public ApplicationManager AppManager;

        public Sprite ScalePlus;
        public Sprite ScaleMinus;

        public Sprite RotateClockwise;
        public Sprite RotateCounterClock;

        public Sprite MoveLeft;
        public Sprite MoveUp;
        public Sprite MoveRight;
        public Sprite MoveDown;

        public float DistanceToCamera = 1.5f;
        public float DistanceBetweenIndicators = 0.04f;

        private const string LeftIndicator = "LeftIndicator";
        private const string TopIndicator = "TopIndicator";
        private const string RightIndicator = "RightIndicator";
        private const string BottomIndicator = "BottomIndicator";
        private const string NorthWestIndicator = "RightIndicator";
        private const string SouthEastIndicator = "BottomIndicator";
        private const string Hand = "Hand";

        private ManipulationMode mode;

        private void Start()
        {
            Deactivate();
            ResetIndicators();
            AppManager.AppState.UiElements.ManipulationIndicators.Mode.Subscribe(SetMode);
        }

        public void ResetIndicators()
        {
            transform.Find(LeftIndicator).localPosition = new Vector3(-DistanceBetweenIndicators / 2, 0, 0);
            transform.Find(TopIndicator).localPosition = new Vector3(0, DistanceBetweenIndicators / 2, 0);
            transform.Find(RightIndicator).localPosition = new Vector3(DistanceBetweenIndicators / 2, 0, 0);
            transform.Find(BottomIndicator).localPosition = new Vector3(0, -DistanceBetweenIndicators / 2, 0);
        }

        public void Position()
        {
            if (Vector3.Distance(GazeManager.Instance.HitPosition, GazeManager.Instance.GazeOrigin) < 2)
            {
                transform.position = GazeManager.Instance.HitPosition;
            }
            else
            {
                transform.position =
                    GazeManager.Instance.GazeOrigin + GazeManager.Instance.GazeNormal * DistanceToCamera;
            }
        }

        public void UpdateHandPosition(Vector3 position)
        {
            transform.Find(Hand).localPosition = position;

            if (position.x < 0)
                transform.Find(LeftIndicator).localPosition =
                    new Vector3(-DistanceBetweenIndicators / 2 + position.x, 0, 0);
            if (position.x > 0)
                transform.Find(RightIndicator).localPosition =
                    new Vector3(DistanceBetweenIndicators / 2 + position.x, 0, 0);

            if (position.y > 0)
                transform.Find(TopIndicator).localPosition =
                    new Vector3(0, DistanceBetweenIndicators / 2 + position.y, 0);
            if (position.y < 0)
                transform.Find(BottomIndicator).localPosition =
                    new Vector3(0, -DistanceBetweenIndicators / 2 + position.y, 0);
        }

        public void Deactivate()
        {
            SetActiveHand(false);
            SetActiveHorizonal(false);
            SetActiveVertical(false);
            ResetIndicators();
        }

        public void Activate()
        {
            ActivateIndicators();
            ActivateHand();
        }

        public void ActivateHand()
        {
            SetActiveHand(true);
        }

        public void ActivateIndicators()
        {
            switch (mode)
            {
                case ManipulationMode.Move:
                    SetActiveHorizonal(true);
                    SetActiveVertical(true);
                    break;
                case ManipulationMode.Scale:
                case ManipulationMode.Rotate:
                    SetActiveHorizonal(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void SetActiveHand(bool isActive)
        {
            transform.Find(Hand).gameObject.SetActive(isActive);
        }

        private void SetActiveHorizonal(bool isActive)
        {
            transform.Find(RightIndicator).gameObject.SetActive(isActive);
            transform.Find(LeftIndicator).gameObject.SetActive(isActive);
        }

        private void SetActiveVertical(bool isActive)
        {
            transform.Find(TopIndicator).gameObject.SetActive(isActive);
            transform.Find(BottomIndicator).gameObject.SetActive(isActive);
        }

        private void SetMode(ManipulationMode mode)
        {
            this.mode = mode;

            switch (mode)
            {
                case ManipulationMode.Move:
                    transform.Find(LeftIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite = MoveLeft;
                    transform.Find(TopIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite = MoveUp;
                    transform.Find(RightIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        MoveRight;
                    transform.Find(BottomIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        MoveDown;
                    break;
                case ManipulationMode.Scale:
                    transform.Find(LeftIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        ScaleMinus;
                    transform.Find(RightIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        ScalePlus;
                    break;
                case ManipulationMode.Rotate:
                    transform.Find(LeftIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        RotateClockwise;
                    transform.Find(RightIndicator).gameObject.GetComponentInChildren<SpriteRenderer>().sprite =
                        RotateCounterClock;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }
}