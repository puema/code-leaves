﻿using System;
using UnityEngine;
using Random = System.Random;

namespace Frontend
{
    public static class TreeGeometry
    {
        public const float NodeDistanceFactor = 0.05f;
        public static readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;
        public static readonly double GoldenAngle = RadianToDegree(2 * Math.PI / Math.Pow(GoldenRatio, 2));
        
        private static Random Random = new Random();

        /// <summary>
        /// Calculates the distance between the central node and the nth sibling
        /// </summary>
        /// <param name="node"></param>
        /// <param name="n"></param>
        /// <returns>Radius</returns>
        public static float CalcRadius(UiInnerNode node, int n)
        {
            if (n == 0)
                return 0;

            if (node.Children[n] is UiLeaf)
                return (float) Math.Sqrt(n) * NodeDistanceFactor;

            return (float) (Math.Sqrt(node.Children[0].GetWidth()) + Math.Sqrt(node.Children[n].GetWidth())) *
                   NodeDistanceFactor;
        }

        /// <summary>
        /// Calculates the azimuth angle, the rotation of an edge around the y-axis
        /// </summary>
        /// <param name="initial">Initial angle</param>
        /// <param name="n">Nth point</param>
        /// <returns>Azimuth angle</returns>
        public static float CalcPhi(int n, float initial)
        {
            return initial + RadianToDegree(2 * Math.PI * n / Math.Pow(GoldenRatio, 2));
        }

        /// <summary>
        /// Calculates the polar angel, the rotation of an edge around the x-axis
        /// </summary>
        /// <param name="h">Height of the unscaled, unrotated edge that belongs to central node A</param>
        /// <param name="r">The distance between node A and a sibling node B</param>
        /// <returns>Polar angle</returns>
        public static float CalcTheta(float h, float r)
        {
            return RadianToDegree(Math.Atan(r / h));
        }

        /// <summary>
        /// Calculates the length of an edge
        /// </summary>
        /// <param name="h">Height of the unscaled, unrotated edge</param>
        /// <param name="theta">Polar angel, the rotation of an edge around the x-axis</param>
        /// <returns></returns>
        public static float CalcEdgeLength(float h, float theta)
        {
            return h / (float) Math.Cos(DegreeToRadian(theta));
        }

        /// <summary>
        /// Calculates the position of the node from a given edge lenght and rotation
        /// </summary>
        /// <param name="l">Edge length</param>
        /// <param name="theta">Polar angle in degree</param>
        /// <param name="phi">Azimuth angle in degree</param>
        /// <returns>Vector of the node position</returns>
        public static Vector3 CalcNodePosition(float l, float theta, float phi)
        {
            theta = DegreeToRadian(theta);
            phi = DegreeToRadian(phi);

            var x = (float) (Math.Sin(phi) * Math.Sin(theta) * l);
            var y = (float) (Math.Cos(theta) * l);
            var z = (float) (Math.Cos(phi) * Math.Sin(theta) * l);

            return new Vector3(x, y, z);
        }

        public static float SizeToScale(float size, float defaultSize, float defaultScale)
        {
            return defaultScale * size / defaultSize;
        }

        public static float GetRandomAngle()
        {
            return Random.Next(0, 360);
        }

        private static float DegreeToRadian(float angle)
        {
            return (float) (angle * Math.PI / 180.0);
        }

        private static float RadianToDegree(double angle)
        {
            return (float) (angle * 180.0 / Math.PI);
        }
    }
}