using System.Collections.Generic;
using System.Linq;

namespace AbstractNode
{
    public abstract class Node
    {
        public int Height { get; set; }
        public int ChildCount { get; set; }

        public abstract int AddHeight();

        public abstract int AddChildCount();

        public abstract void SortChildren();
    }

    public class InnerNode : Node
    {
        public InnerNodeData Data { get; set; }
        public List<Node> Children { get; set; }

        public override int AddHeight()
        {
            if (Children == null) return 0;

            // Calculate max depth of each child, select the heighest and add to own depth
            var maxChildrenHeight = Children.Select(child => child.AddHeight()).Concat(new[] {0}).Max() + 1;

            Height = maxChildrenHeight;
            return maxChildrenHeight;
        }

        public override int AddChildCount()
        {
            if (Children == null) return 0;

            var childCount = Children.Select(child => child.AddChildCount() + 1).Sum();

            ChildCount = childCount;
            return childCount;
        }

        public override void SortChildren()
        {
            Children = Children?.OrderByDescending(node => node.Height).ThenByDescending(node => node.ChildCount)
                .ToList();
        }
    }

    public class Leaf : Node
    {
        public LeafData Data { get; set; }

        public override int AddHeight()
        {
            Height = 0;
            return 0;
        }

        public override int AddChildCount()
        {
            return 0;
        }

        public override void SortChildren()
        {
        }
    }

    public struct InnerNodeData
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    public struct LeafData
    {
        public string Id { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
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