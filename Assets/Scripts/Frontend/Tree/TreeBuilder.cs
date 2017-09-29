using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;
using Utilities;
using Frontend.Forest;
using Frontend.Models;
using UniRx.Triggers;

namespace Frontend.Tree
{
    public class TreeBuilder : Singleton<TreeBuilder>
    {
        /// <summary>
        /// Holds all the necessary game objects, offers their instantiation and manipulation
        /// </summary>
        public ForestManipulator manipulator;

        /// <summary>
        /// The part of the default edge length and thickness, that is added per node height
        /// </summary>
        private const float edgeYScaleFactor = 0.2f;
        private const float edgeXScaleFactor = 0.4f;

        /// <summary>
        /// Generates the unity tree according to the given data structure of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public GameObject GenerateTree(UiNode node, Vector2 position)
        {
            var treeHeight = node.GetHeight() + 1;
            var trunkHeight = manipulator.DefaultEdgeHeight * GetYScalingFactor(treeHeight);
            var trunkYScale = manipulator.DefaultEdgeScale.y * GetYScalingFactor(treeHeight);
            var trunkThickness = manipulator.DefaultEdgeScale.x * GetXScalingFactor(treeHeight);
            var nodePosition = new Vector3(0, trunkHeight, 0);

            var treeObject = manipulator.AddTreeObject(position);
            var trunkObject = manipulator.AddEmptyBranchObject(treeObject.transform);
            var edgeObject = manipulator.AddEdgeObject(trunkObject.transform, trunkYScale, 0, 0, trunkThickness);
            var nodeObject = manipulator.AddEmptyNodeObject(node, trunkObject.transform, nodePosition, edgeObject);

            GenerateBranches(node, nodeObject.transform);

            var disposable = node.Circle.Position.Subscribe(v =>
            {
                treeObject.transform.localPosition = new Vector3(v.x, 0, v.y);
            });
            treeObject.OnDestroyAsObservable().Subscribe(_ => disposable.Dispose());

            return treeObject;
        }

        /// <summary>
        /// Adds recursively the children of a node to the given parent transform 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void GenerateBranches(UiNode node, Transform parent)
        {
            var innerNode = node as UiInnerNode;
            if (innerNode != null)
            {
                if (HasOnlyLeaves(innerNode))
                {
                    DistributeSunflower(innerNode, parent);
                }
                else
                {
                    DistributeCirclePacking(innerNode, parent);
                }

                manipulator.AddCircleVisualization(
                    parent.Find(ForestManipulator.BranchName).Find(ForestManipulator.NodeName),
                    innerNode.Circle.Radius);

                return;
            }

            var leaf = node as UiLeaf;
            if (leaf != null)
            {
                manipulator.AddLeafObject(parent, leaf);
                return;
            }

            Debug.LogError("Unknown type of node, aborting structure generation");
        }

        private void DistributeSunflower(UiInnerNode innerNode, Transform parent)
        {
            var rndAngle = TreeGeometry.GetRandomAngle();

            for (var i = 0; i < innerNode.Children.Count; i++)
            {
                var child = innerNode.Children[i];
                var nodeObject = AddChildContainer(child, parent, i, rndAngle);

                GenerateBranches(child, nodeObject.transform);
            }

            innerNode.Circle.Radius = innerNode.Children.Count == 1
                ? TreeGeometry.NodeDistanceFactor
                : TreeGeometry.CalcSunflowerRadius(innerNode.Children.Count - 1);
        }

        private void DistributeCirclePacking(UiInnerNode innerNode, Transform parent)
        {
            GenerateUnsdistributedBranches(innerNode, parent);

            CirclePacking(innerNode);
        }

        /// <summary>
        /// From Wang's circle packing algorithm, see "Visualization of large hierarchical data by circle packing"
        /// </summary>
        /// <param name="innerNode"></param>
        internal LinkedList<Circle> CirclePacking(UiInnerNode innerNode)
        {
            var frontChain = InitFrontchain(innerNode);

            for (var i = 2; i < innerNode.Children.Count; i++)
            {
                // circle with minimal distance to origin
                var tangentCircle1 = GetClosestFromOrigin(frontChain);

                // circle after tangent circle 1
                var tangentCircle2 = tangentCircle1.Next();

                // circle of current inner node, claculate potential position
                var currentCircleRad = innerNode.Children[i].Circle.Radius;
                var currentCirclePos = TreeGeometry.CalcTangentCircleCenter(tangentCircle1.Value.Position.Value,
                    tangentCircle2.Value.Position.Value, tangentCircle1.Value.Radius, tangentCircle2.Value.Radius,
                    currentCircleRad);

                // circle that intersects current circle
                var intersectingCircle = GetIntersectingCircle(frontChain, currentCirclePos, currentCircleRad);

                // No intersection, place current circle
                if (intersectingCircle == null)
                {
                    innerNode.Children[i].Circle.Position.Value = currentCirclePos;
                    frontChain.AddBefore(tangentCircle2, innerNode.Children[i].Circle);
                    continue;
                }

                // Delete old circles from front chain
                if (intersectingCircle.IsAfter(tangentCircle2))
                {
                    tangentCircle1.DeleteAfterUntil(intersectingCircle);
                }
                else
                {
                    intersectingCircle.DeleteAfterUntil(tangentCircle2);
                }

                // Proceed with current circle again, position is calculated according to updated front chain
                i--;
            }

            innerNode.Circle.Radius = GetEnclosingRadius(frontChain);

            return frontChain;
        }

