using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using AbstractNode;
using HoloToolkit.Unity;
using UniRx;

public class TreeBuilder : Singleton<TreeBuilder>
{
    // ----==== Meshes as public variables ====---- //
    public GameObject Floor;

    public GameObject Edge;

    public GameObject Leaf;

    public GameObject Label;
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

    private const string TreeDataFilePath = "Assets/StreamingAssets/TreeStructureTypes.json";

    private static readonly double GoldenRatio = (1 + Math.Sqrt(5)) / 2;
    private static readonly double GoldenAngle = RadianToDegree(2 * Math.PI - 2 * Math.PI / GoldenRatio);
    private static readonly double CircleThroughGoldenAngle = 360 / GoldenAngle;
    private static float _currentBranchRotation = 0;

    private readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    private Node data = new InnerNode
    {
        Data = new InnerNodeData
        {
            Id = "0"
        },
        Children = new List<Node>
        {
            new InnerNode
            {
                Data = new InnerNodeData
                {
                    Id = "00"
                },
                Children = new List<Node>
                {
                    new Leaf
                    {
                        Data = new LeafData
                        {
                            Color = "208000",
                            Id = "000"
                        }
                    },
                    new Leaf
                    {
                        Data = new LeafData
                        {
                            Color = "208000",
                            Id = "001"
                        }
                    }
                }
            },
            new InnerNode
            {
                Data = new InnerNodeData
                {
                    Id = "01"
                },
                Children = new List<Node>
                {
                    new Leaf
                    {
                        Data = new LeafData
                        {
                            Id = "010"
                        }
                    },
                    new Leaf
                    {
                        Data = new LeafData
                        {
                            Id = "011"
                        }
                    },
                    new InnerNode
                    {
                        Data = new InnerNodeData
                        {
                            Id = "012"
                        },
                        Children = new List<Node>
                        {
                            new Leaf
                            {
                                Data = new LeafData
                                {
                                    Id = "0120"
                                }
                            },
                            new Leaf
                            {
                                Data = new LeafData
                                {
                                    Id = "0121"
                                }
                            }
                        }
                    },
                    new Leaf
                    {
                        Data = new LeafData
                        {
                            Id = "013"
                        }
                    }
                }
            },
            new Leaf
            {
                Data = new LeafData
                {
                    Id = "02"
                }
            }
        }
    };

    // Use this for initialization
    void Start()
    {
        data = DesirializeData();

//        SerializeData(data);

        var trunkBase = AddTrunkBaseObject();

        GenerateTreeStructure(data, trunkBase.transform);
    }

    public void HandleClickSomehow(string id)
    {
        Debug.Log("Handling Click! ID: " + id);
        Node selected = data
                .Traverse(x => (x as InnerNode)?.Children)
                .FirstOrDefault(x => x.Data.Id == id);


        Debug.Log("Found node: " + Logger.Json(selected));
        if (selected != null)
            ((LeafData) selected.Data).Selected.Value ^= true;
    }

