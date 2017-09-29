using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Frontend.Models;
using HoloToolkit.Unity;
using UnityEngine;
using Utilities;

namespace Core
{
    public class NodeStabilizationListEntry
    {
        public string nodeId { get; set; }
        public float focusedTime { get; set; }
    }

    public class NodeFocusManager : Singleton<NodeFocusManager>
    {
        public ApplicationManager AppManager;

        private const float MinFocusSecs = 1;

        private string lastFocusedNode;
        private Coroutine lastEnableCoroutine;
        private Coroutine lastDisableCoroutine;

        private string stabilizedFocusedNode;

        public void HandleFocusEnter(string id)
        {
            // same node as before
            if (id == lastFocusedNode)
            {
                // stop diabling the node
                StopCoroutine(lastDisableCoroutine);
                // and enable it again (necessary if disabling is allready finished)
                EnableFocus(id);
                return;
            }
            
            // new node is focused, disable last focus immediately
            DisableFocus(lastFocusedNode);
            
            // and stop last enabling
            if (lastEnableCoroutine != null)
            {
                StopCoroutine(lastEnableCoroutine);
            }

            lastEnableCoroutine = StartCoroutine(DelayFocusEnabling(id));

            lastFocusedNode = id;
        }

        public void HandleFocusExit(string id)
        {
            lastDisableCoroutine = StartCoroutine(DelayFocusDisabling(id));
        }

        private IEnumerator DelayFocusEnabling(string id)
        {
            yield return new WaitForSecondsRealtime(MinFocusSecs);
            EnableFocus(id);
        }

        private IEnumerator DelayFocusDisabling(string id)
        {
            yield return new WaitForSecondsRealtime(MinFocusSecs);
            DisableFocus(id);
        }

        private void EnableFocus(string id)
        {
            var focused = AppManager.AppState.Forest.Value.Root.Find(id);
            if (focused == null) return;
            focused
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsFocused.Value = true);
            AppManager.AppState.UiElements.GazeText.Text.Value = focused.Text.Value;
            AppManager.AppState.UiElements.GazeText.IsActive.Value = true;
        }

        private void DisableFocus(string id)
        {
            var focused = AppManager.AppState.Forest.Value.Root.Find(id);
            if (focused == null) return;
            focused
                .Traverse(x => (x as UiInnerNode)?.Children)
                .ToList()
                .ForEach(x => x.IsFocused.Value = false);
            AppManager.AppState.UiElements.GazeText.IsActive.Value = false;
            AppManager.AppState.UiElements.GazeText.Text.Value = "";
        }
    }
}