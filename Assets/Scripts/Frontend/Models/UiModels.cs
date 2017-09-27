using System.Collections.Generic;
using System.Linq;
using Frontend.Tree;
using UniRx;
using UnityEngine;

namespace Frontend.Models
{
    public class Forest
    {
        public Floor Floor { get; set; }
        public UiNode Root { get; set; }
        public ReactiveProperty<bool> VisualizeCircles { get; set; }
    }

    public class Floor
    {
        public Color Color { get; set; }
    }

    public abstract class UiNode
    {
        public string Id { get; set; }

        public abstract Circle Circle { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<bool> IsSelected { get; set; }
        public ReactiveProperty<bool> IsFocused { get; set; }

        public abstract int GetHeight();

        public abstract int GetDescendantsCount();

        public abstract int GetWidth();

        public abstract void SortChildren();
    }

    public class UiInnerNode : UiNode
    {
        public ReactiveProperty<float> Thickness { set; get; }
        
        public override Circle Circle { get; set; } = new Circle();

        public List<UiNode> Children { get; set; }

        public override int GetHeight()
        {
            // Calculate max depth of each child, select the heighest and add to own depth
            return Children?.Select(child => child.GetHeight()).Max() + 1 ?? 0;
        }

        public override int GetDescendantsCount()
        {
            return Children?.Select(child => child.GetDescendantsCount() + 1).Sum() ?? 0;
        }

        public override int GetWidth()
        {
            return Children?.Select(child => child.GetWidth()).Sum() ?? 0;
        }

        public override void SortChildren()
        {
            Children = Children?
                .OrderByDescending(node => node.GetHeight())
                .ThenByDescending(node => node.GetDescendantsCount())
                .ToList();
            Children?
                .ForEach(x => x.SortChildren());
        }
    }

    public class Circle
    {
        public float Radius { get; set; } = 0;
        public ReactiveProperty<Vector2> Position { get; set; } = new ReactiveProperty<Vector2>(new Vector2(0, 0));
    }

    public class UiLeaf : UiNode
    {
        public ReactiveProperty<Color?> Color { get; set; }
        
        public override Circle Circle { get; set; } = new Circle{ Radius = TreeGeometry.NodeDistanceFactor / 2 };

        public override int GetHeight()
        {
            return 0;
        }

        public override int GetDescendantsCount()
        {
            return 0;
        }

        public override int GetWidth()
        {
            return 1;
        }

        public override void SortChildren()
        {
        }
    }
}