using System;
using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;
using System.Configuration;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.Message;
using Epi.Cloud.Common.Constants;

namespace Epi.Web.BLL
{
    public class SurveyResponse
    {
        public enum Message
        {
            Failed = 1,
            Success = 2,

        }
        private Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao _surveyResponseDao;

        public SurveyResponse(Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao pSurveyResponseDao)
        {
            _surveyResponseDao = pSurveyResponseDao;
        }

        public List<SurveyResponseBO> GetSurveyResponseById(SurveyAnswerCriteria Criteria, List<SurveyInfoBO> SurveyBOList = null)
        {
            List<SurveyResponseBO> result = _surveyResponseDao.GetSurveyResponse(Criteria.SurveyAnswerIdList, Criteria.UserPublishKey);
            return result;
        }

        public SurveyResponseBO GetSurveyResponseStateById(SurveyAnswerCriteria Criteria)
        {
            string  responseId = Criteria.SurveyAnswerIdList[0];
            SurveyResponseBO result = _surveyResponseDao.GetSurveyResponseState(responseId);
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(string FormId, int PageNumber, bool IsMobile)
        {
            List<SurveyResponseBO> result = null;

            int PageSize;
            if (IsMobile)
            {
                PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
            }
            result = _surveyResponseDao.GetFormResponseByFormId(FormId, PageNumber, PageSize);
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseListById(SurveyAnswerCriteria criteria)
        {
            List<SurveyResponseBO> result = null;

            //int PageSize;
            if (criteria.IsMobile)
            {
                criteria.GridPageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                criteria.GridPageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
            }
            result = _surveyResponseDao.GetFormResponseByFormId(criteria);
            return result;
        }
        public int GetNumberOfPages(string FormId, bool IsMobile)
        {
            int PageSize;
            if (IsMobile)
            {
                PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
            }
            int result = _surveyResponseDao.GetFormResponseCount(FormId);
            if (PageSize > 0)
            {
                result = (result + PageSize - 1) / PageSize;
            }
            return result;
        }

        public int GetNumberOfPages(SurveyAnswerCriteria Criteria)
        {
            //int PageSize;
            if (Criteria.IsMobile)
            {
                Criteria.GridPageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                Criteria.GridPageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
            }
            int result = _surveyResponseDao.GetFormResponseCount(Criteria);
            if (Criteria.GridPageSize > 0)
            {
                result = (result + Criteria.GridPageSize - 1) / Criteria.GridPageSize;
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

        public SurveyResponseBO InsertSurveyResponse(SurveyResponseBO pValue)
        {
            SurveyResponseBO result = pValue;
            _surveyResponseDao.InsertSurveyResponse(pValue);
            return result;
        }

        public List<SurveyResponseBO> InsertSurveyResponse(List<SurveyResponseBO> pValue, int UserId, bool IsNewRecord = false)
        {
            throw new NotImplementedException("InsertSurveyResponse");

            //foreach (var item in pValue)
            //{
            //    ResponseBO ResponseXmlBO = new ResponseBO();
            //    ResponseXmlBO.User = UserId;
            //    ResponseXmlBO.ResponseId = item.ResponseId;
            //    ResponseXmlBO.Xml = item.XML;
            //    ResponseXmlBO.IsNewRecord = IsNewRecord;
            //    _surveyResponseDao.InsertResponse(ResponseXmlBO);

            //}

            //return pValue;
        }

        public SurveyResponseBO InsertChildSurveyResponse(SurveyResponseBO pValue, SurveyInfoBO ParentSurveyInfo, string RelateParentId)
        {

            SurveyResponseBO result = pValue;
            pValue.ParentId = ParentSurveyInfo.ParentId;
            pValue.RelateParentId = RelateParentId;
            _surveyResponseDao.InsertChildSurveyResponse(pValue);
            return result;
        }

        public SurveyResponseBO UpdateSurveyResponse(SurveyResponseBO pValue)
        {
            SurveyResponseBO result = pValue;
            //Check if this respose has prent
            string ParentId = _surveyResponseDao.GetResponseParentId(pValue.ResponseId);
            Guid ParentIdGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(ParentId))
            {
                ParentIdGuid = new Guid(ParentId);
            }

            //if ( pValue.Status == 2 && ParentIdGuid!= Guid.Empty )
            //{
            //if (!string.IsNullOrEmpty(ParentId) && ParentId != Guid.Empty.ToString() && pValue.Status == 2)
            //    {
            //    //read the child 

            //    SurveyResponseBO Child = SurveyResponseDao.GetSingleResponse(pValue.ResponseId);
            //    // read the parent
            //    SurveyResponseBO Parent = SurveyResponseDao.GetSingleResponse(ParentId);
            //    //copy and update
            //    Parent.XML = Child.XML;
            //    SurveyResponseDao.UpdateSurveyResponse(Parent);
            //    result = Parent;
            //    //Check if this child has a related form (subchild)
            //    List<SurveyResponseBO> Children = GetResponsesHierarchyIdsByRootId(Child.ResponseId);
            //    if (Children.Count() > 1)
            //    {
            //        SurveyResponseBO NewChild = Children[1];
            //        NewChild.RelateParentId = Parent.ResponseId;
            //        SurveyResponseDao.UpdateSurveyResponse(NewChild);
            //    }
            //    // Set  child recod UserId
            //    Child.UserId = pValue.UserId;
            //    // delete the child
            //    DeleteSingleSurveyResponse(Child);

            //}
            //else
            //{
            //Check if the record existes.If it does update otherwise insert new 
            _surveyResponseDao.UpdateSurveyResponse(pValue);

            SurveyResponseBO SurveyResponseBO = _surveyResponseDao.GetResponse(pValue.ResponseId);

            //  }
            return result;
        }

        public List<SurveyResponseBO> UpdateSurveyResponse(List<SurveyResponseBO> surveyResponseBOs, int Status)
        {
            List<SurveyResponseBO> result = surveyResponseBOs;
            //Check if this respose has parent
            foreach (var surveyResponseBO in surveyResponseBOs)
            {
                //string ParentId = SurveyResponseDao.GetResponseParentId(Obj.ResponseId);
                //if (!string.IsNullOrEmpty(ParentId) && ParentId != Guid.Empty.ToString() && Status == 2)
                //{
                //    //read the child 

                //    SurveyResponseBO Child = SurveyResponseDao.GetSingleResponse(Obj.ResponseId);
                //    // read the parent
                //    SurveyResponseBO Parent = SurveyResponseDao.GetSingleResponse(ParentId);
                //    //copy and update
                //    Parent.XML = Child.XML;
                //    Parent.Status = Status;
                //    SurveyResponseDao.UpdateSurveyResponse(Parent);
                //    result.Add(Parent);
                //    // Set  child recod UserId
                //    Child.UserId = Obj.UserId;
                //    // delete the child
                //    DeleteSurveyResponse(Child);

                //}
                //else
                //{
                surveyResponseBO.Status = Status;
                _surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);
                // }
            }
            return result;
        }

        public void UpdateFormResponse(SurveyResponseBO surveyResponseBO)
        {

            _surveyResponseDao.UpdateSurveyResponse(surveyResponseBO);
        }

        public bool DeleteSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            bool result = false;

            _surveyResponseDao.DeleteSurveyResponse(surveyResponseBO);
            result = true;

            return result;
        }

        public bool DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponseBO, int Status = -1)
        {
            bool result = false;
            List<SurveyResponseBO> Children = GetResponsesHierarchyIdsByRootId(surveyResponseBO.ResponseId);

            foreach (var child in Children)
            {
                //Get the original copy of the response
                SurveyResponseBO response = _surveyResponseDao.GetResponse(child.ResponseId);
                if (!response.IsNewRecord)
                {
                    child.XML = response.XML;
                    _surveyResponseDao.UpdateSurveyResponse(child);
                }
                else
                {
                    child.UserId = surveyResponseBO.UserId;
                    _surveyResponseDao.DeleteSurveyResponse(child);

                }
                // delete record from ResponseXml Table

                ResponseBO ResponseBO = new ResponseBO();
                ResponseBO.ResponseId = child.ResponseId;
                _surveyResponseDao.DeleteResponse(ResponseBO);
                if (Status > -1)
                {
                    _surveyResponseDao.UpdateRecordStatus(ResponseBO.ResponseId, Status, RecordStatusChangeReason.DeleteInEditMode);
                }
            }

            result = true;

            return result;
        }

