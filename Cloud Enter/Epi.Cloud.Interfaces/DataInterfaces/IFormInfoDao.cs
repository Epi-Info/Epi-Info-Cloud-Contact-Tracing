using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    public interface IFormInfoDao
    {
        List<FormInfoBO> GetFormInfo(int userId, int currentOrgId);
        FormInfoBO GetFormByFormId(string formId, int userId);
        FormInfoBO GetFormByFormId(string formId);
        bool HasDraftRecords(string formId);
    }
}
