using System;
using UnityEngine;

namespace Utilities
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public static class GameObjectExtensions
    {
        public static float GetSize(this GameObject obj, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return obj.GetComponentInChildren<MeshRenderer>().bounds.size.x;
                case Axis.Y:
                    return obj.GetComponentInChildren<MeshRenderer>().bounds.size.y;
                case Axis.Z:
                    return obj.GetComponentInChildren<MeshRenderer>().bounds.size.z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        public static float SizeToScale(this GameObject obj, Axis axis, float size)
        {
            switch (axis)
            {
                case Axis.X:
                    return obj.transform.lossyScale.x * size / obj.GetSize(Axis.X);
                case Axis.Y:
                    return obj.transform.lossyScale.y * size / obj.GetSize(Axis.Y);
                case Axis.Z:
                    return obj.transform.lossyScale.z * size / obj.GetSize(Axis.Z);
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
    }
}