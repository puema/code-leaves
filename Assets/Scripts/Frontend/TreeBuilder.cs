using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;
using Utilities;

namespace Frontend
{
    public class TreeBuilder : Singleton<TreeBuilder>
    {
        private Instantiator Instantiator;

        /// <summary>
        /// Generates the unity tree according to the given data structure of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public void GenerateTree(UiNode node, Vector2 position)
        {
            Instantiator = Instantiator.Instance;
            var tree = Instantiator.AddTreeObject(position);
            var trunkObject = Instantiator.AddEmptyBranchObject(tree.transform);
            var edgeObject =
                Instantiator.AddEdgeObject(trunkObject.transform, Instantiator.DefaultEdgeScale.y, 0, 0);
            var nodeObject = Instantiator.AddEmptyNodeObject(node, trunkObject.transform,
                new Vector3(0, Instantiator.DefaultEdgeHeight, 0), edgeObject);
            node.SortChildren();
            GenerateBranches(node, nodeObject.transform);
        }

        /// <summary>
        /// Adds recursively the children of a node to the given parent transform 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void GenerateBranches(UiNode node, Transform parent)
        {
            if (node is UiInnerNode)
            {
                var innerNode = (UiInnerNode) node;

                if (HasOnlyLeaves(innerNode))
                {
                    DistributeSunflower(innerNode, parent);
                }
                else
                {
                    DistributeCirclePacking(innerNode, parent);
                }

                Instantiator.AddCircleVisualization(
                    parent.Find(Instantiator.BranchName).Find(Instantiator.NodeName),
                    innerNode.Circle.Radius);

                return;
            }

            if (node is UiLeaf)
            {
                var leaf = (UiLeaf) node;
                Instantiator.AddLeafObject(parent, leaf);
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

            innerNode.Circle.Radius = TreeGeometry.CalcRadius(innerNode.Children.Count - 1);
        }

        /// <summary>
        /// From Wang's circle packing algorithm, see "Visualization of large hierarchical data by circle packing"
        /// </summary>
        /// <param name="innerNode"></param>
        /// <param name="parent"></param>
        private void DistributeCirclePacking(UiInnerNode innerNode, Transform parent)
        {
            GenerateUnsdistributedBranches(innerNode, parent);

            if (innerNode.Children.Count <= 1) return;

            var frontChain = new LinkedList<Circle>();
            
            AddToFrontChain(frontChain, innerNode.Children[0].Circle, innerNode.Children[1].Circle);

            for (var i = 2; i < innerNode.Children.Count; i++)
            {
                // C_m is circle with minimal distance to origin
                var C_m = GetC_m(frontChain);

                // C_n is circle after C_n, n = m + 1
                var C_n = C_m.Next();

                // C_i is circle of current inner node, claculate potential position
                var C_i_radius = innerNode.Children[i].Circle.Radius;
                var C_i_position = TreeGeometry.CalcTangentCircleCenter(C_m.Value.Position.Value, 
                    C_n.Value.Position.Value, C_m.Value.Radius, C_n.Value.Radius, C_i_radius);

                // C_j is circle that intersects C_i
                var C_j = GetC_j(frontChain, C_i_position, C_i_radius);

                // No intersection, place C_i
                if (C_j == null)
                {
                    innerNode.Children[i].Circle.Position.Value = C_i_position;
                    frontChain.AddBefore(C_n, innerNode.Children[i].Circle);
                    continue;
                }

                // Delete old circles from front chain
                if (C_j.IsAfter(C_n))
                {
                    C_m.DeleteAfterUntil(C_j);
                }
                else
                {
                    C_j.DeleteAfterUntil(C_n);
                }

                // Proceed with C_i circle again, position is calculated according to updated front chain
                i--;
            }

            var C_max = frontChain.Aggregate(
                (current, next) =>
                    next.Position.Value.magnitude + next.Radius > current.Position.Value.magnitude + current.Radius
                        ? next
                        : current);

            innerNode.Circle.Radius = Vector2.Distance(C_max.Position.Value, Vector2.zero) + C_max.Radius;
        }

        private static void AddToFrontChain(LinkedList<Circle> frontChain, Circle c1, Circle c2)
        {
            c2.Position.Value = TreeGeometry.CalcTangentCircleCenter(c1.Position.Value, c1.Radius, c2.Radius,
                TreeGeometry.GetRandomAngle());
            frontChain.AddLast(c1);
            frontChain.AddLast(c2);
        }

        private static LinkedListNode<Circle> GetC_m(LinkedList<Circle> frontChain)
        {
            var C_m = frontChain.Aggregate(
                (current, next) =>
                    next.Position.Value.magnitude < current.Position.Value.magnitude ? next : current);
            return frontChain.Find(C_m);
        }

        private static LinkedListNode<Circle> GetC_j(LinkedList<Circle> frontChain, Vector2 C_i_position, float C_i_radius)
        {
            Circle C_j;
            try
            {
                C_j = frontChain.First(c =>
                    TreeGeometry.Intersects(c.Position.Value, C_i_position, c.Radius, C_i_radius));
            }
            catch
            {
                C_j = null;
            }
            return frontChain.Find(C_j);
        }

        private void GenerateUnsdistributedBranches(UiInnerNode innerNode, Transform parent)
        {
            foreach (var child in innerNode.Children)
            {
                var branchObject = Instantiator.AddEmptyBranchObject(parent);
                var edgeObject =
                    Instantiator.AddEdgeObject(branchObject.transform, Instantiator.DefaultEdgeScale.y, 0, 0);
                var nodeObject =
                    Instantiator.AddEmptyNodeObject(innerNode, branchObject.transform,
                        Vector3.up * Instantiator.DefaultEdgeHeight, edgeObject);

                SubscribeNodePosition(child.Circle, nodeObject, edgeObject);

                GenerateBranches(child, nodeObject.transform);
            }
        }

        private GameObject AddChildContainer(UiNode node, Transform parent, int n, float initialPhi = 0)
        {
            var r = TreeGeometry.CalcRadius(n);
            var phi = TreeGeometry.CalcPhi(n, initialPhi);
            var theta = TreeGeometry.CalcTheta(Instantiator.DefaultEdgeHeight, r);
            var l = TreeGeometry.CalcEdgeLength(Instantiator.DefaultEdgeHeight, theta);
            var nodePosition = TreeGeometry.CalcNodePosition(l, theta, phi);

            var branchObject = Instantiator.AddEmptyBranchObject(parent);
            var edgeObject = Instantiator.AddEdgeObject(branchObject.transform,
                TreeGeometry.SizeToScale(l, Instantiator.DefaultEdgeHeight, Instantiator.DefaultEdgeScale.y), theta,
                phi);
            var nodeObject =
                Instantiator.AddEmptyNodeObject(node, branchObject.transform, nodePosition, edgeObject);

            return nodeObject;
        }

        /// <summary>
        /// Sets position of node according to the circle property and adjust edge properly
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="node"></param>
        /// <param name="edge"></param>
        private void SubscribeNodePosition(Circle circle, GameObject node, GameObject edge)
        {
            circle.Position.Subscribe(v =>
            {
                var y = node.transform.localPosition.y;
                node.transform.localPosition = new Vector3(v.x, y, v.y);

                var phi = TreeGeometry.CalcAlpha(v.x, v.y);
                var theta = TreeGeometry.CalcTheta(Instantiator.DefaultEdgeHeight, v.magnitude);
                edge.transform.localEulerAngles = new Vector3(theta, phi, 0);

                var diameter = edge.transform.localScale.x;
                var l = TreeGeometry.CalcEdgeLength(Instantiator.DefaultEdgeHeight, theta);
                edge.transform.localScale = new Vector3(diameter,
                    TreeGeometry.SizeToScale(l, Instantiator.DefaultEdgeHeight, Instantiator.DefaultEdgeScale.y),
                    diameter);
            });
        }

        private static bool HasOnlyLeaves(UiInnerNode node)
        {
            return node.Children.TrueForAll(n => n is UiLeaf);
        }
    }
}