        private static float GetEnclosingRadius(IEnumerable<Circle> frontChain)
        {
            var maxDistanceCircle = frontChain.Aggregate(
                (current, next) =>
                    next.Position.Value.magnitude + next.Radius > current.Position.Value.magnitude + current.Radius
                        ? next
                        : current);

            return Vector2.Distance(maxDistanceCircle.Position.Value, Vector2.zero) +
                   maxDistanceCircle.Radius;
        }

        private static LinkedList<Circle> InitFrontchain(UiInnerNode node)
        {
            var frontChain = new LinkedList<Circle>();

            if (node.Children.Count == 0) return frontChain;

            var c1 = node.Children[0].Circle;
            frontChain.AddLast(c1);

            if (node.Children.Count == 1) return frontChain;

            var c2 = node.Children[1].Circle;
            c2.Position.Value = TreeGeometry.CalcTangentCircleCenter(c1.Position.Value, c1.Radius,
                c2.Radius,
                TreeGeometry.GetRandomAngle());
            frontChain.AddLast(c2);

            return frontChain;
        }

        private static LinkedListNode<Circle> GetClosestFromOrigin(LinkedList<Circle> frontChain)
        {
            return frontChain.Find(frontChain.Aggregate(
                (current, next) => next.Position.Value.magnitude < current.Position.Value.magnitude ? next : current));
        }

        private static LinkedListNode<Circle> GetIntersectingCircle(LinkedList<Circle> frontChain, Vector2 position,
            float radius)
        {
            return frontChain.Find(frontChain.FirstOrDefault(
                c => TreeGeometry.Intersects(c.Position.Value, position, c.Radius, radius)));
        }

        private void GenerateUnsdistributedBranches(UiInnerNode innerNode, Transform parent)
        {
            var nodeHeight = innerNode.GetHeight();
            var centralEdgeHeight = manipulator.DefaultEdgeHeight * GetYScalingFactor(nodeHeight);
            var edgeThickness = manipulator.DefaultEdgeScale.x * GetXScalingFactor(nodeHeight);

            foreach (var child in innerNode.Children)
            {
                var branchObject = manipulator.AddEmptyBranchObject(parent);
                // Edge length doesn't matter at this point. It is set properly at subscription of node position.
                var edgeObject = manipulator.AddEdgeObject(branchObject.transform, 1, 0, 0, edgeThickness);
                var nodeObject = manipulator.AddEmptyNodeObject(child, branchObject.transform,
                    Vector3.up * centralEdgeHeight, edgeObject);

                // Rotate and stretch edge according to corresponding node.
                manipulator.SubscribeNodePosition(child.Circle, nodeObject, edgeObject, centralEdgeHeight);

                GenerateBranches(child, nodeObject.transform);
            }
        }

        private GameObject AddChildContainer(UiNode node, Transform parent, int n, float initialPhi = 0)
        {
            //////////////////////////////////
            //                              //
            //         y                    //
            //         ∧                    //           
            //         | .  r               //
            //         |   ˙ .              //   
            //         |       ˙ . node     //
            //         |__˛   ‚´ .          //
            //         | θ \‚´   .          //
            //         |  ‚´ l   .          //
            //         |‚´       .          //
            //         ˚-. ------.--> x     //
            //        / φ  ˙ .   .          //
            //       /————´    ˙ .          //
            //      ⩗                       //
            //    - z                       //
            //                              //
            //////////////////////////////////

            var r = TreeGeometry.CalcSunflowerRadius(n);
            var phi = TreeGeometry.CalcPhi(n, initialPhi);
            var theta = TreeGeometry.CalcTheta(manipulator.DefaultEdgeHeight, r);
            var l = TreeGeometry.CalcEdgeLength(manipulator.DefaultEdgeHeight, theta);
            var nodePosition = TreeGeometry.CalcNodePosition(l, theta, phi);

            var branchObject = manipulator.AddEmptyBranchObject(parent);
            var edgeObject = manipulator.AddEdgeObject(branchObject.transform,
                TreeGeometry.SizeToScale(l, manipulator.DefaultEdgeHeight, manipulator.DefaultEdgeScale.y),
                theta,
                phi);
            var nodeObject =
                manipulator.AddEmptyNodeObject(node, branchObject.transform, nodePosition, edgeObject);

            return nodeObject;
        }

        private static bool HasOnlyLeaves(UiInnerNode node)
        {
            return node.Children.TrueForAll(n => n is UiLeaf);
        }

        private static float GetXScalingFactor(int nodeHeight)
        {
            return 1 + (nodeHeight - 1) * edgeXScaleFactor;
        }

        private static float GetYScalingFactor(int nodeHeight)
        {
            return 1 + (nodeHeight - 1) * edgeYScaleFactor;
        }
    }
}