using HoloToolkit.Unity;
using UniRx;
using UnityEngine;
using Random = System.Random;

namespace Frontend
{
    public class TreeBuilder : Singleton<TreeBuilder>
    {
        // ----==== Meshes as public variables ====---- //
        public GameObject Floor;

        public GameObject Edge;

        public GameObject Leaf;

        public GameObject LeafLabel;

        public GameObject InnerNodeLabel;

        public Shader StandardShader;

        public Shader FocusedShader;
        // -------------------------------------------- //

        // ----==== Flags ====---- //
        public bool UseAllwaysMainTrunk;

        public bool UseMainTrunkAt3Fork;

        public bool GrowInDirectionOfBranches = true;
        // ----------------------- //

        private const float DistanceLeafToLabel = 0.008f;
        private const float DistanceNodeToLabel = 0.008f;
        private static readonly Vector3 DefaultEdgeScale = new Vector3(1, 10, 1);
        private static readonly Vector3 Default3DTextScale = new Vector3(0.005f, 0.005f, 0.005f);

        internal const string TreeName = "Tree";
        internal const string BranchName = "Branch";
        internal const string NodeName = "Node";
        internal const string EdgeName = "Edge";
        internal const string LeafName = "Leaf";
        internal const string LabelName = "Label";

        private float DefaultEdgeHeight;

        protected override void Awake()
        {
            Edge.transform.localScale = DefaultEdgeScale;
            DefaultEdgeHeight = GetYSize(Edge);
            base.Awake();
        }

        /// <summary>
        /// Generates the unity tree according to the given data structure of the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="position"></param>
        public void GenerateTree(UiNode node, Vector2 position)
        {
            var tree = AddTreeObject(position);
            var trunkObject = AddEmptyBranchObject(tree.transform);
            var edgeObject = AddEdgeObject(trunkObject.transform, DefaultEdgeScale.y, 0, 0);
            var nodeObject = AddEmptyNodeObject(node, trunkObject.transform, edgeObject,
                new Vector3(0, DefaultEdgeHeight, 0));
            node.SortChildren();
            GenerateBranchs(node, nodeObject.transform);
        }

        /// <summary>
        /// Adds recursively the children of a node to the given parent transform 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentObject"></param>
        private void GenerateBranchs(UiNode node, Transform parentObject)
        {
            if (node is UiInnerNode)
            {
                var innerNode = (UiInnerNode) node;

                var rndAngle = TreeGeometry.GetRandomAngle();
                
                for (var i = 0; i < innerNode.Children.Count; i++)
                {
                    var nodeObject = AddChild(innerNode, parentObject, i, rndAngle);

                    GenerateBranchs(innerNode.Children[i], nodeObject.transform);
                }

                return;
            }

            if (node is UiLeaf)
            {
                var leaf = (UiLeaf) node;
                AddLeafObject(parentObject, leaf);
                return;
            }

            Debug.LogError("Unknown type of node, aborting structure generation");
        }

        private GameObject AddChild(UiInnerNode node, Transform parent, int n, float initialPhi = 0)
        {
            var r = TreeGeometry.CalcRadius(node, n);
            var phi = TreeGeometry.CalcPhi(n, initialPhi);
            var theta = TreeGeometry.CalcTheta(DefaultEdgeHeight, r);
            var l = TreeGeometry.CalcEdgeLength(DefaultEdgeHeight, theta);
            var nodePosition = TreeGeometry.CalcNodePosition(l, theta, phi);

            var branchObject = AddEmptyBranchObject(parent);
            var edgeObject = AddEdgeObject(branchObject.transform,
                TreeGeometry.SizeToScale(l, DefaultEdgeHeight, DefaultEdgeScale.y), theta, phi);
            var nodeObject = AddEmptyNodeObject(node.Children[n], branchObject.transform, edgeObject, nodePosition);

            return nodeObject;
        }

        private GameObject AddTreeObject(Vector2 position)
        {
            return InstantiateObject(TreeName, parent: Floor.transform,
                localPosition: new Vector3(position.x, 0f, position.y));
        }

        private GameObject AddEmptyBranchObject(Transform parent)
        {
            return InstantiateObject(BranchName, parent: parent);
        }

        private GameObject AddEdgeObject(Transform parent, float length, float theta, float phi, float? diameter = null)
        {
            diameter = diameter ?? DefaultEdgeScale.x;

            var edgeObject = InstantiateObject(
                EdgeName, Edge, parent.transform,
                localEulerAngles: new Vector3(theta, phi, 0),
                localScale: new Vector3(diameter.Value, length, diameter.Value)
            );
            edgeObject.AddComponent<NodeInputHandler>();

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

        private GameObject AddEmptyNodeObject(UiNode node, Transform branch, GameObject edge, Vector3 position)
        {
            var nodeObject = InstantiateObject(
                NodeName,
                parent: branch,
                localPosition: position
            );

            nodeObject.AddComponent<ID>().Id = node.Id;
            nodeObject.AddComponent<NodeInputHandler>();

            GameObject label = null;
            if (node is UiInnerNode)
            {
                label = AddNodeLabel(nodeObject.transform);
            }

            SubscribeReactNodeProperties(node, edge, label);

            return nodeObject;
        }

        private GameObject AddNodeLabel(Transform parent)
        {
            var labelWrapper = InstantiateObject(LabelName, InnerNodeLabel, parent, Vector3.down * DistanceNodeToLabel);
            labelWrapper.AddComponent<Billboard>();
            var labelObject = labelWrapper.transform.GetChild(0).gameObject;
            labelObject.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
            labelObject.SetActive(false);
            return labelObject;
        }

        private GameObject AddLeafLabel(Transform parent, float y)
        {
            var label = InstantiateObject(LabelName, LeafLabel, parent,
                Vector3.up * (y + DistanceLeafToLabel),
                Default3DTextScale, isActive: false);
            label.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
            return label;
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
//            label.GetComponent<TextMesh>().text = node.GetWidth().ToString();
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

        public static float KeepAngleIn360(float angle)
        {
            return (angle %= 360) < 0 ? angle + 360 : angle;
        }
    }
}