using System;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.DataEntryServices
{
    public class SurveyResponseProvider
    {
        public enum Message
        {
            Failed = 1,
            Success = 2,
        }

        private ISurveyResponseDao _surveyResponseDao;

        public SurveyResponseProvider(ISurveyResponseDao surveyResponseDao)
        {
            _surveyResponseDao = surveyResponseDao;
        }

        public List<SurveyResponseBO> GetSurveyResponseById(IResponseContext responseContext, SurveyAnswerCriteria criteria, List<SurveyInfoBO> surveyBOList = null)
        {
            List<SurveyResponseBO> result = _surveyResponseDao.GetSurveyResponse(responseContext);
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(IResponseContext responseContext, SurveyAnswerCriteria criteria)
        {
            criteria.GridPageSize = AppSettings.GetIntValue(criteria.IsMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);
            List<SurveyResponseBO> result = _surveyResponseDao.GetFormResponseByFormId(responseContext, criteria);
            return result;
        }

        public int GetNumberOfPages(SurveyAnswerCriteria criteria)
        {
            criteria.GridPageSize = AppSettings.GetIntValue(criteria.IsMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);

            int result = _surveyResponseDao.GetFormResponseCount(criteria);
            if (criteria.GridPageSize > 0)
            {
                result = (result + criteria.GridPageSize - 1) / criteria.GridPageSize;
            }
            return result;
        }

        //Validate User
        public bool ValidateUser(UserAuthenticationRequestBO uarBO)
        {
            string passCode = uarBO.PassCode;
            string responseId = uarBO.ResponseId;
            List<string> responseIdList = new List<string>();
            responseIdList.Add(responseId);

            UserAuthenticationResponseBO results = _surveyResponseDao.GetAuthenticationResponse(uarBO);

            bool isValidUser = false;

            if (results != null && !string.IsNullOrEmpty(passCode))
            {
                if (results.PassCode == passCode)
                {
                    isValidUser = true;
                }
                else
                {
                    isValidUser = false;
                }
            }
            return isValidUser;
        }

        //Save Pass code 
        public void SavePassCode(UserAuthenticationRequestBO uarBO)
        {
            _surveyResponseDao.UpdatePassCode(uarBO);
        }

        // Get Authentication Response
        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO uarBO)
        {
            UserAuthenticationResponseBO result = _surveyResponseDao.GetAuthenticationResponse(uarBO);
            return result;
        }

		public SurveyResponseBO GetSurveyResponseStateById(IResponseContext responseContext)
		{
            responseContext.ResolveMetadataDependencies();

            SurveyResponseBO result = _surveyResponseDao.GetSurveyResponseState(responseContext);
			return result;
		}

        public void InsertSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            ((IResponseContext)surveyResponseBO).ResolveMetadataDependencies();
            _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
        }

        public List<SurveyResponseBO> InsertSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int userId, bool isNewRecord = false)
        {
            foreach (var surveyResponseBO in surveyResponseBOs)
            {
                ((IResponseContext)surveyResponseBO).ResolveMetadataDependencies();
                _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
            }

            return surveyResponseBOs;
        }

        public bool InsertChildSurveyResponse(SurveyResponseBO surveyResponseBO, SurveyInfoBO parentSurveyInfoBO, string parentResponseId)
        {
            surveyResponseBO = surveyResponseBO.MergeIntoSurveyResponseBO(parentSurveyInfoBO, parentResponseId);
            ((IResponseContext)surveyResponseBO).ResolveMetadataDependencies();
            _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
            return true;
        }

        public SurveyResponseBO UpdateSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            // TODO: Check code from Web Enter
#if false 
            //Check if this respose has parent
            string ParentId = SurveyResponseDao.GetResponseParentId(pValue.ResponseId);
            Guid ParentIdGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(ParentId))
            {
                ParentIdGuid = new Guid(ParentId);
            }
#endif
            ((IResponseContext)surveyResponseBO).ResolveMetadataDependencies();
            _surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);

            SurveyResponseBO result = _surveyResponseDao.GetResponse(surveyResponseBO);
            return result;
        }

		public List<SurveyResponseBO> UpdateSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int status, RecordStatusChangeReason reasonForStatusChange)
		{
			List<SurveyResponseBO> result = surveyResponseBOs;
			//Check if this respose has parent
			foreach (var surveyResponseBO in surveyResponseBOs)
			{
                ((IResponseContext)surveyResponseBO).ResolveMetadataDependencies();
				surveyResponseBO.Status = status;
				surveyResponseBO.ReasonForStatusChange = reasonForStatusChange;
				_surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);
			}
			return result;
		}

        public bool DeleteSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            bool result = false;
            var responseContext = surveyResponseBO.ToResponseContext();
            _surveyResponseDao.UpdateRecordStatus(responseContext, surveyResponseBO.Status, RecordStatusChangeReason.DeleteResponse);
            result = true;

            return result;
        }

        public bool DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponseBO, int status = -1)
        { 
            var responseContext = surveyResponseBO.ToResponseContext();
            _surveyResponseDao.UpdateRecordStatus(responseContext, status, RecordStatusChangeReason.Restore);
            return true;
            
        }

        public int GetNumberOfResponses(SurveyAnswerCriteria criteria)
        {

            int result = _surveyResponseDao.GetFormResponseCount(criteria.SurveyId);

            return result;
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(IResponseContext responceContext) // string formId, string parentResponseId)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetResponsesHierarchyIdsByRootId(responceContext);


            return SurveyResponseBO;

        }

        public void UpdateRecordStatus(IResponseContext responceContext, int statusId, RecordStatusChangeReason reasonForStatusChange)
        {
            _surveyResponseDao.UpdateRecordStatus(responceContext, statusId, reasonForStatusChange);
        }

        public bool HasResponse(IResponseContext responseContext)
        {
            return _surveyResponseDao.HasResponse(responseContext);
        }
    }
}
