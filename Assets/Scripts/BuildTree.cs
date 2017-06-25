using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTree : MonoBehaviour
{
    public GameObject Edge;
    public GameObject Leaf;

    private const string Node = "Node";
    private const float XScale = 1;
    private const float YScale = 10;
    private const float ZScale = 1;
    private static readonly Vector3 BaseAspectRatio = new Vector3(XScale, YScale, ZScale);
    private static readonly Vector3 BasePosition = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        var root = new GameObject("Tree");
        root.transform.position = BasePosition + Vector3.forward * 2;

        var trunk = AddBranches(root.transform, 1, 0, 1);
        var branches0 = AddBranches(trunk[0].transform.Find(Node), 3, 30, 0.8f);
//      AddLeaf(branches0[1].transform.Find(Node));
        AddLeaf(branches0[2].transform.Find(Node));

        var branches10 = AddBranches(branches0[0].transform.Find(Node), 2, 20, 0.5f);
        AddLeaf(branches10[0].transform.Find(Node));
        AddLeaf(branches10[1].transform.Find(Node));

        var branches11 = AddBranches(branches0[1].transform.Find(Node), 3, 20, 0.5f);
        AddLeaf(branches11[1].transform.Find(Node));
        AddLeaf(branches11[2].transform.Find(Node));
        
        var branches21 = AddBranches(branches11[0].transform.Find(Node), 2, 10, 0.3f);

        AddLeaf(branches21[0].transform.Find(Node));
        AddLeaf(branches21[1].transform.Find(Node));
    }

    private List<GameObject> AddBranches(Transform parent, int count, float rotation, float scale)
    {
        var branches = new List<GameObject>();

        for (var i = 0; i < count; i++)
        {
            var branch = AddBranch(parent);
            
            float edgeLength;
            var edge = AddEdge(branch.transform, count, rotation, scale, i, out edgeLength);
            
            AddNode(branch.transform, edge.transform, edgeLength);
            
            branches.Add(branch);
        }

        return branches;
    }

    private static GameObject AddBranch(Transform parent)
    {
        var branch = new GameObject("Branch");
        branch.transform.parent = parent;
        branch.transform.localPosition = Vector3.zero;
        return branch;
    }

    private GameObject AddEdge(Transform branch, int siblings, float angle, float scale, int i, out float edgeLength)
    {
        var edge = Instantiate(Edge);
        edge.transform.parent = branch.transform;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.localScale = scale * BaseAspectRatio;
        edgeLength = GetYSize(edge);

        var xAngle = angle;
        var yAngle = 360 / siblings * i;
        edge.transform.localEulerAngles = new Vector3(xAngle, yAngle, 0);
        return edge;
    }

    private GameObject AddLeaf(Transform parent)
    {
        var leaf = Instantiate(Leaf);
        leaf.AddComponent<Billboard>();
        leaf.SetActive(true);
        leaf.transform.parent = parent;
        leaf.transform.localPosition = Vector3.zero;
        return leaf;
    }

    private static void AddNode(Transform branch, Transform edge, float edgeLength)
    {
        var node = new GameObject("Node");
        node.transform.parent = branch;

        var alpha = edge.eulerAngles.x;
        var beta = edge.eulerAngles.y;
        
        var y = (float) Math.Cos(DegreeToRadian(alpha)) * edgeLength;
        var xz = (float) Math.Sin(DegreeToRadian(alpha)) * edgeLength;
        var x = (float) Math.Sin(DegreeToRadian(beta)) * xz;
        var z = (float) Math.Cos(DegreeToRadian(beta)) * xz;
        
        node.transform.localPosition = new Vector3(x, y, z);
    }

    private static float GetYSize(GameObject obj)
    {
        return obj.GetComponent<MeshRenderer>().bounds.size.y;
    }

    private static double DegreeToRadian(double angle)
    {
        return angle * Math.PI / 180.0;
    }
}