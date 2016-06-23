using System.Collections.Generic;
using System.Linq;

namespace Epi.Cloud.Common.Metadata
{
    public static class ProjectExtensions
    {
        public static ProjectDigest[] FilterDigest(this ProjectDigest[] projectDigest, string[] fieldNames)
        {
            List<ProjectDigest> filteredDigest = new List<ProjectDigest>();
            if (projectDigest != null)
            {
                foreach (var digest in projectDigest)
                {
                    var filteredFieldsNames = new List<string>();
                    digest.FieldNames.Where(n => { if (fieldNames.Contains(n)) { filteredFieldsNames.Add(n); return true; } else return false; });
                    if (filteredFieldsNames.Count > 0)
                    {
                        filteredDigest.Add(new ProjectDigest(digest.ViewId, digest.PageId, digest.Position, filteredFieldsNames.ToArray()));
                    }
                }
            }
            return filteredDigest.ToArray();
        }
    }
}
