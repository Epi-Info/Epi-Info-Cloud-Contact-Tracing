using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    public interface IFormInfoDao
    {
        List<FormInfoBO> GetFormInfo(int userId, int currentOrgId);
        FormInfoBO GetFormByFormId(string formId, bool getMetadata, int userId);
        FormInfoBO GetFormByFormId(string formId);
        bool HasDraftRecords(string formId);
    }
}
