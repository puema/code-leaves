using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using AbstractNode;
using NUnit.Framework;

public class TreeBuilder : MonoBehaviour
{
    // ----==== Meshes as public variables ====---- //
    public GameObject Edge;

    public GameObject Leaf;

    public GameObject Label;
    // -------------------------------------------- //

    // ----==== Flags ====---- //
    public bool UseAllwaysMainTrunk;

    public bool UseMainTrunkAt3Fork;

    public bool GrowInDirectionOfBranches = true;

    public bool InitWithIds;
    // ----------------------- //

    private const float XScale = 1;
    private const float YScale = 10;
    private const float ZScale = 1;
    private static readonly Vector3 BaseAspectRatio = new Vector3(XScale, YScale, ZScale);
    private const float DefaultScale = 1;

    private const string TreeName = "Tree";
    private const string BranchName = "Branch";
    private const string NodeName = "Node";
    private const string EdgeName = "Edge";
    private const string LeafName = "Leaf";
    private const string LabelName = "Label";
    private const string TreeDataFilePath = "Assets/StreamingAssets/TreeStructureTypes.json";

    private static readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;
    private static readonly double GoldenAngle = RadianToDegree(2 * Math.PI - 2 * Math.PI / GoldenRatio);
    private static readonly double CircleThroughGoldenAngle = 360 / GoldenAngle;
    private static float _currentBranchRotation = 0;

    private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    // Use this for initialization
    void Start()
    {
//        SerializeData(this.data);
        var data = DesirializeData();

        var root = AddRootObject();

        GenerateTreeStructure(data, root.transform);
    }

    /// <summary>
    /// Generates the unity tree according to the given data structure of the node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parent"></param>
    private void GenerateTreeStructure(Node node, Transform parent)
    {
        var trunk = AddBranchObject(parent);
        node.AddHeight();
        node.AddChildCount();
        node.SortChildren();
        AddChildrenOfNode(node, trunk.transform.Find(NodeName));
    }

