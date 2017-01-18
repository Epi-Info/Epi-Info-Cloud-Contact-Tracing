using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.DataPersistence.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.DataPersistence.DataStructures;

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
            //Check if this Response exists
            Guid responseId = new Guid(criteria.SurveyAnswerIdList[0]);
            //bool responseExists = _surveyResponseDao.HasResponse(criteria);
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            //if (responseExists)
            //{
                result = _surveyResponseDao.GetSurveyResponse(criteria.SurveyAnswerIdList, criteria.UserPublishKey);
            //}
            //else
            //{
            //    //Retrieve response data sets from Epi 7 DataBase
            //    SurveyAnswerCriteria surveyAnswerCriteria = new SurveyAnswerCriteria();
            //    surveyAnswerCriteria.GetAllColumns = true;
            //    surveyAnswerCriteria.SurveyId = criteria.SurveyId;
            //    surveyAnswerCriteria.SurveyAnswerIdList.Add(criteria.SurveyAnswerIdList[0]);
            //    surveyAnswerCriteria.GridPageSize = 1;
            //    surveyAnswerCriteria.PageNumber = 1;
            //    surveyAnswerCriteria.IsSqlProject = criteria.IsSqlProject;
            //    result = _surveyResponseDao.GetFormResponseByFormId(surveyAnswerCriteria);
            //    if (result.Count > 0 && result[0].SqlData != null)
            //    {
            //        var dataList = result[0].SqlData.ToList();
            //        dataList.RemoveAt(0);

            //        //Build Response
            //        PreFilledAnswerRequest request = new PreFilledAnswerRequest();
            //        request.AnswerInfo.ResponseId = new Guid(criteria.SurveyAnswerIdList[0]);
            //        request.AnswerInfo.SurveyId = new Guid(criteria.SurveyId);
            //        request.AnswerInfo.UserId = criteria.UserId;
            //        request.AnswerInfo.SurveyQuestionAnswerList = dataList.ToDictionary(q => q.Key, a => a.Value);

            //        var response = SetSurveyAnswer(request);
            //    }
            //    result = _surveyResponseDao.GetSurveyResponse(criteria.SurveyAnswerIdList, criteria.UserPublishKey);
            //}
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(string formId, int gridPageNumber, bool isMobile)
        {
            List<SurveyResponseBO> result = null;

            int gridPageSize = AppSettings.GetIntValue(isMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);
            result = _surveyResponseDao.GetFormResponseByFormId(formId, gridPageNumber, gridPageSize);
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(SurveyAnswerCriteria criteria)
        {
            criteria.GridPageSize = AppSettings.GetIntValue(criteria.IsMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);

            List<SurveyResponseBO> result = _surveyResponseDao.GetFormResponseByFormId(criteria);
            return result;
        }
        public int GetNumberOfPages(string formId, bool isMobile)
        {
            int gridPageSize = AppSettings.GetIntValue(isMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);

            int result = _surveyResponseDao.GetFormResponseCount(formId);
            if (gridPageSize > 0)
            {
                result = (result + gridPageSize - 1) / gridPageSize;
            }
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
        public bool ValidateUser(UserAuthenticationRequestBO PassCodeBoObj)
        {
            string PassCode = PassCodeBoObj.PassCode;
            string ResponseId = PassCodeBoObj.ResponseId;
            List<string> ResponseIdList = new List<string>();
            ResponseIdList.Add(PassCodeBoObj.ResponseId);

            UserAuthenticationResponseBO results = _surveyResponseDao.GetAuthenticationResponse(PassCodeBoObj);



            bool ISValidUser = false;

            if (results != null && !string.IsNullOrEmpty(PassCode))
            {

                if (results.PassCode == PassCode)
                {
                    ISValidUser = true;


                }
                else
                {
                    ISValidUser = false;
                }
            }
            return ISValidUser;
        }
        //Save Pass code 
        public void SavePassCode(UserAuthenticationRequestBO pValue)
        {
            UserAuthenticationRequestBO result = pValue;
            _surveyResponseDao.UpdatePassCode(pValue);



        }
        // Get Authentication Response
        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO pValue)
        {
            UserAuthenticationResponseBO result = _surveyResponseDao.GetAuthenticationResponse(pValue);

            return result;

        }

		public SurveyResponseBO GetSurveyResponseStateById(SurveyAnswerCriteria Criteria)
		{
			string responseId = Criteria.SurveyAnswerIdList[0];
			SurveyResponseBO result = _surveyResponseDao.GetSurveyResponseState(responseId);
			return result;
		}

		public List<SurveyResponseBO> GetSurveyResponseBySurveyId(List<String> pSurveyIdList, Guid UserPublishKey)
        {
            List<SurveyResponseBO> result = _surveyResponseDao.GetSurveyResponseBySurveyId(pSurveyIdList, UserPublishKey);
            return result;
        }

        public List<SurveyResponseBO> GetSurveyResponse(List<string> SurveyAnswerIdList, string pSurveyId, DateTime pDateCompleted, int pStatusId, bool IsDraftMode = false)
        {
            List<SurveyResponseBO> result = _surveyResponseDao.GetSurveyResponse(SurveyAnswerIdList, pSurveyId, pDateCompleted, IsDraftMode, pStatusId);
            return result;
        }

        public void InsertSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            //Web.EF.EntitySurveyResponseDao eweSurveyResponseDAO = new Web.EF.EntitySurveyResponseDao();
            //eweSurveyResponseDAO.InsertSurveyResponse(surveyResponseBO);

            _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
        }

        public List<SurveyResponseBO> InsertSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int userId, bool isNewRecord = false)
        {
            foreach (var item in surveyResponseBOs)
            {
                _surveyResponseDao.InsertSurveyResponse(item);
                //_surveyResponseDao.InsertResponse(item.ResponseDetail);
            }

            return surveyResponseBOs;
        }

        public bool InsertChildSurveyResponse(SurveyResponseBO surveyResponseBO, SurveyInfoBO parentSurveyInfoBO, string relateParentId)
        {
            surveyResponseBO = surveyResponseBO.MergeIntoSurveyResponseBO(parentSurveyInfoBO, relateParentId);
            //_surveyResponseDao.InsertChildSurveyResponse(surveyResponseBO);
#region Ananth
            _surveyResponseDao.InsertSurveyResponse(surveyResponseBO);
#endregion Ananth
            return true;
        }

        public SurveyResponseBO UpdateSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