    /// <summary>
    /// Generates the unity tree according to the given data structure of the node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parent"></param>
    private void GenerateTreeStructure(Node node, Transform parent)
    {
        var trunk = AddBranchObject(parent, (InnerNodeData) node.Data);
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
        if (node is InnerNode)
        {
            var innerNode = (InnerNode) node;
            if (innerNode.Children == null) return;
            innerNode.SortChildren();
            var branchObjects = AddBranchObjects(parentObject, (InnerNodeData) innerNode.Data, innerNode.Children.Count, scale);
            scale *= 0.8f;
            for (var i = 0; i < innerNode.Children.Count; i++)
            {
                AddChildrenOfNode(innerNode.Children[i], branchObjects[i].transform.Find(NodeName), scale);
            }
        }
        else if (node is Leaf)
        {
            var leaf = (Leaf) node;
            AddLeafObject(parentObject, (LeafData) leaf.Data);
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
    private GameObject AddTrunkBaseObject()
    {
        return InstantiateObject(TreeName, parent: Floor.transform);
    }

    private List<GameObject> AddBranchObjects(Transform parent, InnerNodeData data, int count, float scale)
    {
        var branches = new List<GameObject>();

        for (var i = 0; i < count; i++)
        {
            // Add a new branch and subsequent edge and node
            branches.Add(AddBranchObject(parent, data, count, i, scale));
        }

        return branches;
    }

    private GameObject AddBranchObject(Transform parent, InnerNodeData data, int siblingCount = 1, int siblingIndex = 0,
        float scale = DefaultScale)
    {
        // Add Branch as new origin
        var branchObject = AddEmptyBranchObject(parent, siblingCount, siblingIndex);
        // Store the height of the unrotated edge
        float edgeLength;
        var edge = AddEdgeObject(branchObject.transform, siblingCount, siblingIndex, scale, out edgeLength);

        // Add node at the end 
        AddEmptyNodeObject(branchObject.transform, edge.transform, edgeLength, data);

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
        var edgeObject = InstantiateObject(EdgeName, Edge, branch.transform, localScale: scale * BaseAspectRatio);
        edgeLength = GetYSize(edgeObject);
        edgeObject.AddComponent<NodeInputHandler>();

        if (!GrowInDirectionOfBranches) RotateBranchOrEdge(edgeObject.transform, siblingsCount, siblingIndex);
        else edgeObject.transform.localEulerAngles = Vector3.zero;

        return edgeObject;
    }


    private GameObject AddLeafObject(Transform parent, LeafData data)
    {
        var leafObject = InstantiateObject(LeafName, Leaf, parent);
        var height = GetYSize(leafObject);
        leafObject.AddComponent<Billboard>();
        leafObject.AddComponent<NodeInputHandler>();
        leafObject.AddComponent<ID>(data.Id);

        var color = HexToNullableColor(data.Color);
        if (color != null) leafObject.GetComponent<MeshRenderer>().material.color = color.Value;

        var label = InstantiateObject(LabelName, Label, leafObject.transform,
            Vector3.up * (height + DistanceLeafToLabel),
            Default3DTextScale, isActive: false);
        data.Selected = new ReactiveProperty<bool>(false);
        data.Selected.Subscribe(isActive => label.SetActive(isActive));
        label.GetComponent<TextMesh>().text = data.Text;

        return leafObject;
    }

    private void AddEmptyNodeObject(Transform branch, Transform edge, float edgeLength, InnerNodeData data)
    {
        var theta = DegreeToRadian(edge.localEulerAngles.x);
        var phi = DegreeToRadian(edge.localEulerAngles.y);

        var y = (float) Math.Cos(theta) * edgeLength;
        var xz = (float) Math.Sin(theta) * edgeLength;
        var x = (float) Math.Sin(phi) * xz;
        var z = (float) Math.Cos(phi) * xz;

        var node = InstantiateObject(NodeName, parent: branch, localPosition: new Vector3(x, y, z));
        node.transform.Rotate(0, (float) GoldenAngle, 0);
        node.AddComponent<ID>(data.Id);

        var label = InstantiateObject(LabelName, Label, node.transform, Vector3.down * DistanceNodeToLabel,
            isActive: false);
        label.GetComponent<TextMesh>().text = data.Text;
        label.AddComponent<Billboard>();
    }

    // ----==== Helper functions ====---- //

    private static GameObject InstantiateObject(string objName = null, GameObject original = null,
        Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null,
        Vector3? localEulerAngles = null, bool isActive = true)
    {
        localPosition = localPosition ?? Vector3.zero;
        localEulerAngles = localEulerAngles ?? Vector3.one;

        var gameObj = original == null ? new GameObject(objName) : Instantiate(original);
        gameObj.SetActive(isActive);
        if (parent != null) gameObj.transform.SetParent(parent);
        if (objName != null) gameObj.name = objName;
        if (localScale != null) gameObj.transform.localScale = localScale.Value;
        gameObj.transform.localPosition = localPosition.Value;
        gameObj.transform.localEulerAngles = localEulerAngles.Value;

        return gameObj;
    }

    private void SerializeData(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, Formatting.Indented, JsonSerializerSettings);
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

    public static Color? HexToNullableColor(string hexcolor)
    {
        Color color;
        return ColorUtility.TryParseHtmlString(hexcolor, out color) ? new Color?(color) : null;
    }

    public static float KeepAngleIn360(float angle)
    {
        return (angle %= 360) < 0 ? angle + 360 : angle;
    }
}