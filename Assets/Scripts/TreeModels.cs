using System;
using System.Collections.Generic;

namespace AbstractNode
{
    public abstract class Node
    {
    }

    public class InnerNode : Node
    {
        public InnerNodeData Data;
        public List<Node> Children;
    }

    public class Leaf : Node
    {
        public LeafData Data;
    }

    public struct InnerNodeData
    {
        public string Id { get; set; }
    }

    public struct LeafData
    {
        public string Id { get; set; }
        public string Color { get; set; }
    }
}


// ----==== Without abstract to be better serializable ====---- //
namespace NonAbstractNode
{
    public class InnerNode
    {
        public InnerNodeData Data { get; set; }
        public List<InnerNode> ChildNodes { get; set; }
        public List<Leaf> Leaves { get; set; }
    }

    public class Leaf
    {
        public LeafData Data { get; set; }
    }

    public struct InnerNodeData
    {
        public string Id { get; set; }
    }

    public struct LeafData
    {
        public string Id { get; set; }
        public string Color { get; set; }
    }
}