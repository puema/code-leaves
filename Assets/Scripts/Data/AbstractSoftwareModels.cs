using System;
using System.Collections.Generic;

namespace Data
{
    public abstract class AbstractSoftware
    {
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Dictionary<string, double> Utilization { get; set; }
        public SoftwareArtefact RootElement { get; set; }
    }

    public class SoftwareArtefact
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public List<SoftwareArtefact> Children { get; set; }
        public List<ArtefactAssociation> Assoziations { get; set; }
        public List<Metric> Metrics { get; set; }
    }

    public class ArtefactAssociation
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
}