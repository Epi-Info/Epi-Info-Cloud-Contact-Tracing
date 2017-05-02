using System;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.Message;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Common.Exception;
using System.ServiceModel;
using Epi.Cloud.SurveyInfoServices.Extensions;

namespace Epi.Cloud.SurveyInfoServices
{
	public class SurveyInfoService : ISurveyInfoService
	{
		private readonly Epi.Cloud.SurveyInfoServices.DAO.SurveyInfoDao _surveyInfoDao;
		private readonly Epi.Cloud.SurveyInfoServices.DAO.FormInfoDao _formInfoDao;

		private string _accessToken;
		private string _userName;

		public SurveyInfoService(
			Epi.Cloud.SurveyInfoServices.DAO.SurveyInfoDao surveyInfoDao,
			Epi.Cloud.SurveyInfoServices.DAO.FormInfoDao formInfoDao)
		{
			_surveyInfoDao = surveyInfoDao;
			_formInfoDao = formInfoDao;
		}


		/// <summary>
		/// Validation options enum. Used in validation of messages.
		/// </summary>
		[Flags]
		private enum Validate
		{
			ClientTag = 0x0001,
			AccessToken = 0x0002,
			UserCredentials = 0x0004,
			All = ClientTag | AccessToken | UserCredentials
		}

		public SurveyInfoBO GetSurveyInfoByFormId(string formId)
		{
			return _surveyInfoDao.GetSurveyInfo(formId);
		}

		public SurveyInfoBO GetParentInfoByChildFormId(string childFormId)
		{
			SurveyInfoBO result = new SurveyInfoBO();

			result = _surveyInfoDao.GetParentInfoByChildId(childFormId);

			return result;
		}

         public SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest pRequest)
		{
			try
			{
				SurveyInfoResponse result = new SurveyInfoResponse (pRequest.RequestId);

				Dictionary<string, int> parentIdList = new Dictionary<string, int>();
				foreach (var item in pRequest.SurveyInfoList)
				{
					parentIdList.Add(item.SurveyId, item.ViewId);
				}
                var surveyInfoBOList = GetChildInfoByParentId(parentIdList);

                result.SurveyInfoList = surveyInfoBOList.ToSurveyInfoDTOList();

				return result;
			}
			catch (Exception ex)
			{
				CustomFaultException customFaultException = new CustomFaultException();
				customFaultException.CustomMessage = ex.Message;
				customFaultException.Source = ex.Source;
				customFaultException.StackTrace = ex.StackTrace;
				customFaultException.HelpLink = ex.HelpLink;
				throw new FaultException<CustomFaultException>(customFaultException);
			}
		}

        private List<SurveyInfoBO> GetChildInfoByParentId(Dictionary<string, int> parentIdList)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            foreach (KeyValuePair<string, int> item in parentIdList)
            {
                result = _surveyInfoDao.GetChildInfoByParentId(item.Key, item.Value);
            }
            return result;
        }

        public List<FormsHierarchyBO> GetFormsHierarchyIdsByRootFormId(string rootFormId)
        {
            List<SurveyInfoBO> SurveyInfoBOList = new List<SurveyInfoBO>();
            List<FormsHierarchyBO> result = new List<FormsHierarchyBO>();

            SurveyInfoBOList = _surveyInfoDao.GetFormsHierarchyIdsByRootFormId(rootFormId);
            foreach (var item in SurveyInfoBOList)
            {
                FormsHierarchyBO FormsHierarchyBO = new FormsHierarchyBO();
                FormsHierarchyBO.RootFormId = rootFormId;
                FormsHierarchyBO.FormId = item.SurveyId;
                FormsHierarchyBO.ViewId = item.ViewId;
                FormsHierarchyBO.SurveyInfo = item;
                if (item.SurveyId == rootFormId)
                {
                    FormsHierarchyBO.IsRoot = true;
                }
                result.Add(FormsHierarchyBO);
            }

            return result;
        }
    }

}
