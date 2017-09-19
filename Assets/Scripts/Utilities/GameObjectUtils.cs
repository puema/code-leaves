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

    public static class GameObjectUtils
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
                    return obj.transform.localScale.x * size / obj.GetSize(Axis.X);
                case Axis.Y:
                    return obj.transform.localScale.y * size / obj.GetSize(Axis.Y);
                case Axis.Z:
                    return obj.transform.localScale.z * size / obj.GetSize(Axis.Z);
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

//        public static void SizeToScale(this GameObject obj, float x = 0, float y = 0, float z = 0)
//        {
//            var xScale = obj.transform.localScale.x * x / obj.GetSize(Axis.X);
//            var yScale = obj.transform.localScale.y * y / obj.GetSize(Axis.Y);
//            var zScale = obj.transform.localScale.z * z / obj.GetSize(Axis.Z);
//            obj.transform.localScale = new Vector3(xScale, yScale, zScale);
//        }
    }
}