using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UniRx;
using UnityEngine;

namespace Frontend
{
    public class TreeBuilderBack : Singleton<TreeBuilder>
    {
        // ----==== Meshes as public variables ====---- //
        public GameObject Floor;

        public GameObject Edge;

        public GameObject Leaf;

        public GameObject Label;

        public Shader StandardShader;

        public Shader FocusedShader;
        // -------------------------------------------- //

        // ----==== Flags ====---- //
        public bool UseAllwaysMainTrunk;

        public bool UseMainTrunkAt3Fork;

        public bool GrowInDirectionOfBranches = true;
        // ----------------------- //

        private const float DefaultScale = 1;
        private const float DistanceLeafToLabel = 0.008f;
        private const float DistanceNodeToLabel = 0.008f;
        private static readonly Vector3 BaseAspectRatio = new Vector3(1, 10, 1);
        private static readonly Vector3 Default3DTextScale = new Vector3(0.005f, 0.005f, 0.005f);

        internal static readonly string TreeName = "Tree";
        internal static readonly string BranchName = "Branch";
        internal static readonly string NodeName = "Node";
        internal static readonly string EdgeName = "Edge";
        internal static readonly string LeafName = "Leaf";
        internal static readonly string LabelName = "Label";

        private static readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;
        private static readonly double GoldenAngle = RadianToDegree(2 * Math.PI - 2 * Math.PI / GoldenRatio);
        private static readonly double CircleThroughGoldenAngle = 360 / GoldenAngle;
        private static float _currentBranchRotation = 0;

        /// <summary>
        /// Generates the unity tree according to the given data structure of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public void GenerateTreeStructure(UiNode node, Vector2 position)
        {
            var tree = AddTreeObject(position);
            var trunk = AddBranchObject(node, tree.transform);
            node.SortChildren();
            AddChildrenOfNode(node, trunk.transform.Find(NodeName));
        }

        /// <summary>
        /// Adds the children of a node to the given parent transform 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentObject"></param>
        /// <param name="scale"></param>
        private void AddChildrenOfNode(UiNode node, Transform parentObject, float scale = DefaultScale)
        {
            if (node is UiInnerNode)
            {
                var innerNode = (UiInnerNode) node;
                if (innerNode.Children == null) return;
                var branchObjects = AddBranchObjects(innerNode, parentObject, scale);
                scale *= 0.8f;
                for (var i = 0; i < innerNode.Children.Count; i++)
                {
                    AddChildrenOfNode(innerNode.Children[i], branchObjects[i].transform.Find(NodeName), scale);
                }
            }
            else if (node is UiLeaf)
            {
                var leaf = (UiLeaf) node;
                AddLeafObject(parentObject, leaf);
            }
            else
            {
                Debug.LogError("Unknown type of node, aborting structure generation");
            }
        }

        /// <summary>
        /// Adds the root game object for the tree
        /// </summary>
        /// <returns></returns>
        private GameObject AddTreeObject(Vector2 position)
        {
            return InstantiateObject(TreeName, parent: Floor.transform,
                localPosition: new Vector3(position.x, 0f, position.y));
        }

        private List<GameObject> AddBranchObjects(UiInnerNode node, Transform parent, float scale)
        {
            var branches = new List<GameObject>();

            var count = node.Children.Count;
            for (var i = 0; i < count; i++)
            {
                // Add a new branch and subsequent edge and node
                branches.Add(AddBranchObject(node.Children[i], parent, count, i, scale));
            }

            return branches;
        }

        private GameObject AddBranchObject(UiNode node, Transform parent, int siblingCount = 1, int siblingIndex = 0,
            float scale = DefaultScale)
        {
            // Add Branch as new origin
            var branchObject = AddEmptyBranchObject(parent, siblingCount, siblingIndex);
            // Store the height of the unrotated edge
            float edgeLength;
            float edgeThickness;
            var edge = AddEdgeObject(branchObject.transform, siblingCount, siblingIndex, scale, out edgeLength,
                out edgeThickness);

            // Add node at the end 
            AddEmptyNodeObject(node, branchObject.transform, edge.transform, edgeLength, edgeThickness);

            return branchObject;
        }

        private GameObject AddEmptyBranchObject(Transform parent, int siblingsCount, int siblingIndex)
        {
            var branchObject = InstantiateObject(BranchName, parent: parent);

            if (!GrowInDirectionOfBranches) return branchObject;

            RotateBranchOrEdge(branchObject.transform, siblingsCount, siblingIndex);
            branchObject.transform.Rotate(0, (float) GoldenAngle, 0, Space.Self);

            return branchObject;
        }

        /// <summary>
        /// Rotates the a whole branch or edge according to the siblings count and the own siblings index
        /// </summary>
        /// <param name="treeObject"></param>
        /// <param name="siblingsCount"></param>
        /// <param name="siblingIndex"></param>
        private void RotateBranchOrEdge(Transform treeObject, int siblingsCount, int siblingIndex)
        {
            float xAngle;
            float yAngle;

            var mainTrunk = UseAllwaysMainTrunk ||
                            siblingsCount % 2 != 0 && !(siblingsCount == 3 && !UseMainTrunkAt3Fork);

            if (!mainTrunk) siblingIndex++;
            if (mainTrunk) siblingsCount--;

            if (siblingIndex != 0)
            {
                xAngle = 30;
                yAngle = 360 / siblingsCount * --siblingIndex;
            }
            else
            {
                xAngle = 0;
                yAngle = 0;
            }

            treeObject.transform.localEulerAngles = new Vector3(xAngle, yAngle, 0);

//        float xzLength;
//        if (!mainTrunk)
//        {
//            siblingIndex++;
//        }
//        
//
//        if (siblingIndex == 0)
//        {
//            xzLength = 0;
//        }
//        else
//        {
//            // Do not touch, ask not why, never, ever.
//            xzLength = edgeLength / ((siblingsCount - 2) / 3 + 2) *
//                       ((siblingIndex - 1) / 3 + 1);
//        }
//     
//        var xAngle = (float) RadianToDegree(Math.Asin(xzLength / edgeLength));
//
//        if (!angle.Equals(0) && !siblingIndex.Equals(0))
//        {
//            _currentBranchRotation += (float) GoldenAngle;
//        }
//        yAngle = _currentBranchRotation;
        }

