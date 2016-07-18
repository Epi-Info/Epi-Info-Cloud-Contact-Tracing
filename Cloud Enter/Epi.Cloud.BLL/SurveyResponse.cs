using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Web.Enter.Common.BusinessObject;
using System.Configuration;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.ObjectMapping;
using System.Xml.Linq;
using Epi.Web.Enter.Common.Xml;
using Epi.Web.Enter.Common.DTO;

using ResponseBO = Epi.Web.Enter.Common.BusinessObject.ResponseBO;

namespace Epi.Cloud.BLL
{
    public class SurveyResponse
    {
        public enum Message
        {
            Failed = 1,
            Success = 2,

        }
        private Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao _surveyResponseDao;
        private readonly Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao _eweSurveyResponseDao;

        public SurveyResponse(Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyResponseDao,
                              Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao eweSurveyResponseDao)
        {
            _surveyResponseDao = surveyResponseDao;
            _eweSurveyResponseDao = eweSurveyResponseDao;
        }


        public List<SurveyResponseBO> GetSurveyResponseById(SurveyAnswerCriteria Criteria, List<SurveyInfoBO> SurveyBOList = null)
        {

            //Check if this Response exists in DocumentDB
            Guid Id = new Guid(Criteria.SurveyAnswerIdList[0]);
            bool ResponseExists = _surveyResponseDao.DoesResponseExist(Id);
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            if (ResponseExists)
            {
                result = _surveyResponseDao.GetSurveyResponse(Criteria.SurveyAnswerIdList, Criteria.UserPublishKey);

            }
            else
            {

                //Get Form Name
                // string 
                //Retrieve response data sets from Epi 7 DataBase
                SurveyAnswerCriteria SurveyAnswerCriteria = new SurveyAnswerCriteria();
                SurveyAnswerCriteria.GetAllColumns = true;
                SurveyAnswerCriteria.SurveyId = Criteria.SurveyId;
                SurveyAnswerCriteria.SurveyAnswerIdList.Add(Criteria.SurveyAnswerIdList[0]);
                SurveyAnswerCriteria.PageSize = 1;
                SurveyAnswerCriteria.PageNumber = 1;
                SurveyAnswerCriteria.IsSqlProject = Criteria.IsSqlProject;
                result = _surveyResponseDao.GetFormResponseByFormId(SurveyAnswerCriteria);
                if (result[0].SqlData != null)
                {
                    var DataList = result[0].SqlData.ToList();
                    DataList.RemoveAt(0);

                    //Build Response Xml
                    PreFilledAnswerRequest Request = new PreFilledAnswerRequest();
                    Request.AnswerInfo.ResponseId = new Guid(Criteria.SurveyAnswerIdList[0]);
                    Request.AnswerInfo.SurveyId = new Guid(Criteria.SurveyId);
                    Request.AnswerInfo.UserId = Criteria.UserId;
                    Request.AnswerInfo.SurveyQuestionAnswerList = new Dictionary<string, string>();
                    foreach (var item in DataList)
                    {


                        Request.AnswerInfo.SurveyQuestionAnswerList.Add(item.Key, item.Value);

                    }
                    //  Request.AnswerInfo.OrganizationKey = new Guid ( "a4b6a687-610d-442a-a80c-d1c781087181");
                    var response = SetSurveyAnswer(Request);
                }
                // string Xml = CreateResponseXml(  Request,  SurveyBOList);

                //Insert response xml into EWE

                result = _surveyResponseDao.GetSurveyResponse(Criteria.SurveyAnswerIdList, Criteria.UserPublishKey);
            }
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
                criteria.PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                criteria.PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
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
            int result = _eweSurveyResponseDao.GetFormResponseCount(FormId);
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
                Criteria.PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE_Mobile"]);
            }
            else
            {
                Criteria.PageSize = Int32.Parse(ConfigurationManager.AppSettings["RESPONSE_PAGE_SIZE"]);
            }
            int result = _eweSurveyResponseDao.GetFormResponseCount(Criteria);
            if (Criteria.PageSize > 0)
            {
                result = (result + Criteria.PageSize - 1) / Criteria.PageSize;
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
            _eweSurveyResponseDao.UpdatePassCode(pValue);



        }
        // Get Authentication Response
        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO pValue)
        {
            UserAuthenticationResponseBO result = _eweSurveyResponseDao.GetAuthenticationResponse(pValue);

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

            foreach (var item in pValue)
            {
                ResponseBO ResponseXmlBO = new ResponseBO();
                ResponseXmlBO.User = UserId;
                ResponseXmlBO.ResponseId = item.ResponseId;
                ResponseXmlBO.Xml = item.XML;
                ResponseXmlBO.IsNewRecord = IsNewRecord;
                _eweSurveyResponseDao.InsertResponse(ResponseXmlBO);

            }

            return pValue;
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
            throw new NotImplementedException("SurveyResponse.UpdateSurveyResponse is not implemented");
#if NotImplemented
            SurveyResponseBO result = pValue;
            //Check if this respose has prent
            string ParentId = _eweSurveyResponseDao.GetResponseParentId(pValue.ResponseId);
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

            SurveyResponseBO SurveyResponseBO = _surveyResponseDao.GetResponseXml(pValue.ResponseId);



            //  }
            return result;
#endif
        }
        public List<SurveyResponseBO> UpdateSurveyResponse(List<SurveyResponseBO> pValue, int Status)
        {
            List<SurveyResponseBO> result = pValue;
            //Check if this respose has prent
            foreach (var Obj in pValue.ToList())
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
                Obj.Status = Status;
                _surveyResponseDao.UpdateSurveyResponse(Obj);
                // }
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

            _surveyResponseDao.DeleteSurveyResponse(pValue);
            result = true;

            return result;
        }
        public bool DeleteSurveyResponseInEditMode(SurveyResponseBO pValue, int Status = -1)
        {
            throw new NotImplementedException("SurveyResponse.DeleteSurveyResponseInEditMode is not implemented");
#if NotImplemented
            bool result = false;
            List<SurveyResponseBO> Children = GetResponsesHierarchyIdsByRootId(pValue.ResponseId);

            foreach (var child in Children)
            {
                //Get the original copy of the xml
                SurveyResponseBO ResponseXml = _eweSurveyResponseDao.GetResponseXml(child.ResponseId);
                if (!ResponseXml.IsNewRecord)
                {
                    child.XML = ResponseXml.XML;
                    _surveyResponseDao.UpdateSurveyResponse(child);
                }
                else
                {
                    child.UserId = pValue.UserId;
                    _surveyResponseDao.DeleteSurveyResponse(child);

                }
                // delete record from ResponseXml Table

                ResponseXmlBO ResponseXmlBO = new ResponseXmlBO();
                ResponseXmlBO.ResponseId = child.ResponseId;
                _eweSurveyResponseDao.DeleteResponseXml(ResponseXmlBO);
                if (Status > -1)
                {
                    _eweSurveyResponseDao.UpdateRecordStatus(ResponseXmlBO.ResponseId, Status);
                }
            }

            result = true;

            return result;
#endif
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

            int result = _eweSurveyResponseDao.GetFormResponseCount(FormId);

            return result;
        }

