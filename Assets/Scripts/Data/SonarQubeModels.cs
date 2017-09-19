namespace Data
{
    internal class SonarQubeTree
    {
        public SonarQubeComponent baseComponent;
        public SonarQubeComponent[] components;
        public SonarQubePaging paging;
    }

    internal class SonarQubeComponent
    {
        public string id;
        public string key;
        public string language;
        public SonarQubeMeasure[] measures;
        public string name;
        public string path;
        public SonarQubeQualifier qualifier;
    }

    internal class SonarQubeMeasure
    {
        public string metric;
        public float value;
        public SonarQubePeriod[] periods;
    }

    internal class SonarQubePeriod
    {
        public float index;
        public string value;
    }

    internal class SonarQubePaging
    {
        public int pageIndex;
        public int pageSize;
        public int total;
    }

    /// <summary>
    /// BRC - Sub-projects
    /// DIR - Directories
    /// FIL - Files
    /// TRK - Projects
    /// UTS - Unit Test Files
    /// </summary>
    internal enum SonarQubeQualifier
    {
        BRC, DIR, FIL, TRK, UTS
    }
    
    /// <summary>
    /// children: return the children components of the base component. Grandchildren components are not returned
    /// all: return all the descendants components of the base component. Grandchildren are returned.
    /// leaves: return all the descendant components (files, in general) which don't have other children. They are the leaves of the component tree.
    /// </summary>
    internal static class SonarQubeStrategy
    {
        internal const string Children = "children";
        internal const string All = "all";
        internal const string Leaves = "leaves";
    }
    
    
    internal static class SonarQubeSortField
    {
        internal const string Metric = "metric";
        internal const string MetricPeriod = "metricPeriod";
        internal const string Name = "name";
        internal const string Path = "path";
        internal const string Qualifier = "qualifier";
    }
    
    internal static class SonarQubeMetric
    {
        internal const string Coverage = "coverage";
        internal const string LOC = "ncloc";
        internal const string Complexity = "complexity";
    }
}