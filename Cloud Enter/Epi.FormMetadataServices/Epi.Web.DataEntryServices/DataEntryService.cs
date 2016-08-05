using System;
using Epi.Cloud.Interfaces.DataInterface;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.MessageBase;
using System.Collections.Generic;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.ObjectMapping;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Exception;
using System.ServiceModel;

namespace Epi.Cloud.DataEntryServices
{
    public class DataEntryService : IDataEntryService
    {

        // Session state variables 
        private string _accessToken;
        //private ShoppingCart _shoppingCart;
        private string _userName;

        public DataEntryService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRequest"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(pRequest.RequestId);
                //Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyInfoDao = new EF.EntitySurveyResponseDao();
                //Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(surveyInfoDao);

                Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new Web.EF.EntityDaoFactory();
                Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = entityDaoFactory.SurveyResponseDao;
                Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(ISurveyResponseDao);


                // Validate client tag, access token, and user credentials
                if (!ValidRequest(pRequest, result, Validate.All))
                {
                    return result;
                }

                var criteria = pRequest.Criteria as SurveyAnswerCriteria;
                string sort = criteria.SortExpression;

                //if (request.LoadOptions.Contains("SurveyInfos"))
                //    {
                //    IEnumerable<SurveyInfoDTO> SurveyInfos;
                //    if (!criteria.IncludeOrderStatistics)
                //        {
                //        SurveyInfos = Implementation.GetSurveyInfos(sort);
                //        }
                //    else
                //        {
                //        SurveyInfos = Implementation.GetSurveyInfosWithOrderStatistics(sort);
                //        }

                //    response.SurveyInfos = SurveyInfos.Select(c => Mapper.ToDataTransferObject(c)).ToList();
                //    }

                //if (pRequest.LoadOptions.Contains("SurveyInfo"))
                //{
                //result.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.GetSurveyResponseById(pRequest.Criteria.SurveyAnswerIdList, pRequest.Criteria.UserPublishKey));
                //}
                Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao SurveyInfoDao = new Web.EF.EntitySurveyInfoDao();
                Epi.Web.BLL.SurveyInfo Implementation1 = new Epi.Web.BLL.SurveyInfo(SurveyInfoDao);
                SurveyInfoBO SurveyInfoBO = Implementation1.GetSurveyInfoById(pRequest.Criteria.SurveyId);
                List<SurveyInfoBO> SurveyInfoBOList = new List<SurveyInfoBO>();
                SurveyInfoBOList.Add(SurveyInfoBO);

                // result.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.GetSurveyResponseById(pRequest.Criteria.SurveyAnswerIdList, pRequest.Criteria.UserPublishKey, pRequest.Criteria.SurveyId, SurveyInfoBOList));

                result.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.GetSurveyResponseById(pRequest.Criteria, SurveyInfoBOList));
                result.FormInfo = Mapper.ToFormInfoDTO(SurveyInfoBO);
                //SurveyResponseBO Request = new SurveyResponseBO();

                //foreach (var item in pRequest.Criteria.SurveyAnswerIdList)
                //    {
                //    Request.ResponseId = item;
                //    Request.UserId = criteria.UserId;

                //    result.SurveyResponseList.Add(Mapper.ToDataTransferObject(Implementation.GetSurveyResponseByUserId(Request)));
                //    }

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

        public SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            return new SurveyAnswerResponse();
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

        /// <summary>
        /// Validate 3 security levels for a request: ClientTag, AccessToken, and User Credentials
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="response">The response message.</param>
        /// <param name="validate">The validation that needs to take place.</param>
        /// <returns></returns>
        private bool ValidRequest(RequestBase request, ResponseBase response, Validate validate)
        {
            bool result = true;

            // Validate Client Tag. 
            // Hardcoded here. In production this should query a 'client' table in a database.
            if ((Validate.ClientTag & validate) == Validate.ClientTag)
            {
                if (request.ClientTag != "ABC123")
                {
                    response.Acknowledge = AcknowledgeType.Failure;
                    response.Message = "Unknown Client Tag";
                    //return false;
                }
            }


            // Validate access token
            if ((Validate.AccessToken & validate) == Validate.AccessToken)
            {
                if (request.AccessToken != _accessToken)
                {
                    response.Acknowledge = AcknowledgeType.Failure;
                    response.Message = "Invalid or expired AccessToken. Call GetToken()";
                    //return false;
                }
            }

            // Validate user credentials
            if ((Validate.UserCredentials & validate) == Validate.UserCredentials)
            {
                if (_userName == null)
                {
                    response.Acknowledge = AcknowledgeType.Failure;
                    response.Message = "Please login and provide user credentials before accessing these methods.";
                    //return false;
                }
            }


            return result;
        }
    }
}