        public int GetNumberOfResponses(SurveyAnswerCriteria Criteria)
        {

            int result = _eweSurveyResponseDao.GetFormResponseCount(Criteria);

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
            throw new NotImplementedException("SurveyResponse.GetResponseXml is not implemented");
#if NotImplemented
            SurveyResponseBO SurveyResponseBO = new SurveyResponseBO();

            SurveyResponseBO = _eweSurveyResponseDao.GetResponseXml(ResponseId);

            return SurveyResponseBO;
#endif
        }

        public void DeleteResponseXml(ResponseBO ResponseXmlBO)
        {
            throw new NotImplementedException("SurveyResponse.DeleteResponseXml is not implemented");
#if NotImplemented
            _eweSurveyResponseDao.DeleteResponseXml(ResponseXmlBO);
#endif
        }
        public void UpdateRecordStatus(string ResponseId, int StatusId)
        {

            _eweSurveyResponseDao.UpdateRecordStatus(ResponseId, StatusId);
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
            var criteria = pRequest.Criteria as Epi.Web.Enter.Common.Criteria.SurveyInfoCriteria;

            var entityDaoFactory = new EF.EntityDaoFactory();
            var surveyInfoDao = entityDaoFactory.SurveyInfoDao;
            SurveyInfo implementation = new SurveyInfo(surveyInfoDao);

            SurveyBOList = implementation.GetSurveyInfo(SurveyIdList, criteria.ClosingDate, OrganizationId, criteria.SurveyType, criteria.PageNumber, criteria.PageSize);//Default 

            return SurveyBOList;
#endif
        }
        private string CreateResponseXml(Epi.Web.Enter.Common.Message.PreFilledAnswerRequest request, List<SurveyInfoBO> SurveyBOList)
        {

            string ResponseXml;

            XDocument SurveyXml = new XDocument();

            foreach (var item in SurveyBOList)
            {
                SurveyXml = XDocument.Parse(item.XML);
            }
            SurveyResponseXML Implementation = new SurveyResponseXML(request, SurveyXml);
            ResponseXml = Implementation.CreateResponseDocument(SurveyXml).ToString();


            return ResponseXml;
        }


        public bool HasResponse(string SurveyId, string ResponseId)
        {
            SurveyAnswerCriteria SurveyAnswerCriteria = new SurveyAnswerCriteria();
            SurveyAnswerCriteria.SurveyId = SurveyId;
            SurveyAnswerCriteria.SurveyAnswerIdList = new List<string>();
            SurveyAnswerCriteria.SurveyAnswerIdList.Add(ResponseId);

            return _eweSurveyResponseDao.HasResponse(SurveyAnswerCriteria);
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
