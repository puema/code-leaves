using System;
using System.Collections.Generic;
using UniRx;

namespace Core
{
    public class AppState
    {
        public ReactiveProperty<FloorInteractionMode> FloorInteractionMode { get; set; }
        public AppData AppData { get; set; }
    }
    
    public enum FloorInteractionMode {
        TapToMenu, TapToPlace, DragToScale, DragToRotate
    }

    public class AppData
    {
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Dictionary<string, double> SnapshotProperties { get; set; }
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
        public double Value { set; get; }
        public List<string> Infos { get; set; }
    }

    public struct InnerNodeData
    {
        public string Key { get; set; }
        public double Value { set; get; }
        public List<string> Infos { get; set; }
    }
}