        private GameObject AddEdgeObject(Transform branch, int siblingsCount, int siblingIndex, float scale,
            out float edgeLength, out float edgeThickness)
        {
            var edgeObject = InstantiateObject(EdgeName, Edge, branch.transform, localScale: scale * BaseAspectRatio);
            edgeLength = GetYSize(edgeObject);
            edgeThickness = GetZSize(edgeObject);
            edgeObject.AddComponent<NodeInputHandler>();

            if (!GrowInDirectionOfBranches) RotateBranchOrEdge(edgeObject.transform, siblingsCount, siblingIndex);
            else edgeObject.transform.localEulerAngles = Vector3.zero;

            return edgeObject;
        }


        private GameObject AddLeafObject(Transform parent, UiLeaf leaf)
        {
            var leafObject = InstantiateObject(LeafName, Leaf, parent);
            var height = GetYSize(leafObject);
            leafObject.AddComponent<Billboard>();
            leafObject.AddComponent<NodeInputHandler>();
            leafObject.AddComponent<ID>().Id = leaf.Id;

            if (leaf.Color.Value != null)
                leafObject.GetComponent<MeshRenderer>().material.color = leaf.Color.Value.Value;

            var label = AddLeafLabel(leafObject.transform, height);

            SubscribeReactNodeProperties(leaf, leafObject, label);

            return leafObject;
        }

        private void AddEmptyNodeObject(UiNode node, Transform branch, Transform edge, float edgeLength,
            float edgeThickness)
        {
            var nodeObject = InstantiateObject(NodeName, parent: branch,
                localPosition: CalculatePositionOfNode(edge, edgeLength));

            nodeObject.transform.Rotate(0, (float) GoldenAngle, 0);
            nodeObject.AddComponent<ID>().Id = node.Id;
            nodeObject.AddComponent<NodeInputHandler>();

            GameObject label = null;
            if (node is UiInnerNode)
            {
                label = AddNodeLabel(nodeObject.transform, edgeThickness / 2);
            }

            SubscribeReactNodeProperties(node, edge.gameObject, label);
        }

        private GameObject AddNodeLabel(Transform parent, float z)
        {
            var label = InstantiateObject(LabelName, Label, parent, isActive: false);
            label.transform.Translate(-z, -DistanceNodeToLabel, 0, Space.Self);
            label.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
            label.AddComponent<Billboard>();
            return label;
        }

        private GameObject AddLeafLabel(Transform parent, float y)
        {
            var label = InstantiateObject(LabelName, Label, parent,
                Vector3.up * (y + DistanceLeafToLabel),
                Default3DTextScale, isActive: false);
            label.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
            return label;
        }

        private static Vector3 CalculatePositionOfNode(Transform edge, float edgeLength)
        {
            var theta = DegreeToRadian(edge.localEulerAngles.x);
            var phi = DegreeToRadian(edge.localEulerAngles.y);

            var y = (float) Math.Cos(theta) * edgeLength;
            var xz = (float) Math.Sin(theta) * edgeLength;
            var x = (float) Math.Sin(phi) * xz;
            var z = (float) Math.Cos(phi) * xz;

            return new Vector3(x, y, z);
        }

        // ----==== Helper functions ====---- //

        private void SubscribeReactNodeProperties(UiNode node, GameObject obj, GameObject label)
        {
            node.IsSelected = node.IsSelected ?? new ReactiveProperty<bool>(false);
            node.IsFocused = node.IsFocused ?? new ReactiveProperty<bool>(false);
            node.Text = node.Text ?? new ReactiveProperty<string>("No text available.");

            node.IsFocused.Subscribe(isFocused =>
                obj.GetComponent<MeshRenderer>().material.shader = isFocused
                    ? FocusedShader
                    : StandardShader);
            if (label == null) return;

            node.IsSelected.Subscribe(label.SetActive);
            node.Text.Subscribe(text => label.GetComponent<TextMesh>().text = text);
        }

        private static GameObject InstantiateObject(string objName = null, GameObject original = null,
            Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null,
            Vector3? localEulerAngles = null, bool isActive = true)
        {
            localPosition = localPosition ?? Vector3.zero;
            localEulerAngles = localEulerAngles ?? Vector3.zero;

            var gameObj = original == null ? new GameObject(objName) : Instantiate(original);
            gameObj.SetActive(isActive);
            if (parent != null) gameObj.transform.SetParent(parent);
            if (objName != null) gameObj.name = objName;
            if (localScale != null) gameObj.transform.localScale = localScale.Value;
            gameObj.transform.localPosition = localPosition.Value;
            gameObj.transform.localEulerAngles = localEulerAngles.Value;

            return gameObj;
        }

        private static float GetYSize(GameObject obj)
        {
            return obj.GetComponent<MeshRenderer>().bounds.size.y;
        }

        private static float GetZSize(GameObject obj)
        {
            return obj.GetComponent<MeshRenderer>().bounds.size.z;
        }

        private static double DegreeToRadian(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        private static double RadianToDegree(double angle)
        {
            return angle * 180.0 / Math.PI;
        }

        public static float KeepAngleIn360(float angle)
        {
            return (angle %= 360) < 0 ? angle + 360 : angle;
        }
    }
}