#if false // Code from Web Enter
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


		public List<SurveyResponseBO> UpdateSurveyResponse(List<SurveyResponseBO> pValue, int Status)
        {
            List<SurveyResponseBO> result = pValue;
            //Check if this respose has prent
            foreach (var Obj in pValue.ToList())
            {
                Obj.Status = Status;
                _surveyResponseDao.UpdateSurveyResponse(Obj);
            }
            return result;
        }
        public void UpdateFormResponse(SurveyResponseBO pValue)
        {
            _surveyResponseDao.UpdateSurveyResponse(pValue);
        }
        public bool DeleteSurveyResponse(SurveyResponseBO pValue)
        {
            bool result = false;

            _surveyResponseDao.UpdateRecordStatus(pValue.ResponseId, pValue.Status, RecordStatusChangeReason.Restore);
            result = true;

            return result;
        }
        public bool DeleteSurveyResponseInEditMode(SurveyResponseBO pValue, int Status = -1)
        { 
            _surveyResponseDao.UpdateRecordStatus(pValue.ResponseId, Status, RecordStatusChangeReason.Restore);
            return true;
            
        }
        public bool DeleteSingleSurveyResponse(SurveyResponseBO pValue)
        {
            bool result = false;

            _surveyResponseDao.DeleteSingleSurveyResponse(pValue);
            result = true;

            return result;
        }

        public PageInfoBO GetResponseSurveySize(List<string> SurveyResponseIdList, string SurveyId, DateTime pClosingDate, int BandwidthUsageFactor, bool IsDraftMode = false, int pSurveyType = -1, int pPageNumber = -1, int pPageSize = -1, int pResponseMaxSize = -1)
        {
             throw new NotImplementedException("SurveyResponse.GetResponseSurveySize is not implemented");
#if NotImplemented
           List<SurveyResponseBO> surveyResponseBOList = _surveyResponseDao.GetSurveyResponseSize(SurveyResponseIdList, SurveyId, pClosingDate, IsDraftMode, pSurveyType, pPageNumber, pPageSize, pResponseMaxSize);
            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(surveyResponseBOList, BandwidthUsageFactor, pResponseMaxSize);
            return result;
#endif
        }

        public PageInfoBO GetSurveyResponseBySurveyIdSize(List<string> SurveyIdList, Guid UserPublishKey, int BandwidthUsageFactor, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            throw new NotImplementedException("SurveyResponse.GetSurveyResponseBySurveyIdSize is not implemented");
#if NotImplemented
            List<SurveyResponseBO> surveyResponseBOList = _surveyResponseDao.GetSurveyResponseBySurveyIdSize(SurveyIdList, UserPublishKey, PageNumber, PageSize, ResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(surveyResponseBOList, BandwidthUsageFactor, ResponseMaxSize);
            return result;
#endif
        }
        public PageInfoBO GetSurveyResponseSize(List<string> SurveyResponseIdList, Guid UserPublishKey, int BandwidthUsageFactor, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            throw new NotImplementedException("SurveyResponse.GetSurveyResponseSize is not implemented");
#if NotImplemented
            List<SurveyResponseBO> surveyResponseBOList = _surveyResponseDao.GetSurveyResponseSize(SurveyResponseIdList, UserPublishKey, PageNumber, PageSize, ResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(surveyResponseBOList, BandwidthUsageFactor, ResponseMaxSize);
            return result;
#endif
        }
        public int GetNumberOfResponses(string FormId)
        {

            int result = _surveyResponseDao.GetFormResponseCount(FormId);

            return result;
        }

        public int GetNumberOfResponses(SurveyAnswerCriteria criteria)
        {

            int result = _surveyResponseDao.GetFormResponseCount(criteria.SurveyId);

            return result;
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string RootId)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetResponsesHierarchyIdsByRootId(RootId);


            return SurveyResponseBO;

        }



        public SurveyResponseBO GetFormResponseByParentRecordId(string ParentRecordId)
        {
            SurveyResponseBO SurveyResponseBO = new SurveyResponseBO();

            SurveyResponseBO = _surveyResponseDao.GetFormResponseByParentRecordId(ParentRecordId);
            return SurveyResponseBO;
        }

        public List<SurveyResponseBO> GetAncestorResponseIdsByChildId(string ChildId)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetAncestorResponseIdsByChildId(ChildId);


            return SurveyResponseBO;

        }

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string ResponseId, string SurveyId)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetResponsesByRelatedFormId(ResponseId, SurveyId);


            return SurveyResponseBO;

        }

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string ResponseId, SurveyAnswerCriteria Criteria)
        {
            List<SurveyResponseBO> SurveyResponseBO = new List<SurveyResponseBO>();

            SurveyResponseBO = _surveyResponseDao.GetResponsesByRelatedFormId(ResponseId, Criteria);


            return SurveyResponseBO;

        }

        public SurveyResponseBO GetResponse(string ResponseId)
        {
            var surveyResponseBO = _surveyResponseDao.GetSingleResponse(ResponseId);
            return surveyResponseBO;
        }

        public void DeleteResponse(FormResponseDetail formResponseDetail)
        {
            throw new NotImplementedException("SurveyResponse.DeleteResponseXml is not implemented");
#if NotImplemented
            _eweSurveyResponseDao.DeleteResponseXml(ResponseXmlBO);
#endif
        }

        public void UpdateRecordStatus(string responseId, int statusId, RecordStatusChangeReason reasonForStatusChange)
        {
            _surveyResponseDao.UpdateRecordStatus(responseId, statusId, reasonForStatusChange);
        }
        public PreFilledAnswerResponse SetSurveyAnswer(PreFilledAnswerRequest request)
        {
            throw new NotImplementedException("SurveyResponse.SetSurveyAnswer is not implemented");
#if NotImplemented
            string SurveyId = request.AnswerInfo.SurveyId.ToString();
            string ResponseId = request.AnswerInfo.ResponseId.ToString();
            Guid ParentRecordId = request.AnswerInfo.ParentRecordId;
            Dictionary<string, string> ErrorMessageList = new Dictionary<string, string>();
            PreFilledAnswerResponse response;


            SurveyResponseBO SurveyResponse = new SurveyResponseBO();
            UserAuthenticationRequestBO UserAuthenticationRequestBO = new UserAuthenticationRequestBO();
            //Get Survey Info (MetaData)
            List<SurveyInfoBO> SurveyBOList = GetSurveyInfo(request);
            //Build Survey Response Xml

            string Xml = CreateResponseXml(request, SurveyBOList);
            //Validate Response values

            ErrorMessageList = ValidateResponse(SurveyBOList, request);

            if (ErrorMessageList.Count() > 0)
            {
                response = new PreFilledAnswerResponse();
                response.ErrorMessageList = ErrorMessageList;
                response.Status = ((Message)1).ToString();
            }
            else
            {
                //Insert Survey Response
                SurveyResponse = _surveyResponseDao.GetSingleResponse(request.AnswerInfo.ResponseId.ToString());
                if (SurveyResponse.SurveyId == null)
                {
                    SurveyResponse = InsertSurveyResponse(Mapper.ToBusinessObject(Xml, request.AnswerInfo.SurveyId.ToString(), request.AnswerInfo.ParentRecordId.ToString(), request.AnswerInfo.ResponseId.ToString(), request.AnswerInfo.UserId));
                    response = new PreFilledAnswerResponse();
                    response.Status = ((Message)2).ToString();
                }
                else
                {
                    UpdateFormResponse(Mapper.ToBusinessObject(Xml, request.AnswerInfo.SurveyId.ToString(), request.AnswerInfo.ParentRecordId.ToString(), request.AnswerInfo.ResponseId.ToString(), request.AnswerInfo.UserId));
                    response = new PreFilledAnswerResponse();
                    response.Status = ((Message)2).ToString();
                }
            }


            return response;
#endif
        }

        private Dictionary<string, string> ValidateResponse(List<SurveyInfoBO> SurveyBOList, PreFilledAnswerRequest request)
        {
            throw new NotImplementedException("SurveyResponse.ValidateResponse is not implemented");
#if NotImplemented

            XDocument SurveyXml = new XDocument();
            foreach (var item in SurveyBOList)
            {
                SurveyXml = XDocument.Parse(item.XML);
            }
            Dictionary<string, string> MessageList = new Dictionary<string, string>();
            Dictionary<string, string> FieldNotFoundList = new Dictionary<string, string>();
            Dictionary<string, string> WrongFieldTypeList = new Dictionary<string, string>();
            SurveyResponseXML Implementation = new SurveyResponseXML(request, SurveyXml);
            FieldNotFoundList = Implementation.ValidateResponseFileds();
            //WrongFieldTypeList = Implementation.ValidateResponseFiledTypes();
            MessageList = MessageList.Union(FieldNotFoundList).Union(WrongFieldTypeList).ToDictionary(k => k.Key, v => v.Value);
            return MessageList;

#endif
        }
        private List<SurveyInfoBO> GetSurveyInfo(PreFilledAnswerRequest request)
        {
            throw new NotImplementedException("SurveyResponse.GetSurveyInfo is not implemented");
#if NotImplemented

            List<string> SurveyIdList = new List<string>();
            string SurveyId = request.AnswerInfo.SurveyId.ToString();
            string OrganizationId = request.AnswerInfo.OrganizationKey.ToString();
            //Guid UserPublishKey = request.AnswerInfo.UserPublishKey;
            List<SurveyInfoBO> SurveyBOList = new List<SurveyInfoBO>();



            SurveyIdList.Add(SurveyId);

            SurveyInfoRequest pRequest = new SurveyInfoRequest();
            var criteria = pRequest.Criteria as Epi.Cloud.Common.Criteria.SurveyInfoCriteria;

            var entityDaoFactory = new EF.EntityDaoFactory();
            var surveyInfoDao = entityDaoFactory.SurveyInfoDao;
            SurveyInfo implementation = new SurveyInfo(surveyInfoDao);

            SurveyBOList = implementation.GetSurveyInfo(SurveyIdList, criteria.ClosingDate, OrganizationId, criteria.SurveyType, criteria.PageNumber, criteria.PageSize);//Default 

            return SurveyBOList;
#endif
        }

        public bool HasResponse(string childFormId, string parentResponseId)
        {
            SurveyAnswerCriteria SurveyAnswerCriteria = new SurveyAnswerCriteria();
            SurveyAnswerCriteria.SurveyId = childFormId;
            SurveyAnswerCriteria.SurveyAnswerIdList = new List<string>();
            SurveyAnswerCriteria.SurveyAnswerIdList.Add(parentResponseId);

            return _surveyResponseDao.HasResponse(SurveyAnswerCriteria);
        }

        public void UpdateRecordStatus(SurveyResponseBO SurveyResponseBO)
        {
             throw new NotImplementedException("SurveyResponse.GetResponseXml is not implemented");
#if NotImplemented
           if (SurveyResponseBO.Status == 1)
            {
                SurveyResponseBO.Status = 2;
            }

            _eweSurveyResponseDao.UpdateRecordStatus(SurveyResponseBO);
#endif
        }
    }
}
