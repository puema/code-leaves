using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Node
{
}

[Serializable]
public class InnerNode : Node
{
    public InnerNodeData Data;
    public List<Node> Children;
}

[Serializable]
public class Leaf : Node
{
    public LeafData Data;
}

// ----==== Without abstract to be better serializable ====---- //
//[Serializable]
//public class Node
//{
//    public InnerNodeData Data;
//    public List<Node> Children;
//    public List<Leaf> Leaves;
//}

//[Serializable]
//public class Leaf
//{
//    public LeafData Data;
//}

[Serializable]
public struct InnerNodeData
{
    public string Id { get; set; }
}

[Serializable]
public struct LeafData
{
    public string Id { get; set; }
    public string Color { get; set; }
}