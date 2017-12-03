using System.Linq;
using Frontend.Models;
using UniRx;
using UnityEngine;

namespace Core
{
    public static class AppToUiMapper
    {
        public static UiNode Map(Node node)
        {
            var innerNode = node as InnerNode;
            if (innerNode != null)
            {
                return new UiInnerNode
                {
                    Id = innerNode.Key,
                    IsFocused = new ReactiveProperty<bool>(false),
                    IsSelected = new ReactiveProperty<bool>(false),
                    Text = new ReactiveProperty<string>(innerNode.Name),
                    Children = innerNode.Children.Select(Map).ToList()
                };
            }

            var leaf = node as Leaf;
            if (leaf != null)
            {
                return new UiLeaf
                {
                    Id = leaf.Key,
                    IsFocused = new ReactiveProperty<bool>(false),
                    IsSelected = new ReactiveProperty<bool>(false),
                    Text = new ReactiveProperty<string>(leaf.Data == null
                        ? "Metric not available"
                        : leaf.Data[0].Value + "%"),
                    Color = new ReactiveProperty<Color?>(PercentageToNullableColor(leaf))
                };
            }

            return null;
        }

        public static Color? HexToNullableColor(string hexcolor)
        {
            Color color;
            return ColorUtility.TryParseHtmlString(hexcolor, out color) ? new Color?(color) : null;
        }

        public static Color? PercentageToNullableColor(Leaf leaf)
        {
            if (leaf.Data == null || leaf.Data.Count == 0) return null;
            var factor = 1 - leaf.Data[0].Value / 100;
            return new Color(2.0f * factor, 2.0f * (1 - factor), 0);
        }
    }
}