using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MetadataServices.Common.Extensions;
using Epi.Cloud.MVC.Extensions;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.Common.Exceptions;
using Epi.DataPersistence.Common.BusinessObjects;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.DataEntryServices
{
    public class DataEntryService : IDataEntryService
    {
        private readonly ISecurityDataService _securityDataService;
        private readonly ISurveyInfoService _surveyInfoService;
        private readonly IFormInfoDao _formInfoDao;
        private readonly ISurveyInfoDao _surveyInfoDao;

        private readonly ISurveyResponseDao _surveyResponseDao;
        private readonly SurveyResponseProvider _surveyResponseProvider;

        public DataEntryService(
            ISurveyInfoService surveyInfoService,
            ISecurityDataService securityDataService,
            IFormInfoDao formInfoDao,
            ISurveyInfoDao surveyInfoDao,
            SurveyResponseProvider surveyResponseProvider,
            ISurveyResponseDao surveyResponseDao)
        {
            _surveyInfoService = surveyInfoService;
            _securityDataService = securityDataService;
            _formInfoDao = formInfoDao;
            _surveyInfoDao = surveyInfoDao;
            _surveyResponseDao = surveyResponseDao;
            _surveyResponseProvider = surveyResponseProvider;
        }

        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO)
        {
            // TODO: Implement this correctly
            var userAuthenticationResponseBO = new UserAuthenticationResponseBO { PassCode = passcodeBO.PassCode, ResponseId = passcodeBO.ResponseId };
            return userAuthenticationResponseBO;

#if false // from WebEnter
			UserAuthenticationResponseBO UserAuthenticationResponseBO = Mapper.ToAuthenticationResponseBO(UserAuthenticationRequestBO);
            try
            {
                Guid Id = new Guid(UserAuthenticationRequestBO.ResponseId);

                using (var Context = DataObjectFactory.CreateContext())
                {
                    SurveyResponse surveyResponse = Context.SurveyResponses.First(x => x.ResponseId == Id);
                    if (surveyResponse != null)
                    {
                        UserAuthenticationResponseBO.PassCode = surveyResponse.ResponsePasscode;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return UserAuthenticationResponseBO;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest request)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(request.RequestId);
                Epi.Cloud.DataEntryServices.SurveyResponseProvider surveyResponseProvider = new SurveyResponseProvider(_surveyResponseDao);

                var responseContext = request.ResponseContext;
                var criteria = request.Criteria as SurveyAnswerCriteria;
                List<SurveyResponseBO> surveyResponseList = surveyResponseProvider.GetSurveyResponseById(responseContext, request.Criteria);
                result.SurveyResponseList = surveyResponseList.ToSurveyAnswerDTOList();
                SurveyInfoBO surveyInfoBO = _surveyInfoService.GetSurveyInfoByFormId(request.FormId ?? request.RootFormId);
                result.FormInfo = surveyInfoBO.ToFormInfoDTO();

                return result;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest request)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse();

                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                var responseContext = request.ResponseContext;
                SurveyResponseBO surveyResponseBO = surveyResponseImplementation.GetSurveyResponseStateById(responseContext);
                SurveyAnswerDTO surveyAnswerDTO = surveyResponseBO != null ? surveyResponseBO.ToSurveyAnswerDTO() : null;
                result.SurveyResponseList = new List<SurveyAnswerDTO>();
                if (surveyAnswerDTO != null) result.SurveyResponseList.Add(surveyAnswerDTO);
                return result;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse response = new SurveyAnswerResponse(surveyAnswerRequest.RequestId);

            var responseContext = surveyAnswerRequest.ResponseContext;

            // Transform SurveyResponse data transfer object to SurveyResponse business object
            SurveyResponseBO surveyResponseBO = surveyAnswerRequest.SurveyAnswerList[0].ToSurveyResponseBO();

            surveyResponseBO.IsNewRecord = surveyAnswerRequest.IsNewRecord;

            if (surveyAnswerRequest.Criteria.UserOrganizationId > 0)
            {
                surveyResponseBO.UserOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;
                surveyResponseBO.CurrentOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;
            }

            if (surveyAnswerRequest.Criteria.UserId > 0) surveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
            if (!string.IsNullOrWhiteSpace(surveyAnswerRequest.Criteria.UserName)) surveyResponseBO.UserName = surveyAnswerRequest.Criteria.UserName;
            

            // Validate SurveyResponse business rules

            if (!surveyAnswerRequest.Action.Equals(RequestAction.Delete, StringComparison.OrdinalIgnoreCase))
            {
                //if (!SurveyResponse.Validate())
                //{

                //    foreach (string error in SurveyResponse.ValidationErrors)
                //        response.Message += error + Environment.NewLine;

                //    return response;
                //}
            }

            if (surveyAnswerRequest.Action.Equals(RequestAction.Create, StringComparison.OrdinalIgnoreCase))
            {
                _surveyResponseProvider.InsertSurveyResponse(surveyResponseBO);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.CreateMulti, StringComparison.OrdinalIgnoreCase))
            {
                if (surveyAnswerRequest.SurveyAnswerList[0].ParentResponseId != null)
                {

                    var parentResponseId = surveyAnswerRequest.SurveyAnswerList[0].ParentResponseId;
                    var viewId = surveyAnswerRequest.SurveyAnswerList[0].ViewId;
                    var metadataAccessor = new MetadataAccessor();
                    var formId = metadataAccessor.GetFormIdByViewId(viewId);
                    var formName = metadataAccessor.GetFormName(formId);
                    responseContext = surveyAnswerRequest.SurveyAnswerList[0].CloneResponseContext();
                    List<SurveyResponseBO> surveyResponseBOList = _surveyResponseProvider.GetResponsesHierarchyIdsByRootId(responseContext); //formId, parentResponseId);

                    //if (!surveyAnswerRequest.SurveyAnswerList[0].RecoverLastRecordVersion)
                    //// if we are not keeping the version of xml found currently in the SurveyResponse table (meaning getting the original copy form the ResponseXml table)
                    //{
                    //    //check if any orphan records exists 
                    //    foreach (var item in _surveyResponseBOList)
                    //    {
                    //        if (item.ResponseId != null)
                    //        {
                    //            SurveyResponseBO SurveyResponseBO = _surveyResponseProvider.GetResponse(item.ResponseId);
                    //            // before we delete the temp version we need to move it the SurveResponse table

                    //            if (!string.IsNullOrEmpty(SurveyResponseBO.ResponseId))
                    //            {
                    //                SurveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
                    //                ResponseBO responseBO = new ResponseBO();
                    //                responseBO.ResponseId = SurveyResponseBO.ResponseId;
                    //                // During the delete process below: 
                    //                //  1) Delete the record from ResponseXml table.
                    //                //  2) Update Record status in the SurveyResponse table which fires database triggers.
                    //                _surveyResponseProvider.DeleteResponse(responseBO);
                    //                _surveyResponseProvider.UpdateRecordStatus(responseBO.ResponseId.ToString(), 2);

                    //                //This will handle the status update and the swapping of the Xml
                    //                // but for this scenario I will keep the status unchanged 
                    //                // Implementation.DeleteSurveyResponseInEditMode(SurveyResponseBO);
                    //            }
                    //        }
                    //    }

                    //    _surveyResponseBOList = _surveyResponseProvider.GetResponsesHierarchyIdsByRootId(surveyAnswerRequest.SurveyAnswerList[0].ParentResponseId);
                    //    // Inserting a temp xml to the ResponseXml table
                    //    response.SurveyResponseList = _surveyResponseProvider.InsertSurveyResponse(_surveyResponseBOList, surveyAnswerRequest.Criteria.UserId).ToSurveyAnswerDTOList();
                    //}
                    //else
                    {
                        // load the version curently found the SurveyResponse table 

                        response.SurveyResponseList = surveyResponseBOList.ToSurveyAnswerDTOList();
                    }
                }
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.Update, StringComparison.OrdinalIgnoreCase))
            {
                _surveyResponseProvider.UpdateSurveyResponse(surveyResponseBO);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());
            }

            else if (surveyAnswerRequest.Action.Equals(RequestAction.CreateChild, StringComparison.OrdinalIgnoreCase))
            {
                SurveyInfoBO surveyInfoBO = _surveyInfoService.GetParentInfoByChildFormId(surveyResponseBO.FormId);

                _surveyResponseProvider.InsertChildSurveyResponse(surveyResponseBO, surveyInfoBO, surveyAnswerRequest.SurveyAnswerList[0].ParentResponseId);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());

                List<SurveyResponseBO> List = new List<SurveyResponseBO>();
                List.Add(surveyResponseBO);
                _surveyResponseProvider.InsertSurveyResponse(List, surveyAnswerRequest.Criteria.UserId, true);
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.UpdateMulti, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException(RequestAction.UpdateMulti);
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.Delete, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DeleteResponse: " + ex.ToString());
                }
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.DontSave, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item, RecordStatus.RecoverLastRecordVersion, RecordStatusChangeReason.DontSave);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("DontSave: " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DeleteResponse: " + ex.ToString());
                }
            }
            else if (surveyAnswerRequest.Action.Equals(RequestAction.DeleteResponse, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item, RecordStatus.Saved, RecordStatusChangeReason.DeleteResponse);
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DeleteResponse: " + ex.ToString());
                }
            }

            return response;
        }


        public SurveyAnswerResponse UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse surveyAnswerResponse = new SurveyAnswerResponse();
            try
            {
                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                var responseContext = surveyAnswerRequest.ResponseContext;
                List<SurveyResponseBO> surveyResponseBOList = surveyResponseImplementation.GetSurveyResponseById(responseContext, surveyAnswerRequest.Criteria);
                foreach (var surveyResponseBO in surveyResponseBOList)
                {
                    try
                    {
                        if (surveyAnswerRequest.IsChildResponse)
                        {
                            surveyResponseBO.ActiveChildResponseDetail = surveyResponseBO.ResponseDetail.FindFormResponseDetail(surveyAnswerRequest.ResponseId);
                        }
                        surveyResponseBO.IsNewRecord = surveyAnswerRequest.IsNewRecord;
                        surveyResponseBO.UserOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;
                        surveyResponseBO.CurrentOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;
                        surveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
                        surveyResponseBO.UserName = surveyAnswerRequest.Criteria.UserName;
                        surveyResponseBO.LastSaveLogonName = surveyAnswerRequest.Criteria.UserName;
                        surveyResponseBO.LastSaveTime = DateTime.UtcNow;
                        surveyResponseBO.RecStatus = surveyAnswerRequest.Criteria.StatusId;
                        surveyResponseBO.ReasonForStatusChange = surveyAnswerRequest.Criteria.StatusChangeReason;
                    }
                    finally
                    {
                        surveyResponseBO.ActiveChildResponseDetail = null;
                    }
                }

                List<SurveyResponseBO> resultList = surveyResponseImplementation.UpdateSurveyResponse(surveyResponseBOList, surveyAnswerRequest.Criteria.StatusId, surveyAnswerRequest.Criteria.StatusChangeReason);
                surveyAnswerResponse.SurveyResponseList = resultList.Select(bo => bo.ToSurveyAnswerDTO()).ToList();
                surveyAnswerResponse.NumberOfResponses = surveyAnswerResponse.SurveyResponseList.Count();
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
            return surveyAnswerResponse;
        }

        public SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(surveyAnswerRequest.RequestId);

                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);
                foreach (var response in surveyAnswerRequest.SurveyAnswerList)
                {
                    if (surveyAnswerRequest.Criteria.IsEditMode)
                    {
                        surveyResponseImplementation.DeleteSurveyResponseInEditMode(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId), RecordStatus.RecoverLastRecordVersion);
                    }
                    else
                    {

                        if (surveyAnswerRequest.Action != null && surveyAnswerRequest.Action.Equals(RequestAction.DontSave, StringComparison.OrdinalIgnoreCase))
                        {
                            surveyResponseImplementation.DeleteSurveyResponseInEditMode(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId), RecordStatus.RecoverLastRecordVersion);
                        }
                        else
                        {
                            surveyResponseImplementation.DeleteSurveyResponse(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId));
                        }                       
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
            }
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(surveyAnswerRequest.RequestId);

                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                SurveyAnswerCriteria criteria = surveyAnswerRequest.Criteria;
                criteria.GridPageSize = AppSettings.GetIntValue(criteria.IsMobile ? AppSettings.Key.MobileResponsePageSize : AppSettings.Key.ResponsePageSize);

                ResponseGridQueryResultBO responseGridQueryResultBO = surveyResponseImplementation.GetFormResponseListByFormId(surveyAnswerRequest.ResponseContext, criteria);
                //Query The number of records
                result.NumberOfResponses = responseGridQueryResultBO.NumberOfResponsesReturnedByQuery;
                result.NumberOfPages = responseGridQueryResultBO.NumberOfPages;
                result.NumberOfResponsesPerPage = responseGridQueryResultBO.NumberOfResponsesPerPage;
                result.QuerySetToken = responseGridQueryResultBO.QuerySetToken;
                
                var surveyResponseList = responseGridQueryResultBO.SurveyResponseBOList;
                result.SurveyResponseList = surveyResponseList.ToSurveyAnswerDTOList();

                surveyAnswerRequest.Criteria.FormResponseCount = result.NumberOfResponses;

                //Query The number of records
                //result.NumberOfPages = surveyResponseImplementation.GetNumberOfPages(surveyAnswerRequest.Criteria);

                //Get form info 
                Epi.Cloud.BLL.FormInfo formInfoImplementation = new Epi.Cloud.BLL.FormInfo(_formInfoDao);
                var formInfoBO = formInfoImplementation.GetFormInfoByFormId(surveyAnswerRequest.Criteria.SurveyId, surveyAnswerRequest.Criteria.UserId);
                result.FormInfo = formInfoBO.ToFormInfoDTO();

                return result;
            }
            catch (Exception ex)
            {
                CustomFaultException customFaultException = new CustomFaultException();
                customFaultException.CustomMessage = ex.Message;
                customFaultException.Source = ex.Source;
                customFaultException.StackTrace = ex.StackTrace;
                customFaultException.HelpLink = ex.HelpLink;
                return null;
            }
        }

        public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest)
        {
            try
            {
                var rootFormId = formsHierarchyRequest.SurveyInfo.FormId;
                var metadatAccessor = new MetadataAccessor();
                var viewId = metadatAccessor.GetFormDigest(rootFormId).ViewId;
                var formIdHierarchy = metadatAccessor.GetFormIdHierarchyByRootFormId(rootFormId);
                var formDigests = metadatAccessor.FormDigests.Where(fd => formIdHierarchy.Contains(fd.FormId)).ToArray();
                var formInfoDTO = formsHierarchyRequest.SurveyInfo;
                var surveyInfoBO = formInfoDTO.ToSurveyInfoBO(viewId);
                var formsHierarchyBOList = formDigests.ToFormsHierarchyBOList(surveyInfoBO);

                FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();

                List<SurveyResponseBO> allResponsesIDsList = new List<SurveyResponseBO>();

                //1- Get All form  ID's
                List<FormsHierarchyBO> relatedFormIDsList = _surveyInfoService.GetFormsHierarchyIdsByRootFormId(rootFormId);

                //2- Get all Responses ID's
                Epi.Cloud.DataEntryServices.SurveyResponseProvider surveyResponseProvider = new SurveyResponseProvider(_surveyResponseDao);
                if (!string.IsNullOrEmpty(formsHierarchyRequest.SurveyResponseInfo.ResponseId))
                {
                    IResponseContext responseContext = formsHierarchyRequest.SurveyResponseInfo.ResponseContext.ResolveMetadataDependencies();
                    allResponsesIDsList = surveyResponseProvider.GetResponsesHierarchyIdsByRootId(responseContext);
                }
                else
                {
                    allResponsesIDsList = null;
                }

                //3 Combining the lists.
                List<FormsHierarchyBO> combinedList = CombineLists(relatedFormIDsList, allResponsesIDsList);
                formsHierarchyResponse.FormsHierarchy = combinedList.ToFormsHierarchyDTOList();

                return formsHierarchyResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<FormsHierarchyBO> CombineLists(List<FormsHierarchyBO> relatedFormIDsList, List<SurveyResponseBO> allResponsesIDsList)
        {
            List<FormsHierarchyBO> List = new List<FormsHierarchyBO>();

            foreach (var item in relatedFormIDsList)
            {
                FormsHierarchyBO formsHierarchyBO = new FormsHierarchyBO();
                formsHierarchyBO.RootFormId = item.RootFormId;
                formsHierarchyBO.FormId = item.FormId;
                formsHierarchyBO.ViewId = item.ViewId;
                formsHierarchyBO.IsSqlProject = item.IsSqlProject;
                formsHierarchyBO.IsRoot = item.IsRoot;
                formsHierarchyBO.SurveyInfo = item.SurveyInfo;
                if (allResponsesIDsList != null)
                {
                    formsHierarchyBO.ResponseIds = allResponsesIDsList.Where(x => x.FormId == item.FormId).ToList();
                }
                List.Add(formsHierarchyBO);
            }
            return List;
        }

        public FormsInfoResponse GetFormsInfo(FormsInfoRequest formsInfoRequest)
        {
            FormsInfoResponse result = new FormsInfoResponse();
            Epi.Cloud.BLL.FormInfo implementation = new Epi.Cloud.BLL.FormInfo(_formInfoDao);
            try
            {
                List<FormInfoBO> FormInfoBOList = implementation.GetFormsInfo(formsInfoRequest.Criteria.UserId, formsInfoRequest.Criteria.CurrentOrgId);

                foreach (FormInfoBO item in FormInfoBOList)
                {
                    result.FormInfoList.Add(item.ToFormInfoDTO());
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;


        }

        public SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest surveyInfoRequest)
        {
            var criteria = surveyInfoRequest.Criteria as SurveyInfoCriteria;
            List<string> SurveyIdList = new List<string>();
            foreach (string id in criteria.SurveyIdList)
            {
                SurveyIdList.Add(id.ToUpper());
            }

            var surveyId = SurveyIdList.FirstOrDefault();
            var surveyInfoBO = _surveyInfoService.GetSurveyInfoByFormId(surveyId);
            var result = new SurveyInfoResponse();
            result.SurveyInfoList.Add(surveyInfoBO.ToSurveyInfoDTO());
            return result;
        }

        public bool HasResponse(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                var hasResponse = _surveyResponseProvider.HasResponse(surveyAnswerRequest.ResponseContext);
                return hasResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
