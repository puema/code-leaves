using System;
using System.Collections.Generic;
using Frontend;
using Frontend.Models;
using UniRx;

namespace Core
{
    public class UiElements
    {
        public GazeText GazeText { get; set; }
        public ContextMenu ContexMenu { get; set; }
        public ProjectMenu AppMenu { get; set; }
        public ReactiveProperty<bool> IsPlacing { get; set; }
        public ManipulationIndicators ManipulationIndicators { get; set; }
        public ReactiveProperty<ManipulationMode> ForestManipulationMode { get; set; }
    } 
    
    public enum ManipulationMode {
        Scale, Rotate, Move
    }
    
    public class GazeText
    {
        public ReactiveProperty<bool> IsActive { get; set; }
        public ReactiveProperty<string> Text { get; set; }
    }
    
    public class ContextMenu
    {
        public ReactiveProperty<bool> IsActive { get; set; }
        public ReactiveProperty<ContextMenuButton[]> Buttons { get; set; }
    }
    
    public class ProjectMenu
    {
        public ReactiveProperty<bool> IsActive { get; set; }
        public ReactiveProperty<bool> IsTagalong { get; set; }
    }
    
    public class ManipulationIndicators
    {
        public ReactiveProperty<bool> IsActive;
        public ReactiveProperty<ManipulationMode> Mode;
    }

    public class ContextMenuButton
    {
        public string Icon { get; set; }
        public string Text  { get; set; }
        public Action Action { get; set; }
    }

    public class AppState
    {
        public string[] AvailableExampleProjects { get; set; }
        public UiElements UiElements { get; set; }
        public ReactiveProperty<Forest> Forest { get; set; }
        public ReactiveProperty<AppData> AppData { get; set; }
    }

    public class AppData
    {
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Dictionary<string, float> SnapshotProperties { get; set; }
        public Node Root { get; set; }
    }

    public abstract class Node
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public Edge Edge { get; set; }
    }

    public class InnerNode : Node
    {
        public List<Node> Children { get; set; }
        public List<InnerNodeData> Data { get; set; }
    }

    public class Leaf : Node
    {
        public List<LeafData> Data { get; set; }
    }

    public class Edge
    {
        public Dictionary<string, AggregatedConnection> Connections { get; set; }
    }

    public class AggregatedConnection
    {
        public List<string> IngoingConnections { get; set; }
        public List<string> OutgoingConnections { get; set; }
    }

    public abstract class DirectConnections
    {
        public string Key { get; set; }
        public string Info { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public struct LeafData
    {
        public string Key { get; set; }
        public float Value { set; get; }
        public List<string> Infos { get; set; }
    }

    public struct InnerNodeData
    {
        public string Key { get; set; }
        public float Value { set; get; }
        public List<string> Infos { get; set; }
    }
}