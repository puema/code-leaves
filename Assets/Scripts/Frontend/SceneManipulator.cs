using HoloToolkit.Unity;
using HUX.Focus;
using UniRx;
using UnityEngine;
using Utilities;

namespace Frontend
{
    public class SceneManipulator : Singleton<SceneManipulator>
    {
        // ----==== Meshes as public variables ====---- //
        public GameObject Forest;
        
        public GameObject Floor;

        public GameObject Edge;

        public GameObject Leaf;

        public GameObject LeafLabel;

        public GameObject InnerNodeLabel;

        public GameObject CirclePlane;

        public Shader StandardShader;

        public Shader FocusedShader;

        public BoolReactiveProperty VisualizeCircles;
        // -------------------------------------------- //

        internal readonly Vector3 DefaultEdgeScale = new Vector3(1, 10, 1);
        internal readonly Vector3 Default3DTextScale = new Vector3(0.005f, 0.005f, 0.005f);

        internal const string TreeName = "Tree";
        internal const string BranchName = "Branch";
        internal const string NodeName = "Node";
        internal const string EdgeName = "Edge";
        internal const string LeafName = "Leaf";
        internal const string LabelName = "Label";
        internal const string CircleName = "Circle";

        internal float DefaultEdgeHeight;
        internal float DefaultCirclePlaneRadius;

        private const float DistanceNodeToLabel = 0.008f;
        private const float DistanceLeafToLabel = 0.008f;

        protected override void Awake()
        {
            Edge.transform.localScale = DefaultEdgeScale;
            DefaultEdgeHeight = Edge.GetSize(Axis.Y);
            DefaultCirclePlaneRadius = CirclePlane.transform.GetChild(0).gameObject.GetSize(Axis.X) / 2;
            base.Awake();
        }

        internal GameObject AddTreeObject(Vector2 position)
        {
            return InstantiateObject(TreeName, parent: Forest.transform,
                localPosition: new Vector3(position.x, 0f, position.y));
        }

        internal GameObject AddEmptyBranchObject(Transform parent)
        {
            return InstantiateObject(BranchName, parent: parent);
        }

        internal GameObject AddEdgeObject(Transform parent, float length, float theta,
            float phi, float? diameter = null)
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


        internal GameObject AddLeafObject(Transform parent, UiLeaf leaf)
        {
            var leafObject = InstantiateObject(LeafName, Leaf, parent);
            var height = leafObject.GetSize(Axis.Y);
            leafObject.AddComponent<Billboard>();
            leafObject.AddComponent<NodeInputHandler>();
            leafObject.AddComponent<ID>().Id = leaf.Id;

            if (leaf.Color.Value != null)
                leafObject.GetComponent<MeshRenderer>().material.color = leaf.Color.Value.Value;

            var label = AddLeafLabel(leafObject.transform, height);

            SubscribeReactNodeProperties(leaf, leafObject, label);

            return leafObject;
        }

        internal GameObject AddEmptyNodeObject(UiNode node, Transform parent, Vector3 position, GameObject edge)
        {
            var nodeObject = InstantiateObject(
                NodeName,
                parent: parent,
                localPosition: position
            );

            nodeObject.AddComponent<ID>().Id = node.Id;
            nodeObject.AddComponent<NodeInputHandler>();

            GameObject labelObject = null;
            if (node is UiInnerNode)
            {
                labelObject = AddNodeLabel(nodeObject.transform);
            }

            SubscribeReactNodeProperties(node, edge, labelObject);

            return nodeObject;
        }

        internal void AddCircleVisualization(Transform parent, float radius)
        {
            var scale = TreeGeometry.SizeToScale(radius, DefaultCirclePlaneRadius, 1);
            var cirvleObject = InstantiateObject(CircleName, CirclePlane, parent,
                localScale: new Vector3(scale, 1, scale));
            VisualizeCircles.Subscribe(cirvleObject.SetActive);
        }

        internal GameObject AddNodeLabel(Transform parent)
        {
            var labelWrapper = InstantiateObject(LabelName, InnerNodeLabel, parent, Vector3.down * DistanceNodeToLabel);
            labelWrapper.AddComponent<Billboard>();
            var labelObject = labelWrapper.transform.GetChild(0).gameObject;
            labelObject.GetComponent<TextMesh>().anchor = TextAnchor.UpperCenter;
            labelObject.SetActive(false);
            return labelObject;
        }

        internal GameObject AddLeafLabel(Transform parent, float y)
        {
            var label = InstantiateObject(LabelName, LeafLabel, parent,
                Vector3.up * (y + DistanceLeafToLabel),
                Default3DTextScale, isActive: false);
            label.GetComponent<TextMesh>().anchor = TextAnchor.LowerCenter;
            return label;
        }

        // ----==== Helper functions ====---- //

        internal void SubscribeReactNodeProperties(UiNode node, GameObject obj, GameObject label)
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

        /// <summary>
        /// Sets position of node according to the circle property and adjust edge properly
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="node"></param>
        /// <param name="edge"></param>
        internal void SubscribeNodePosition(Circle circle, GameObject node, GameObject edge)
        {
            circle.Position.Subscribe(v =>
            {
                var y = node.transform.localPosition.y;
                node.transform.localPosition = new Vector3(v.x, y, v.y);

                var phi = TreeGeometry.CalcAlpha(v.x, v.y);
                var theta = TreeGeometry.CalcTheta(DefaultEdgeHeight, v.magnitude);
                edge.transform.localEulerAngles = new Vector3(theta, phi, 0);

                var diameter = edge.transform.localScale.x;
                var l = TreeGeometry.CalcEdgeLength(DefaultEdgeHeight, theta);
                edge.transform.localScale = new Vector3(diameter,
                    TreeGeometry.SizeToScale(l, DefaultEdgeHeight, DefaultEdgeScale.y),
                    diameter);
            });
        }

        internal GameObject InstantiateObject(string objName = null, GameObject original = null,
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

        public void AdjustFloorRadius(UiNode forest)
        {
            var xScale = Floor.SizeToScale(Axis.X, forest.Circle.Radius * 2);
            var zScale = Floor.SizeToScale(Axis.Z, forest.Circle.Radius * 2);
            Floor.transform.localScale = new Vector3(xScale, 1, zScale);
        }
    }
}