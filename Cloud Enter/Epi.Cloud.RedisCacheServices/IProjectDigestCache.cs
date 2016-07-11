using System;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.CacheServices
{
    public interface IProjectDigestCache
    {
        bool ProjectDigestExists(string projectId);
        ProjectDigest[] GetProjectDigest(string projectId);
        bool SetProjectDigest(string projectId, ProjectDigest[] projectDigest);
        void ClearProjectDigest(string projectId);

    }
}
