using System;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Interfaces.DataInterfaces;
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

        public List<SurveyResponseBO> GetSurveyResponseById(SurveyAnswerCriteria criteria, List<SurveyInfoBO> surveyBOList = null)
        {
            Guid responseId = new Guid(criteria.SurveyAnswerIdList[0]);
            List<SurveyResponseBO> result = _surveyResponseDao.GetSurveyResponse(criteria.SurveyAnswerIdList, criteria.UserPublishKey);
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(SurveyAnswerCriteria criteria)
        {
            criteria.GridPageSize = AppSettings.GetIntValue(criteria.IsMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);
            List<SurveyResponseBO> result = _surveyResponseDao.GetFormResponseByFormId(criteria);
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

		public SurveyResponseBO GetSurveyResponseStateById(SurveyAnswerCriteria criteria)
		{
			string responseId = criteria.SurveyAnswerIdList[0];
			SurveyResponseBO result = _surveyResponseDao.GetSurveyResponseState(responseId);
			return result;
		}

        public void InsertSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
        }

        public List<SurveyResponseBO> InsertSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int userId, bool isNewRecord = false)
        {
            foreach (var item in surveyResponseBOs)
            {
                _surveyResponseDao.InsertSurveyResponse(item);
            }

            return surveyResponseBOs;
        }

        public bool InsertChildSurveyResponse(SurveyResponseBO surveyResponseBO, SurveyInfoBO parentSurveyInfoBO, string relateParentId)
        {
            surveyResponseBO = surveyResponseBO.MergeIntoSurveyResponseBO(parentSurveyInfoBO, relateParentId);
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
            _surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);

            SurveyResponseBO result = _surveyResponseDao.GetResponse(surveyResponseBO.ResponseId);
            return result;
        }

		public List<SurveyResponseBO> UpdateSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int status, RecordStatusChangeReason reasonForStatusChange)
		{
			List<SurveyResponseBO> result = surveyResponseBOs;
			//Check if this respose has parent
			foreach (var surveyResponseBO in surveyResponseBOs)
			{
				surveyResponseBO.Status = status;
				surveyResponseBO.ReasonForStatusChange = reasonForStatusChange;
				_surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);
			}
			return result;
		}

        public bool DeleteSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            bool result = false;

            _surveyResponseDao.UpdateRecordStatus(surveyResponseBO.ResponseId, surveyResponseBO.Status, RecordStatusChangeReason.Restore);
            result = true;

            return result;
        }

        public bool DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponseBO, int status = -1)
        { 
            _surveyResponseDao.UpdateRecordStatus(surveyResponseBO.ResponseId, status, RecordStatusChangeReason.Restore);
            return true;
            
        }

        public int GetNumberOfResponses(SurveyAnswerCriteria criteria)
        {

            int result = _surveyResponseDao.GetFormResponseCount(criteria.SurveyId);

            return result;
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string rootId)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetResponsesHierarchyIdsByRootId(rootId);


            return SurveyResponseBO;

        }

        public void UpdateRecordStatus(string responseId, int statusId, RecordStatusChangeReason reasonForStatusChange)
        {
            _surveyResponseDao.UpdateRecordStatus(responseId, statusId, reasonForStatusChange);
        }

        public bool HasResponse(string childFormId, string parentResponseId)
        {
            SurveyAnswerCriteria SurveyAnswerCriteria = new SurveyAnswerCriteria();
            SurveyAnswerCriteria.SurveyId = childFormId;
            SurveyAnswerCriteria.SurveyAnswerIdList = new List<string>();
            SurveyAnswerCriteria.SurveyAnswerIdList.Add(parentResponseId);

            return _surveyResponseDao.HasResponse(SurveyAnswerCriteria);
        }
    }
}
