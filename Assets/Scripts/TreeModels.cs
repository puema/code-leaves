using System.Collections.Generic;

public class Tree
{
    private Node root;
}

public class Edge
{
    public Node Start;
    public Node End;
    public EdgeData Data;
}

public class Node
{
    public Node parent;
    public List<Edge> Edge;
    public Node end;
    public NodeData data;
}

public struct NodeData
{
    
}

public struct EdgeData
{
    
}

public struct LeafData
{
    
}