    /// <summary>
    /// Adds the children of a node to the given parent transform 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parentObject"></param>
    /// <param name="scale"></param>
    private void AddChildrenOfNode(Node node, Transform parentObject, float scale = DefaultScale)
    {
        if (node.GetType() == typeof(InnerNode))
        {
            var innerNode = (InnerNode) node;
            if (innerNode.Children == null) return;
            innerNode.SortChildren();
            var branchObjects = AddBranchObjects(parentObject, innerNode.Children.Count, scale);
            scale *= 0.8f;
            for (var i = 0; i < innerNode.Children.Count; i++)
            {
                AddChildrenOfNode(innerNode.Children[i], branchObjects[i].transform.Find(NodeName), scale);
            }
        }
        else if (node.GetType() == typeof(Leaf))
        {
            var leaf = (Leaf) node;
            AddLeafObject(parentObject, HexToNullableColor(leaf.Data.Color), leaf.Height);
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
    private static GameObject AddRootObject()
    {
        var root = new GameObject(TreeName);
        root.transform.position = Vector3.zero + Vector3.forward * 2;
        return root;
    }

    private List<GameObject> AddBranchObjects(Transform parent, int count, float scale)
    {
        var branches = new List<GameObject>();

        for (var i = 0; i < count; i++)
        {
            // Add a new branch and subsequent edge and node
            branches.Add(AddBranchObject(parent, count, i, scale));
        }

        return branches;
    }

    private GameObject AddBranchObject(Transform parent, int siblingCount = 1, int siblingIndex = 0,
        float scale = DefaultScale)
    {
        // Add Branch as new origin
        var branchObject = AddEmptyBranchObject(parent, siblingCount, siblingIndex);
        // Store the height of the unrotated edge
        float edgeLength;
        var edge = AddEdgeObject(branchObject.transform, siblingCount, siblingIndex, scale, out edgeLength);

        // Add node at the end 
        AddEmptyNodeObject(branchObject.transform, edge.transform, edgeLength);

        return branchObject;
    }

    private GameObject AddEmptyBranchObject(Transform parent, int siblingsCount, int siblingIndex)
    {
        var branch = new GameObject(BranchName);
        branch.transform.parent = parent;
        branch.transform.localPosition = Vector3.zero;
        branch.transform.localEulerAngles = Vector3.zero;

        if (!GrowInDirectionOfBranches) return branch;
        
        RotateBranchOrEdge(branch.transform, siblingsCount, siblingIndex);
        branch.transform.Rotate(0, (float) GoldenAngle, 0, Space.Self);
        
        return branch;
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

        var mainTrunk = UseAllwaysMainTrunk || siblingsCount % 2 != 0 && !(siblingsCount == 3 && !UseMainTrunkAt3Fork);

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
        out float edgeLength)
    {
        var edge = Instantiate(Edge);
        edge.name = EdgeName;
        edge.transform.localScale = scale * BaseAspectRatio;
        // Store unrotated y size to the edge length, needed for the later calculation of the node coordinates
        edgeLength = GetYSize(edge);
        edge.transform.parent = branch.transform;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.localEulerAngles = Vector3.zero;

        if (!GrowInDirectionOfBranches) RotateBranchOrEdge(edge.transform, siblingsCount, siblingIndex);
        return edge;
    }


    private GameObject AddLeafObject(Transform parent, Color? color = null, int? id = null)
    {
        var leaf = Instantiate(Leaf);
        leaf.AddComponent<Billboard>();
        leaf.name = LeafName;
        leaf.transform.parent = parent;
        leaf.transform.localPosition = Vector3.zero;

        if (color != null)
        {
            leaf.GetComponent<MeshRenderer>().material.color = color.Value;
        }

        if (id != null && InitWithIds)
        {
            var label = Instantiate(Label);
            label.name = LabelName;
            label.transform.parent = leaf.transform;
            label.transform.localPosition = Vector3.zero;
            label.transform.localScale = Vector3.one;
            var text = label.transform.GetChild(0);
            text.GetComponent<RectTransform>().localPosition = new Vector3(0, 0.04f, 0.02f);
            text.GetComponent<Text>().text = id.Value.ToString();
        }

        return leaf;
    }

    private static void AddEmptyNodeObject(Transform branch, Transform edge, float edgeLength)
    {
        var node = new GameObject(NodeName);
        node.transform.parent = branch;
        node.transform.localEulerAngles = Vector3.zero;

        var theta = DegreeToRadian(edge.localEulerAngles.x);
        var phi = DegreeToRadian(edge.localEulerAngles.y);

        var y = (float) Math.Cos(theta) * edgeLength;
        var xz = (float) Math.Sin(theta) * edgeLength;
        var x = (float) Math.Sin(phi) * xz;
        var z = (float) Math.Cos(phi) * xz;

        node.transform.localPosition = new Vector3(x, y, z);
        node.transform.Rotate(0, (float) GoldenAngle, 0);
    }

    // ----==== Helper functions ====---- //

    private void SerializeData(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        File.WriteAllText(TreeDataFilePath, json);
    }

    private InnerNode DesirializeData()
    {
        if (!File.Exists(TreeDataFilePath))
        {
            Debug.LogError("Connot load tree data, for there is no such file.");
            return null;
        }
        var json = File.ReadAllText(TreeDataFilePath);
        return JsonConvert.DeserializeObject<InnerNode>(json, JsonSerializerSettings);
    }

    private static float GetYSize(GameObject obj)
    {
        return obj.GetComponent<MeshRenderer>().bounds.size.y;
    }

    private static double DegreeToRadian(double angle)
    {
        return angle * Math.PI / 180.0;
    }

    private static double RadianToDegree(double angle)
    {
        return angle * 180.0 / Math.PI;
    }

    private static Color? HexToNullableColor(string hexcolor)
    {
        Color color;
        return ColorUtility.TryParseHtmlString(hexcolor, out color) ? new Color?(color) : null;
    }
    
    public static float KeepAngleIn360 (float angle) {
        return (angle %= 360) < 0 ? angle + 360 : angle;
    }
}