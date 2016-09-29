using System;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.CacheServices
{
    public interface IPageDigestCache
    {
        bool ProjectPageDigestsExists(Guid projectId);
        PageDigest[][] GetProjectPageDigests(Guid projectId);
        bool SetProjectPageDigests(Guid projectId, PageDigest[][] projectPageDigests);

        bool PageDigestsExists(Guid projectId, Guid formId);
        PageDigest[] GetPageDigests(Guid projectId, Guid formId);
        bool SetPageDigests(Guid projectId, Guid formId, PageDigest[] pageDigests);

        void ClearAllFormPageDigests(Guid projectId);
    }
}
