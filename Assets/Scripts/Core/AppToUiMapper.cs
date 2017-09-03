using System.Linq;
using Frontend;
using UniRx;
using UnityEngine;

namespace Core
{
    public static class AppToUiMapper
    {
        public static UiNode Map(Node node)
        {
            if (node is InnerNode)
            {
                return new UiInnerNode
                {
                    Id = node.Key,
                    IsFocused = new ReactiveProperty<bool>(false),
                    IsSelected = new ReactiveProperty<bool>(false),
                    Text = new ReactiveProperty<string>(node.Name),
                    Children = ((InnerNode)node).Children.Select(Map).ToList()
                };
            }
            
            if (node is Leaf)
            {
                return new UiLeaf
                {
                    Id = node.Key,
                    IsFocused = new ReactiveProperty<bool>(false),
                    IsSelected = new ReactiveProperty<bool>(false),
                    Text = new ReactiveProperty<string>(node.Name),
                    Color = new ReactiveProperty<Color?>(HexToNullableColor("#208000"))
                };
            }
            
            return null;
        }
        
        public static Color? HexToNullableColor(string hexcolor)
        {
            Color color;
            return ColorUtility.TryParseHtmlString(hexcolor, out color) ? new Color?(color) : null;
        }
    }
}