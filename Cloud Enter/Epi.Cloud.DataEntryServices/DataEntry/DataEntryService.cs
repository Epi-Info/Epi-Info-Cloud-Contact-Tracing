using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MetadataServices.Extensions;
using Epi.Cloud.MVC.Extensions;
using Epi.DataPersistence.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Criteria;
using Epi.Cloud.Common.DTO;
using Epi.Common.Exception;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.MessageBase;

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

        private string _accessToken;
        private string _userName;

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

                var criteria = request.Criteria as SurveyAnswerCriteria;
                string sort = criteria.SortExpression;

                SurveyInfoBO surveyInfoBO = _surveyInfoService.GetSurveyInfoById(request.Criteria.SurveyId);
                List<SurveyInfoBO> surveyInfoBOList = new List<SurveyInfoBO>();
                surveyInfoBOList.Add(surveyInfoBO);

                List<SurveyResponseBO> surveyResponseList = surveyResponseProvider.GetSurveyResponseById(request.Criteria, surveyInfoBOList);
                result.SurveyResponseList = surveyResponseList.ToSurveyAnswerDTOList();
                result.FormInfo = surveyInfoBO.ToFormInfoDTO();

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest request)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(request.RequestId);

                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                SurveyResponseBO surveyResponseBO = surveyResponseImplementation.GetSurveyResponseStateById(request.Criteria);
                SurveyAnswerDTO surveyAnswerDTO = surveyResponseBO != null ? surveyResponseBO.ToSurveyAnswerDTO() : null;
                result.SurveyResponseList = new List<SurveyAnswerDTO>();
                if (surveyAnswerDTO != null) result.SurveyResponseList.Add(surveyAnswerDTO);
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

        public SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse response = new SurveyAnswerResponse(surveyAnswerRequest.RequestId);

            // Transform SurveyResponse data transfer object to SurveyResponse business object
            SurveyResponseBO surveyResponseBO = surveyAnswerRequest.SurveyAnswerList[0].ToSurveyResponseBO();

            surveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
            surveyResponseBO.CurrentOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;

            // Validate SurveyResponse business rules

            if (surveyAnswerRequest.Action != "Delete")
            {
                //if (!SurveyResponse.Validate())
                //{

                //    foreach (string error in SurveyResponse.ValidationErrors)
                //        response.Message += error + Environment.NewLine;

                //    return response;
                //}
            }

            if (surveyAnswerRequest.Action.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                _surveyResponseProvider.InsertSurveyResponse(surveyResponseBO);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());
            }
            else if (surveyAnswerRequest.Action.Equals("CreateMulti", StringComparison.OrdinalIgnoreCase))
            {
                if (surveyAnswerRequest.SurveyAnswerList[0].ParentRecordId != null)
                {

                    List<SurveyResponseBO> _surveyResponseBOList = _surveyResponseProvider.GetResponsesHierarchyIdsByRootId(surveyAnswerRequest.SurveyAnswerList[0].ParentRecordId);

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

                    //    _surveyResponseBOList = _surveyResponseProvider.GetResponsesHierarchyIdsByRootId(surveyAnswerRequest.SurveyAnswerList[0].ParentRecordId);
                    //    // Inserting a temp xml to the ResponseXml table
                    //    response.SurveyResponseList = _surveyResponseProvider.InsertSurveyResponse(_surveyResponseBOList, surveyAnswerRequest.Criteria.UserId).ToSurveyAnswerDTOList();
                    //}
                    //else
                    {
                        // load the version curently found the SurveyResponse table 

                        response.SurveyResponseList = _surveyResponseBOList.ToSurveyAnswerDTOList();
                    }
                }
            }
            else if (surveyAnswerRequest.Action.Equals(Constant.UPDATE, StringComparison.OrdinalIgnoreCase))
            {
                _surveyResponseProvider.UpdateSurveyResponse(surveyResponseBO);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());
            }

            else if (surveyAnswerRequest.Action.Equals(Constant.CREATECHILD, StringComparison.OrdinalIgnoreCase))
            {
                SurveyInfoBO surveyInfoBO = _surveyInfoService.GetParentInfoByChildId(surveyResponseBO.SurveyId);

                _surveyResponseProvider.InsertChildSurveyResponse(surveyResponseBO, surveyInfoBO, surveyAnswerRequest.SurveyAnswerList[0].RelateParentId);
                response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());

                List<SurveyResponseBO> List = new List<SurveyResponseBO>();
                List.Add(surveyResponseBO);
                _surveyResponseProvider.InsertSurveyResponse(List, surveyAnswerRequest.Criteria.UserId, true);
            }
            else if (surveyAnswerRequest.Action.Equals(Constant.UpdateMulti, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException(Constant.UpdateMulti);
            }
            else if (surveyAnswerRequest.Action.Equals(Constant.DELETE, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item.ResponseId, RecordStatus.Deleted, RecordStatusChangeReason.DeleteResponse);
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
            else if (surveyAnswerRequest.Action.Equals("DoNotSaveAction", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item.ResponseId, RecordStatus.Restore, RecordStatusChangeReason.Restore);
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
            else if (surveyAnswerRequest.Action.Equals(Constant.DELETERESPONSE, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
                    {
                        try
                        {
                            _surveyResponseProvider.UpdateRecordStatus(item.ResponseId, RecordStatus.Saved, RecordStatusChangeReason.DeleteResponse);
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


        public void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                SurveyResponseBO surveyResponseBO1 = surveyAnswerRequest.SurveyAnswerList.ToSurveyResponseBOList(surveyAnswerRequest.Criteria.UserId)[0];

                List<SurveyResponseBO> SurveyResponseBOList = surveyResponseImplementation.GetSurveyResponseById(surveyAnswerRequest.Criteria);
                foreach (var surveyResponseBO in SurveyResponseBOList)
                {
                    surveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
                    surveyResponseBO.CurrentOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;
                    surveyResponseBO.Status = surveyAnswerRequest.Criteria.StatusId;
                    surveyResponseBO.ReasonForStatusChange = surveyAnswerRequest.Criteria.StatusChangeReason;
                }

                List<SurveyResponseBO> ResultList = surveyResponseImplementation.UpdateSurveyResponse(SurveyResponseBOList, surveyAnswerRequest.Criteria.StatusId, surveyAnswerRequest.Criteria.StatusChangeReason);
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
                        surveyResponseImplementation.DeleteSurveyResponseInEditMode(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId), RecordStatus.Restore);
                    }
                    else
                    {

                        if (surveyAnswerRequest.Action != null && surveyAnswerRequest.Action.Equals("DoNotSaveAction", StringComparison.OrdinalIgnoreCase))
                        {
                            surveyResponseImplementation.DeleteSurveyResponseInEditMode(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId), RecordStatus.Restore);
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
                CustomFaultException customFaultException = new CustomFaultException();
                customFaultException.CustomMessage = ex.Message;
                customFaultException.Source = ex.Source;
                customFaultException.StackTrace = ex.StackTrace;
                customFaultException.HelpLink = ex.HelpLink;
                throw new FaultException<CustomFaultException>(customFaultException);
            }
        }

        public SurveyAnswerResponse GetAncestorResponseIdsByChildId(SurveyAnswerRequest surveyAnswerRequest)
        {
            throw new NotImplementedException();
            try
            {
#if WebEnterCode
			Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Cloud.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Cloud.BLL.SurveyResponse Implementation = new Epi.Cloud.BLL.SurveyResponse(SurveyResponseDao);
			List<SurveyResponseBO> SurveyResponseBOList = Implementation.GetAncestorResponseIdsByChildId(pRequest.Criteria.SurveyAnswerIdList[0]);
			SurveyAnswerResponse.SurveyResponseList = Mapper.ToDataTransferObject(SurveyResponseBOList);

			return SurveyAnswerResponse;
#endif //WebEnterCode
            }
            catch (Exception ex)
            {
            }
        }

        public FormResponseInfoResponse GetFormResponseInfo(FormResponseInfoRequest formResponseInfoRequest)
        {
            throw new NotImplementedException();
            try
            {
#if WebEnterCode
			FormResponseInfoResponse FormResponseInfoResponse = new FormResponseInfoResponse();
			return FormResponseInfoResponse;
#endif //WebEnterCode
            }
            catch (Exception ex)
            {
            }
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyAnswerResponse result = new SurveyAnswerResponse(surveyAnswerRequest.RequestId);

                SurveyResponseProvider surveyResponseImplementation = new SurveyResponseProvider(_surveyResponseDao);

                SurveyAnswerCriteria criteria = surveyAnswerRequest.Criteria;
                var surveyResponseList = surveyResponseImplementation.GetFormResponseListById(criteria);

                result.SurveyResponseList = surveyResponseList.ToSurveyAnswerDTOList();
                surveyAnswerRequest.Criteria.FormResponseCount = result.SurveyResponseList.Count;
                //Query The number of records

                //result.NumberOfPages = Implementation.GetNumberOfPages(surveyAnswerRequest.Criteria.SurveyId, surveyAnswerRequest.Criteria.IsMobile);
                //result.NumberOfResponses = Implementation.GetNumberOfResponses(surveyAnswerRequest.Criteria.SurveyId);

                result.NumberOfPages = surveyResponseImplementation.GetNumberOfPages(surveyAnswerRequest.Criteria);
                result.NumberOfResponses = surveyResponseImplementation.GetNumberOfResponses(surveyAnswerRequest.Criteria);

                //Get form info 
                Epi.Cloud.BLL.FormInfo formInfoImplementation = new Epi.Cloud.BLL.FormInfo(_formInfoDao);
                var formInfoBO = formInfoImplementation.GetFormInfoByFormId(surveyAnswerRequest.Criteria.SurveyId, false, surveyAnswerRequest.Criteria.UserId);
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
#if WebEnterCode
			try
			{
				SurveyAnswerResponse result = new SurveyAnswerResponse(pRequest.RequestId);


				Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Cloud.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = entityDaoFactory.SurveyResponseDao;
				Epi.Cloud.BLL.SurveyResponse Implementation = new Epi.Cloud.BLL.SurveyResponse(ISurveyResponseDao);

				SurveyAnswerCriteria criteria = pRequest.Criteria;
				//result.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.GetFormResponseListById(pRequest.Criteria.SurveyId, pRequest.Criteria.PageNumber, pRequest.Criteria.IsMobile));
				result.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.GetFormResponseListById(criteria));
				pRequest.Criteria.FormResponseCount = result.SurveyResponseList.Count;
				//Query The number of records

				//result.NumberOfPages = Implementation.GetNumberOfPages(pRequest.Criteria.SurveyId, pRequest.Criteria.IsMobile);
				//result.NumberOfResponses = Implementation.GetNumberOfResponses(pRequest.Criteria.SurveyId);

				result.NumberOfPages = Implementation.GetNumberOfPages(pRequest.Criteria);
				result.NumberOfResponses = Implementation.GetNumberOfResponses(pRequest.Criteria);

				//Get form info 
				Epi.Web.Enter.Interfaces.DataInterface.IFormInfoDao formInfoDao = new EF.EntityFormInfoDao();
				Epi.Cloud.BLL.FormInfo ImplementationFormInfo = new Epi.Cloud.BLL.FormInfo(formInfoDao);
				result.FormInfo = Mapper.ToFormInfoDTO(ImplementationFormInfo.GetFormInfoByFormId(pRequest.Criteria.SurveyId, false, pRequest.Criteria.UserId));

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
#endif //WebEnterCode
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest)
        {
            throw new NotImplementedException();
            try
            {
#if WebEnterCode
			try
			{
				Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();


				IFormInfoDao FormInfoDao = entityDaoFactory.FormInfoDao;
				Epi.Cloud.BLL.FormInfo FormInfoImplementation = new Epi.Cloud.BLL.FormInfo(FormInfoDao);
				FormInfoBO FormInfoBO = FormInfoImplementation.GetFormInfoByFormId(pRequest.FormInfo.FormId, pRequest.GetMetadata, pRequest.FormInfo.UserId);
				Response.FormInfo = Mapper.ToFormInfoDTO(FormInfoBO);


				Epi.Web.Enter.Interfaces.DataInterface.IFormSettingDao IFormSettingDao = entityDaoFactory.FormSettingDao;
				Epi.Web.Enter.Interfaces.DataInterface.IUserDao IUserDao = entityDaoFactory.UserDao;
				Epi.Web.Enter.Interfaces.DataInterface.IFormInfoDao IFormInfoDao = entityDaoFactory.FormInfoDao;
				Epi.Cloud.BLL.FormSetting SettingsImplementation = new Epi.Cloud.BLL.FormSetting(IFormSettingDao, IUserDao, IFormInfoDao);
				Response.FormSetting = Mapper.ToDataTransferObject(SettingsImplementation.GetFormSettings(pRequest.FormInfo.FormId.ToString(), FormInfoBO.Xml, pRequest.CurrentOrgId));



				return Response;


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
#endif //WebEnterCode
            }
            catch (Exception ex)
            {
            }
        }

        public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest)
        {
            try
            {
                var rootId = formsHierarchyRequest.SurveyInfo.FormId;
                var metadatAccessor = new MetadataAccessor(rootId);
                var viewId = metadatAccessor.GetCurrentFormDigest().ViewId;
                var formDigests = metadatAccessor.FormDigests;
                var formInfoDTO = formsHierarchyRequest.SurveyInfo;
                var surveyInfoBO = formInfoDTO.ToSurveyInfoBO(viewId);
                var formsHierarchyBOList = formDigests.ToFormsHierarchyBOList(surveyInfoBO);

                FormsHierarchyResponse formsHierarchyResponse = new FormsHierarchyResponse();

                List<SurveyResponseBO> AllResponsesIDsList = new List<SurveyResponseBO>();

                //1- Get All form  ID's
                List<FormsHierarchyBO> relatedFormIDsList = _surveyInfoService.GetFormsHierarchyIdsByRootId(rootId);

                //2- Get all Responses ID's
                Epi.Cloud.DataEntryServices.SurveyResponseProvider surveyResponseProviderImplementation1 = new SurveyResponseProvider(_surveyResponseDao);
                if (!string.IsNullOrEmpty(formsHierarchyRequest.SurveyResponseInfo.ResponseId))
                {
                    AllResponsesIDsList = surveyResponseProviderImplementation1.GetResponsesHierarchyIdsByRootId(formsHierarchyRequest.SurveyResponseInfo.ResponseId);
                }
                else
                {
                    AllResponsesIDsList = null;
                }

                //3 Combining the lists.
                List<FormsHierarchyBO> combinedList = CombineLists(relatedFormIDsList, AllResponsesIDsList);
                formsHierarchyResponse.FormsHierarchy = combinedList.ToFormsHierarchyDTOList();

                return formsHierarchyResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<FormsHierarchyBO> CombineLists(List<FormsHierarchyBO> relatedFormIDsList, List<SurveyResponseBO> AllResponsesIDsList)
        {

            List<FormsHierarchyBO> List = new List<FormsHierarchyBO>();

            foreach (var Item in relatedFormIDsList)
            {
                FormsHierarchyBO FormsHierarchyBO = new FormsHierarchyBO();
                FormsHierarchyBO.RootFormId = Item.RootFormId;
                FormsHierarchyBO.FormId = Item.FormId;
                FormsHierarchyBO.ViewId = Item.ViewId;
                FormsHierarchyBO.IsSqlProject = Item.IsSqlProject;
                FormsHierarchyBO.IsRoot = Item.IsRoot;
                FormsHierarchyBO.SurveyInfo = Item.SurveyInfo;
                if (AllResponsesIDsList != null)
                {
                    FormsHierarchyBO.ResponseIds = AllResponsesIDsList.Where(x => x.SurveyId == Item.FormId).ToList();
                }
                List.Add(FormsHierarchyBO);
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
                //  result.SurveyInfoList = FormInfoBOList;

                foreach (FormInfoBO item in FormInfoBOList)
                {
                    result.FormInfoList.Add(item.ToFormInfoDTO());
                }


            }
            catch (Exception ex)
            {

            }
            return result;


        }
        public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest surveyAnswerRequest)
        {
            throw new NotImplementedException();
            try
            {
#if WebEnterCode
			Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Cloud.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Cloud.BLL.SurveyResponse Implementation = new Epi.Cloud.BLL.SurveyResponse(SurveyResponseDao);

			//List<SurveyResponseBO> SurveyResponseBOList = Implementation.GetResponsesByRelatedFormId(pRequest.Criteria.SurveyAnswerIdList[0], pRequest.Criteria.SurveyId);

			List<SurveyResponseBO> SurveyResponseBOList = Implementation.GetResponsesByRelatedFormId(pRequest.Criteria.SurveyAnswerIdList[0], pRequest.Criteria);

			SurveyAnswerResponse.SurveyResponseList = Mapper.ToDataTransferObject(SurveyResponseBOList);
			//Query The number of records

			//SurveyAnswerResponse.NumberOfPages = Implementation.GetNumberOfPages(pRequest.Criteria.SurveyId, pRequest.Criteria.IsMobile);
			//SurveyAnswerResponse.NumberOfResponses = Implementation.GetNumberOfResponses(pRequest.Criteria);

			//SurveyAnswerResponse.NumberOfPages = Implementation.GetNumberOfPages(pRequest.Criteria);
			//SurveyAnswerResponse.NumberOfResponses = Implementation.GetNumberOfResponses(pRequest.Criteria);

			return SurveyAnswerResponse;
#endif //WebEnterCode
            }
            catch (Exception ex)
            {
            }
        }

        public SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest surveyAnswerRequest)
        {
            throw new NotImplementedException();
            try
            {
#if WebEnterCode
			Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Cloud.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Cloud.BLL.SurveyResponse Implementation = new Epi.Cloud.BLL.SurveyResponse(SurveyResponseDao);
			List<SurveyResponseBO> SurveyResponseBOList = Implementation.GetResponsesHierarchyIdsByRootId(pRequest.SurveyAnswerList[0].ResponseId);
			SurveyAnswerResponse.SurveyResponseList = Mapper.ToDataTransferObject(SurveyResponseBOList);

			return SurveyAnswerResponse;
#endif //WebEnterCode
            }
            catch (Exception ex)
            {
            }
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
            var surveyInfoBO = _surveyInfoService.GetSurveyInfoById(surveyId);
            var result = new SurveyInfoResponse();
            result.SurveyInfoList.Add(surveyInfoBO.ToSurveyInfoDTO());
            return result;
#if WebEnterCode
            try
			{
			try
			{
				SurveyInfoResponse result = new SurveyInfoResponse(pRequest.RequestId);
				//Epi.Cloud.BLL.SurveyInfo implementation = new Epi.Cloud.BLL.SurveyInfo(_surveyInfoDao);

				Epi.Cloud.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Cloud.Interfaces.DataInterfaces.ISurveyInfoDao surveyInfoDao = entityDaoFactory.SurveyInfoDao;
				Epi.Cloud.BLL.SurveyInfo implementation = new Epi.Cloud.BLL.SurveyInfo(surveyInfoDao);

				// Validate client tag, access token, and user credentials
				if (!ValidRequest(pRequest, result, Validate.All))
				{
					return result;
				}

				var criteria = pRequest.Criteria as SurveyInfoCriteria;
				string sort = criteria.SortExpression;
				List<string> SurveyIdList = new List<string>();
				foreach (string id in criteria.SurveyIdList)
				{
					SurveyIdList.Add(id.ToUpper());
				}


				//if (request.LoadOptions.Contains("SurveyInfos"))
				//{
				//    IEnumerable<SurveyInfoDTO> SurveyInfos;
				//    if (!criteria.IncludeOrderStatistics)
				//    {
				//        SurveyInfos = Implementation.GetSurveyInfos(sort);
				//    }
				//    else
				//    {
				//        SurveyInfos = Implementation.GetSurveyInfosWithOrderStatistics(sort);
				//    }

				//    response.SurveyInfos = SurveyInfos.Select(c => Mapper.ToDataTransferObject(c)).ToList();
				//}

				//if (pRequest.LoadOptions.Contains("SurveyInfo"))
				//{
				result.SurveyInfoList = Mapper.ToDataTransferObject(implementation.GetSurveyInfoById(SurveyIdList));
				//}

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
			catch (Exception ex)
			{
			}
#endif //WebEnterCode
        }

        public bool HasResponse(string childFormId, string parentResponseId)
        {
            try
            {
                var hasResponse = _surveyResponseProvider.HasResponse(childFormId, parentResponseId);
                return hasResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
