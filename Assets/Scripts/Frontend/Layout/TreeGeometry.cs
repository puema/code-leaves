using System;
using UnityEngine;
using Random = System.Random;

namespace Frontend
{
    public static class TreeGeometry
    {
        public const float NodeDistanceFactor = 0.05f;
        public const float IntersectTolerance = 1.0E-4f;
        public static readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;
        public static readonly double GoldenAngle = RadianToDegree(2 * Math.PI / Math.Pow(GoldenRatio, 2));

        private static readonly Random Random = new Random();

        /// <summary>
        /// Calculates the distance between the central node and the nth sibling
        /// </summary>
        /// <param name="n"></param>
        /// <returns>Radius</returns>
        public static float CalcSunflowerRadius(int n)
        {
            return (float) Math.Sqrt(n) * NodeDistanceFactor;
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
            return CalcAlpha(r, h);
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

        public static Vector2 CalcTangentCircleCenter(Vector2 c1, Vector2 c2, float r1, float r2, float r3)
        {
            return CalcCircleIntersectionPoints(c1, c2, r1 + r3, r2 + r3)[0];
        }

        public static Vector2 CalcTangentCircleCenter(Vector2 c1, float r1, float r2, float alpha)
        {
            var d = r1 + r2;

            var x = (float) Math.Cos(DegreeToRadian(alpha)) * d;
            var y = (float) Math.Sin(DegreeToRadian(alpha)) * d;

            return c1 + new Vector2(x, y);
        }

        /// <summary>
        /// Calculates the intersection of two circles
        /// See https://de.wikipedia.org/wiki/Schnittpunkt#Schnittpunkte_zweier_Kreise
        /// </summary>
        /// <param name="c1">Mid</param>
        /// <param name="c2"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        private static Vector2[] CalcCircleIntersectionPoints(Vector2 c1, Vector2 c2, float r1, float r2)
        {
            var d = c2 - c1;
            var d1 = (float) (0.5f * ((Math.Pow(r1, 2) - Math.Pow(r2, 2)) / Math.Pow(d.magnitude, 2) + 1)) * d;
            var h = (float) Math.Sqrt(Math.Pow(r1, 2) - Math.Pow(d1.magnitude, 2));
            h = float.IsNaN(h) ? 0 : h;
            var e1 = d.normalized;
            var e2 = new Vector2(-e1.y, e1.x);
            var c3_1 = c1 + d1 + h * e2;
            var c3_2 = c1 + d1 - h * e2;
            return new[] {c3_1, c3_2};
        }

        /// <summary>
        /// Calculates the alpha angle of a arbitrary trianle with 3 given sides
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float CalcAlpha(float a, float b, float c)
        {
            // Law of cosins
            return RadianToDegree(Math.Acos((Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2)) / (2 * b * c)));
        }

        /// <summary>
        /// Calculates the alpha angle of a triangle with right angle between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float CalcAlpha(float a, float b)
        {
            var alpha = RadianToDegree(Math.Atan(a / b));
            if (b < 0) alpha += 180;
            return float.IsNaN(alpha) ? 0 : alpha;
        }

        public static bool Intersects(Vector2 c1, Vector2 c2, float r1, float r2)
        {
            return r1 + r2 - Vector2.Distance(c1, c2) > IntersectTolerance;
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