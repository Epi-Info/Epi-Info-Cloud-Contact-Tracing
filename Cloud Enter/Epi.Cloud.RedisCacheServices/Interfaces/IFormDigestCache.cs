using System;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.CacheServices
{
    public interface IFormDigestCache
    {
        bool FormDigestsExists(Guid projectId);
        FormDigest[] GetFormDigests(Guid projectId);
        bool SetFormDigests(Guid projectId, FormDigest[] formDigests);
        void ClearFormDigests(Guid projectId);

    }
}
