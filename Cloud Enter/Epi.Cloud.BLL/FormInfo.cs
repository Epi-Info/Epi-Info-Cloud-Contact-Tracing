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

        public List<FormInfoBO> GetFormsInfo(int userId, int currentOrgId)
        {
            //Owner Forms
            List<FormInfoBO> result = _formInfoDao.GetFormInfo(userId, currentOrgId);
            return result;
        }

        public FormInfoBO GetFormInfoByFormId(string formId, int userId)
        {
            //Owner Forms
            FormInfoBO result = new FormInfoBO();
            if (userId > 0)
            {
                result = _formInfoDao.GetFormByFormId(formId, userId);
            }

            result.HasDraftModeData = _formInfoDao.HasDraftRecords(formId);

            return result;
        }
    }
}
