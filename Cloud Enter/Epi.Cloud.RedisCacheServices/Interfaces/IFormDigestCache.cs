using System;
using Epi.Cloud.Common.Metadata;

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
