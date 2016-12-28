using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Interfaces.DataInterfaces;

namespace Epi.Cloud.BLL
{
    public class FormInfo
    {
        private IFormInfoDao _formInfoDao;

        public FormInfo(IFormInfoDao formInfoDao)
        {
            _formInfoDao = formInfoDao;
        }

        public List<FormInfoBO> GetFormsInfo(int UserId, int CurrentOrgId)
        {
            //Owner Forms
            List<FormInfoBO> result = _formInfoDao.GetFormInfo(UserId, CurrentOrgId);
            return result;
        }

        public FormInfoBO GetFormInfoByFormId(string FormId, bool getMetadata, int UserId)
        {
            //Owner Forms
            FormInfoBO result = _formInfoDao.GetFormByFormId(FormId, getMetadata, UserId);

            result.HasDraftModeData = _formInfoDao.HasDraftRecords(FormId);

            return result;
        }
    }
}
