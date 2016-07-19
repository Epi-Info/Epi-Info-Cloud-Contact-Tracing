using System;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IFormDigestCache
    {
        bool FormDigestsExists(string projectId);
        FormDigest[] GetFormDigests(string projectId);
        bool SetFormDigests(string projectId, FormDigest[] formDigests);
        void ClearFormDigests(string projectId);

    }
}
