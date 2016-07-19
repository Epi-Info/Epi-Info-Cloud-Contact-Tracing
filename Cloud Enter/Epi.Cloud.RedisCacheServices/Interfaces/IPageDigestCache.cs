using System;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IPageDigestCache
    {
        bool ProjectPageDigestsExists(string projectId);
        PageDigest[][] GetProjectPageDigests(string projectId);
        bool SetProjectPageDigests(string projectId, PageDigest[][] projectPageDigests);

        bool PageDigestsExists(string projectId, string formId);
        PageDigest[] GetPageDigests(string projectId, string formId);
        bool SetPageDigests(string projectId, string formId, PageDigest[] pageDigests);

        void ClearAllFormPageDigests(string projectId);
    }
}
