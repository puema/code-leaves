using System.Collections.Generic;
using System.Linq;

namespace AbstractNode
{
    public abstract class Node
    {
        public int Depth { get; set; }

        public abstract int AddMaxDepth(int depth = 0);

        public abstract void SortChildren();
    }

    public class InnerNode : Node
    {
        public InnerNodeData Data { get; set; }
        public List<Node> Children { get; set; }

        public override int AddMaxDepth(int depth = 0)
        {
            if (Children == null) return 0;
            
            depth++;

            // Calculate max depth of each child, select the heighest and add to own depth
            var maxChildDepth = Children.Select(child => child.AddMaxDepth(depth)).Concat(new[] {0}).Max();
            depth += maxChildDepth;
            
            Depth = depth;
           
            return depth;
        }
        
        public override void SortChildren()
        {
            if (Children == null) return;
            Children = Children.OrderByDescending(node => node.Depth).ToList();
        }
    }

    public class Leaf : Node
    {
        public LeafData Data { get; set; }

        public override int AddMaxDepth(int depth = 0)
        {
            Depth = depth;
            return 0;
        }
        
        public override void SortChildren()
        {
        }
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