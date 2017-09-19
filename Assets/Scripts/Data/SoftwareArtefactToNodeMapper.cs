using System.Collections.Generic;
using System.Linq;
using Core;

namespace Data
{
    public static class SoftwareArtefactToNodeMapper
    {
        public static Node Map(SoftwareArtefact artefact)
        {
            if (artefact == null) return null;
            
            if (artefact.Children?.Count > 0)
            {
                return new InnerNode
                {
                    Name = artefact.Name,
                    Key = artefact.Key,
                    Edge = null,
                    Children = artefact.Children.Select(Map).ToList(),
                    Data = new List<InnerNodeData>()
                };
            }


            if (artefact.Children == null || artefact.Children.Count == 0)
            {
                return new Leaf
                {
                    Name = artefact.Name,
                    Key = artefact.Key,
                    Edge = null,
                    Data = artefact.Metrics?.Select(a => new LeafData
                    {
                        Key = a.Key,
                        Value = a.Value
                    }).ToList()
                };
            }

            return null;
        }
    }
}