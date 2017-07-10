using System.Collections.Generic;
using System.Linq;

namespace AbstractNode
{
    public abstract class Node
    {
        public int Depth { get; set; }

        public int AddMaxDepth(int depth)
        {
            if (GetType() == typeof(Leaf) || ((InnerNode) this).Children == null)
            {
                Depth = depth;
                return 0;
            }
            var innerNode = (InnerNode) this;

            depth++;

            var maxChildDepth = innerNode.Children.Select(child => child.AddMaxDepth(depth)).Concat(new[] {0}).Max();

            depth += maxChildDepth;
            Depth = depth;
            return depth;
        }

        public void SortChildren()
        {
            if (GetType() == typeof(Leaf) || ((InnerNode) this).Children == null) return;
            var innerNode = (InnerNode) this;

            innerNode.Children = innerNode.Children.OrderByDescending(node => node.Depth).ToList();
        }
    }

    public class InnerNode : Node
    {
        public InnerNodeData Data { get; set; }
        public List<Node> Children { get; set; }
    }

    public class Leaf : Node
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