        public bool DeleteSingleSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            bool result = false;

            _surveyResponseDao.DeleteSingleSurveyResponse(surveyResponseBO);
            result = true;

            return result;
        }

        public PageInfoBO GetResponseSurveySize(List<string> SurveyResponseIdList, string SurveyId, DateTime pClosingDate, int BandwidthUsageFactor, bool IsDraftMode = false, int pSurveyType = -1, int pPageNumber = -1, int pPageSize = -1, int pResponseMaxSize = -1)
        {
            List<SurveyResponseBO> SurveyResponseBOList = _surveyResponseDao.GetSurveyResponseSize(SurveyResponseIdList, SurveyId, pClosingDate, IsDraftMode, pSurveyType, pPageNumber, pPageSize, pResponseMaxSize);
            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(SurveyResponseBOList, BandwidthUsageFactor, pResponseMaxSize);
            return result;
        }

        public PageInfoBO GetSurveyResponseBySurveyIdSize(List<string> SurveyIdList, Guid UserPublishKey, int BandwidthUsageFactor, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {
            List<SurveyResponseBO> SurveyResponseBOList = _surveyResponseDao.GetSurveyResponseBySurveyIdSize(SurveyIdList, UserPublishKey, PageNumber, PageSize, ResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(SurveyResponseBOList, BandwidthUsageFactor, ResponseMaxSize);
            return result;

        }
        public PageInfoBO GetSurveyResponseSize(List<string> SurveyResponseIdList, Guid UserPublishKey, int BandwidthUsageFactor, int PageNumber = -1, int PageSize = -1, int ResponseMaxSize = -1)
        {

            List<SurveyResponseBO> SurveyResponseBOList = _surveyResponseDao.GetSurveyResponseSize(SurveyResponseIdList, UserPublishKey, PageNumber, PageSize, ResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(SurveyResponseBOList, BandwidthUsageFactor, ResponseMaxSize);
            return result;
        }
        public int GetNumberOfResponses(string FormId)
        {

            int result = _surveyResponseDao.GetFormResponseCount(FormId);

            return result;
        }

        public int GetNumberOfResponses(SurveyAnswerCriteria Criteria)
        {

            int result = _surveyResponseDao.GetFormResponseCount(Criteria);

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

        public SurveyResponseBO GetResponseXml(string ResponseId)
        {
            SurveyResponseBO SurveyResponseBO = new SurveyResponseBO();

            SurveyResponseBO = _surveyResponseDao.GetResponse(ResponseId);

            return SurveyResponseBO;
        }

        public void DeleteResponseXml(ResponseBO ResponseXmlBO)
        {

            _surveyResponseDao.DeleteResponse(ResponseXmlBO);
        }
        public void UpdateRecordStatus(string ResponseId, int StatusId, RecordStatusChangeReason reasonForStatusChange)
        {

            _surveyResponseDao.UpdateRecordStatus(ResponseId, StatusId, reasonForStatusChange);
        }
        public PreFilledAnswerResponse SetSurveyAnswer(PreFilledAnswerRequest request)
        {
            throw new NotImplementedException("SetSurveyAnswer");

            //string SurveyId = request.AnswerInfo.SurveyId.ToString();
            //string ResponseId = request.AnswerInfo.ResponseId.ToString();
            //Guid ParentRecordId = request.AnswerInfo.ParentRecordId;
            //Dictionary<string, string> ErrorMessageList = new Dictionary<string, string>();
            //PreFilledAnswerResponse response;

            //SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
            //UserAuthenticationRequestBO UserAuthenticationRequestBO = new UserAuthenticationRequestBO();
            ////Get Survey Info (MetaData)
            //List<SurveyInfoBO> SurveyBOList = GetSurveyInfo(request);
            ////Build Survey Response Xml

            //string xmlResponse;
            //FormResponseDetail formResponseDetail = CreateResponseXml(request, SurveyBOList, out xmlResponse);
            ////Validate Response values

            //ErrorMessageList = ValidateResponse(SurveyBOList, request);

            //if (ErrorMessageList.Count() > 0)
            //{
            //    response = new PreFilledAnswerResponse();
            //    response.ErrorMessageList = ErrorMessageList;
            //    response.Status = ((Message)1).ToString();
            //}
            //else
            //{
            //    //Insert Survey Response
            //    surveyResponseBO = _surveyResponseDao.GetSingleResponse(request.AnswerInfo.ResponseId.ToString());
            //    if (surveyResponseBO.SurveyId == null)
            //    {
            //        surveyResponseBO = InsertSurveyResponse(Mapper.ToBusinessObject(request.AnswerInfo.SurveyId.ToString(), request.AnswerInfo.ParentRecordId.ToString(), request.AnswerInfo.ResponseId.ToString(), request.AnswerInfo.UserId, formResponseDetail, xmlResponse));
            //        response = new PreFilledAnswerResponse();
            //        response.Status = ((Message)2).ToString();
            //    }
            //    else
            //    {
            //        UpdateFormResponse(Mapper.ToBusinessObject(request.AnswerInfo.SurveyId.ToString(), request.AnswerInfo.ParentRecordId.ToString(), request.AnswerInfo.ResponseId.ToString(), request.AnswerInfo.UserId, formResponseDetail, xmlResponse));
            //        response = new PreFilledAnswerResponse();
            //        response.Status = ((Message)2).ToString();
            //    }
            //}


            //return response;
        }

        private Dictionary<string, string> ValidateResponse(List<SurveyInfoBO> SurveyBOList, PreFilledAnswerRequest request)
        {
            throw new NotImplementedException("ValidateResponse");

            //XDocument SurveyXml = new XDocument(); 
            //foreach (var item in SurveyBOList)
            //{
            //    SurveyXml = XDocument.Parse(item.XML);
            //}
            //Dictionary<string, string> MessageList = new Dictionary<string, string>();
            //Dictionary<string, string> FieldNotFoundList = new Dictionary<string, string>();
            //Dictionary<string, string> WrongFieldTypeList = new Dictionary<string, string>();
            //SurveyResponseXML Implementation = new SurveyResponseXML(request, SurveyXml);
            //FieldNotFoundList = Implementation.ValidateResponseFields();
            //MessageList = MessageList.Union(FieldNotFoundList).Union(WrongFieldTypeList).ToDictionary(k => k.Key, v => v.Value);
            //return MessageList;
        }

        private List<SurveyInfoBO> GetSurveyInfo(PreFilledAnswerRequest request)
        {

            List<string> SurveyIdList = new List<string>();
            string SurveyId = request.AnswerInfo.SurveyId.ToString();
            string OrganizationId = request.AnswerInfo.OrganizationKey.ToString();
            //Guid UserPublishKey = request.AnswerInfo.UserPublishKey;
            List<SurveyInfoBO> SurveyBOList = new List<SurveyInfoBO>();

            SurveyIdList.Add(SurveyId);

            SurveyInfoRequest pRequest = new SurveyInfoRequest();
            var criteria = pRequest.Criteria as Epi.Web.Enter.Common.Criteria.SurveyInfoCriteria;

            var entityDaoFactory = new EF.EntityDaoFactory();
            var surveyInfoDao = entityDaoFactory.SurveyInfoDao;
            SurveyInfo implementation = new SurveyInfo(surveyInfoDao);

            SurveyBOList = implementation.GetSurveyInfo(SurveyIdList, criteria.ClosingDate, OrganizationId, criteria.SurveyType, criteria.PageNumber, criteria.PageSize);//Default 

            return SurveyBOList;
        }

        public bool HasResponse(string SurveyId, string ResponseId)
        {
            SurveyAnswerCriteria SurveyAnswerCriteria = new SurveyAnswerCriteria();
            SurveyAnswerCriteria.SurveyId = SurveyId;
            SurveyAnswerCriteria.SurveyAnswerIdList = new List<string>();
            SurveyAnswerCriteria.SurveyAnswerIdList.Add(ResponseId);

            return _surveyResponseDao.HasResponse(SurveyAnswerCriteria);
        }

        public void UpdateRecordStatus(SurveyResponseBO SurveyResponseBO)
        {
            if (SurveyResponseBO.Status == RecordStatus.InProcess)
            {
                SurveyResponseBO.Status = RecordStatus.Saved;
            }

            _surveyResponseDao.UpdateRecordStatus(SurveyResponseBO);
        }
    }
}
