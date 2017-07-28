using System;
using System.Collections.Generic;

public abstract class AbstractSoftware
{
    public string Name { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public Dictionary<string, double> Utilization { get; set; }
    public HierarchyElement RootElement { get; set; }
}

public abstract class HierarchyElement
{
    public string Name { get; set; }
    public string Key { get; set; }
    public List<HierarchyElement> Children { get; set; }
    public List<ElementAssociation> Assoziations { get; set; }
    public List<Metric> Metrics { get; set; }
}

public abstract class ElementAssociation
{
    public string CodeSnippet { get; set; }
    public string Address { get; set; }
}

public struct Metric
{
    public string Key { get; set; }
    public double Value { set; get; }
    public List<string> CodeSnippets { get; set; }
}