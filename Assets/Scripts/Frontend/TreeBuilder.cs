using HoloToolkit.Unity;
using UnityEngine;
using Utilities;

namespace Frontend
{
    public class TreeBuilder : Singleton<TreeBuilder>
    {
        private ObjectInstantiator Instantiator;

        protected override void Awake()
        {
            Instantiator = ObjectInstantiator.Instance;
            base.Awake();
        }

        /// <summary>
        /// Generates the unity tree according to the given data structure of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public void GenerateTree(UiNode node, Vector2 position)
        {
            var tree = Instantiator.AddTreeObject(position);
            var trunkObject = Instantiator.AddEmptyBranchObject(tree.transform);
            var edgeObject =
                Instantiator.AddEdgeObject(trunkObject.transform, Instantiator.DefaultEdgeScale.y, 0, 0);
            var nodeObject = Instantiator.AddEmptyNodeObject(node, trunkObject.transform,
                new Vector3(0, Instantiator.DefaultEdgeHeight, 0), edgeObject);
            node.SortChildren();
            GenerateBranchs(node, nodeObject.transform);
        }

        /// <summary>
        /// Adds recursively the children of a node to the given parent transform 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent"></param>
        private void GenerateBranchs(UiNode node, Transform parent)
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
                var nodeObject = AddChild(innerNode, parent, i, rndAngle);

                GenerateBranchs(innerNode.Children[i], nodeObject.transform);
            }

            Instantiator.AddCircleVisualization(
                parent.Find(ObjectInstantiator.BranchName).Find(ObjectInstantiator.NodeName),
                TreeGeometry.CalcRadius(innerNode, innerNode.Children.Count - 1));
        }

        private void DistributeCirclePacking(UiInnerNode innerNode, Transform parent)
        {
            DistributeSunflower(innerNode, parent);
        }

        private GameObject AddChild(UiInnerNode node, Transform parent, int n, float initialPhi = 0)
        {
            var r = TreeGeometry.CalcRadius(node, n);
            var phi = TreeGeometry.CalcPhi(n, initialPhi);
            var theta = TreeGeometry.CalcTheta(Instantiator.DefaultEdgeHeight, r);
            var l = TreeGeometry.CalcEdgeLength(Instantiator.DefaultEdgeHeight, theta);
            var nodePosition = TreeGeometry.CalcNodePosition(l, theta, phi);

            var branchObject = Instantiator.AddEmptyBranchObject(parent);
            var edgeObject = Instantiator.AddEdgeObject(branchObject.transform,
                TreeGeometry.SizeToScale(l, Instantiator.DefaultEdgeHeight, Instantiator.DefaultEdgeScale.y), theta,
                phi);
            var nodeObject =
                Instantiator.AddEmptyNodeObject(node.Children[n], branchObject.transform, nodePosition, edgeObject);

            return nodeObject;
        }

        private static bool HasOnlyLeaves(UiInnerNode node)
        {
            return node.Children.TrueForAll(n => n is UiLeaf);
        }
    }
}