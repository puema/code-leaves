using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Forest
{
    public Floor Floor { get; set; }
    public List<UiNode> Trees { get; set; }
}

public class Floor
{
    public Color Color { get; set; }
}

public abstract class UiNode
{
    public string Id;
    
    public ReactiveProperty<string> Text { get; set; }
    public ReactiveProperty<bool> IsSelected { get; set; }
    public ReactiveProperty<bool> IsFocused { get; set; }

    public abstract int GetHeight();

    public abstract int GetChildCount();

    public abstract void SortChildren();
}

public class UiInnerNode : UiNode
{
    public ReactiveProperty<float> Thickness { set; get; }
    
    public List<UiNode> Children { get; set; }


    public override int GetHeight()
    {
        if (Children == null) return 0;

        // Calculate max depth of each child, select the heighest and add to own depth
        var maxChildrenHeight = Children.Select(child => child.GetHeight()).Concat(new[] {0}).Max() + 1;

        return maxChildrenHeight;
    }

    public override int GetChildCount()
    {
        if (Children == null) return 0;

        var childCount = Children.Select(child => child.GetChildCount() + 1).Sum();

        return childCount;
    }

    public override void SortChildren()
    {
        Children = Children?
            .OrderByDescending(node => node.GetHeight())
            .ThenByDescending(node => node.GetChildCount())
            .ToList();
        Children?
            .ForEach(x => x.SortChildren());
    }
}

public class UiLeaf : UiNode
{
    public ReactiveProperty<string> Color { get; set; }

    public override int GetHeight()
    {
        return 0;
    }

    public override int GetChildCount()
    {
        return 0;
    }

    public override void SortChildren()
    {
    }
}

