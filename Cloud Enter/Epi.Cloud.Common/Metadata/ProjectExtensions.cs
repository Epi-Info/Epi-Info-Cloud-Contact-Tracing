using System.Collections.Generic;
using System.Linq;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.Common.Metadata
{
    public static class ProjectExtensions
    {
        public static PageDigest[] FilterDigest(this PageDigest[] pageDigest, string[] fieldNames)
        {
            fieldNames = fieldNames.Select(n => n.ToLower()).ToArray();
            List<PageDigest> filteredDigest = new List<PageDigest>();
            if (pageDigest != null)
            {
                foreach (var digest in pageDigest)
                {
                    var filteredFields = new List<AbridgedFieldInfo>();
                    digest.Fields.Where(f => { if (fieldNames.Contains(f.FieldName.ToLower())) { filteredFields.Add(f); return true; } else return false; });
                    if (filteredFields.Count > 0)
                    {
                        filteredDigest.Add(new PageDigest(digest.FormName, digest.FormId, digest.ViewId, digest.IsRelatedView,
                                                          digest.PageName, digest.PageId, digest.Position, filteredFields.ToArray()));
                    }
                }
            }
            return filteredDigest.ToArray();
        }

        public static int PageIdFromPageNumber(this PageDigest[][] projectPageDigests, string formId, int pageNumber)
        {
            int pageId = 0;
            var formPageDigests = projectPageDigests.SingleOrDefault(p => p[0].FormId == formId);
            if (formPageDigests != null)
            {
                var pageDigest = formPageDigests.SingleOrDefault(p => p.PageNumber == pageNumber);
                if (pageDigest != null)
                {
                    pageId = pageDigest.PageId;
                }
            }
            return pageId;
        }

        public static int PageIdFromPageNumber(this PageDigest[] formPageDigests, int pageNumber)
        {
            int pageId = 0;
            var pageDigest = formPageDigests.SingleOrDefault(p => p.PageNumber == pageNumber);
            if (pageDigest != null)
            {
                pageId = pageDigest.PageId;
            }
            return pageId;
        }
    }
}
