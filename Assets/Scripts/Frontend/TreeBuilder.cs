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

        protected override void Awake()
        {
            base.Awake();
        }

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

            Instantiator.AddCircleVisualization(
                parent.Find(Instantiator.BranchName).Find(Instantiator.NodeName),
                innerNode.Circle.Radius);
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

            var C_1 = innerNode.Children[0].Circle;
            var C_2 = innerNode.Children[1].Circle;

            C_2.Position.Value = TreeGeometry.CalcTangentCircleCenter(C_1.Position.Value, C_1.Radius, C_2.Radius,
                TreeGeometry.GetRandomAngle());

            var frontChain = new LinkedList<Circle>(new[] {C_1, C_2});

            for (var i = 2; i < innerNode.Children.Count; i++)
            {
                var C_m = frontChain.Aggregate(
                    (current, next) =>
                        next.Position.Value.magnitude < current.Position.Value.magnitude ? next : current);
                // m = n + 1
                var C_n_Node = frontChain.Find(C_m).Next();
                var C_n = C_n_Node.Value;
//                var C_n = frontChain.Last.Value;

                var C_i_radius = innerNode.Children[i].Circle.Radius;
                var C_i_position = TreeGeometry.CalcTangentCircleCenter(C_m.Position.Value, C_n.Position.Value,
                    C_m.Radius,
                    C_n.Radius, C_i_radius);
                
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
                
                if (C_j == null)
                {
                    innerNode.Children[i].Circle.Position.Value = C_i_position;
                    frontChain.AddBefore(C_n_Node, innerNode.Children[i].Circle);
                    continue;
                }
                
//                if (frontChain.GetIndex(C_j) > frontChain.GetIndex(C_n))
//                {
//                    var next = frontChain.Find(C_j).Next();
//                    while (next.Value != C_n)
//                    {
//                        next = next.Next;
//                        frontChain.Remove(next.Previous);
//                    }
//                }
//                else
//                {
//                    var previous = frontChain.Find(C_j).Previous();
//                    while (previous.Value != C_n)
//                    {
//                        previous = previous.Previous;
//                        frontChain.Remove(previous.Next);
//                    }
//                }

            }

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