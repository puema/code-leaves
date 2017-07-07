using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class TreeBuilder : MonoBehaviour
{
    // ----==== Meshes as public variables ====---- //
    public GameObject Edge;

    public GameObject Leaf;

    public GameObject Label;
    // -------------------------------------------- //

    // ----==== Flags ====---- //
    public bool UseAllwaysMainTrunk;

    public bool SingleLeafMode = true;
    
    public bool InitWithIds;
    // ----------------------- //

    private const float XScale = 1;
    private const float YScale = 10;
    private const float ZScale = 1;
    private static readonly Vector3 BaseAspectRatio = new Vector3(XScale, YScale, ZScale);
    private const float DefaultScale = 1;
    private const float DefaultRotation = 0;

    private const string TreeName = "Tree";
    private const string BranchName = "Branch";
    private const string NodeName = "Node";
    private const string EdgeName = "Edge";
    private const string LeafName = "Leaf";
    private const string LabelName = "Label";
    private const string TreeDataFilePath = "Assets/StreamingAssets/TreeStructure.json";


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
//        SerializeData(this.data);
        var data = DesirializeData();

        var root = new GameObject(TreeName);
        root.transform.position = Vector3.zero + Vector3.forward * 2;

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
        AddChildrenOfNode(node, trunk.transform.Find(NodeName), 30, 0.8f);
    }

    /// <summary>
    /// Adds the children of a node to the given parent transform 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parentObject"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    private void AddChildrenOfNode(Node node, Transform parentObject, float rotation, float scale)
    {
        if (node.GetType() == typeof(InnerNode))
        {
            var innerNode = (InnerNode) node;
            var branchObjects = AddBranchObjects(parentObject, innerNode.Children.Count, rotation, scale);
            rotation = rotation.Equals(0) ? 30 : rotation - 10;
            scale = scale * 0.8f;
            for (var i = 0; i < innerNode.Children.Count; i++)
            {
                AddChildrenOfNode(innerNode.Children[i], branchObjects[i].transform.Find(NodeName), rotation, scale);
            }
        }
        else if (node.GetType() == typeof(Leaf))
        {
            var leaf = (Leaf) node;
            AddLeafObject(parentObject, HexToNullableColor(leaf.Data.Color), leaf.Data.Id);
        }
        else
        {
            Debug.LogError("Unknown type of node, aborting structure generation");
        }
    }


    private List<GameObject> AddBranchObjects(Transform parent, int count, float rotation, float scale)
    {
        var branches = new List<GameObject>();

        if (count % 2 != 0)
        {
            branches.Add(AddBranchObject(parent, scale: scale));
            count -= 1;
        }

        for (var i = 0; i < count; i++)
        {
            // Add a new branch and subsequent edge and node
            branches.Add(AddBranchObject(parent, count, i, rotation, scale));
        }

        return branches;
    }

    private GameObject AddBranchObject(Transform parent, int siblings = 1, int siblingNumber = 0,
        float rotation = DefaultRotation, float scale = DefaultScale)
    {
        // Add Branch as new origin
        var branchObject = AddEmptyBranchObject(parent);

        // Store the height of the unrotated edge
        float edgeLength;
        var edge = AddEdgeObject(branchObject.transform, siblings, siblingNumber, rotation, scale, out edgeLength);

        // Add node at the end 
        AddEmptyNodeObject(branchObject.transform, edge.transform, edgeLength);

        return branchObject;
    }

    private static GameObject AddEmptyBranchObject(Transform parent)
    {
        var branch = new GameObject(BranchName);
        branch.transform.parent = parent;
        branch.transform.localPosition = Vector3.zero;
        return branch;
    }

    private GameObject AddEdgeObject(Transform branch, int siblings, int siblingNumber, float angle, float scale,
        out float edgeLength)
    {
        var edge = Instantiate(Edge);
        edge.name = EdgeName;
        edge.transform.parent = branch.transform;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.localScale = scale * BaseAspectRatio;
        // Store unrotated y size to the edge length, needed for the later calculation of the node coordinates
        edgeLength = GetYSize(edge);

        var xAngle = angle;
        var yAngle = 360 / siblings * siblingNumber;
        edge.transform.localEulerAngles = new Vector3(xAngle, yAngle, 0);
        return edge;
    }

    private GameObject AddLeafObject(Transform parent, Color? color = null, [CanBeNull] string id = null)
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
            text.GetComponent<Text>().text = id;
        }
        
        return leaf;
    }

    private static void AddEmptyNodeObject(Transform branch, Transform edge, float edgeLength)
    {
        var node = new GameObject(NodeName);
        node.transform.parent = branch;

        var theta = DegreeToRadian(edge.eulerAngles.x);
        var phi = DegreeToRadian(edge.eulerAngles.y);

        var y = (float) Math.Cos(theta) * edgeLength;
        var xz = (float) Math.Sin(theta) * edgeLength;
        var x = (float) Math.Sin(phi) * xz;
        var z = (float) Math.Cos(phi) * xz;

        node.transform.localPosition = new Vector3(x, y, z);
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

    private static Color? HexToNullableColor(string hexcolor)
    {
        Color color;
        return ColorUtility.TryParseHtmlString(hexcolor, out color) ? new Color?(color) : null;
    }
}