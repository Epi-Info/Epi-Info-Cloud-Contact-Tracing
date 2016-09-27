using System;
using Epi.Cloud.Interfaces.DataInterface;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.MessageBase;
using System.Collections.Generic;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Exception;
using System.ServiceModel;
using Epi.Web.EF;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Cloud.DataEntryServices
{
	public class DataEntryService : IDataEntryService
	{

		private readonly Epi.Cloud.DataEntryServices.DAO.SurveyInfoDao _surveyInfoDao;
		private readonly Epi.Cloud.DataEntryServices.SurveyResponseProvider _surveyResponseProvider;
		private readonly Epi.Web.WCF.SurveyService.IEWEDataService _eweDataService;
		private readonly Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory _entityDaoFactory;
		private string _accessToken;
		private string _userName;

		public DataEntryService(
			Epi.Cloud.DataEntryServices.DAO.SurveyInfoDao surveyInfoDao,
			Epi.Cloud.DataEntryServices.SurveyResponseProvider surveyResponseProvider,
			Epi.Web.WCF.SurveyService.IEWEDataService eweDataService,
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory)
		{
			_surveyInfoDao = surveyInfoDao;
			_surveyResponseProvider = surveyResponseProvider;
			_eweDataService = eweDataService;
			_entityDaoFactory = entityDaoFactory;
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
				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new Web.EF.EntityDaoFactory();
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyResponseDao = entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse surveyResponseImplementation = new Epi.Web.BLL.SurveyResponse(surveyResponseDao);


				// Validate client tag, access token, and user credentials
				if (!ValidRequest(request, result, Validate.All))
				{
					return result;
				}

				var criteria = request.Criteria as SurveyAnswerCriteria;
				string sort = criteria.SortExpression;

				Epi.Web.BLL.SurveyInfo surveyInfoImplementation = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);
				SurveyInfoBO surveyInfoBO = surveyInfoImplementation.GetSurveyInfoById(request.Criteria.SurveyId);
				List<SurveyInfoBO> surveyInfoBOList = new List<SurveyInfoBO>();
				surveyInfoBOList.Add(surveyInfoBO);

				// result.SurveyResponseList = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToDataTransferObject(Implementation.GetSurveyResponseById(pRequest.Criteria.SurveyAnswerIdList, pRequest.Criteria.UserPublishKey, pRequest.Criteria.SurveyId, SurveyInfoBOList));

				List<SurveyResponseBO> surveyResponseList = surveyResponseImplementation.GetSurveyResponseById(request.Criteria, surveyInfoBOList);
				result.SurveyResponseList = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToDataTransferObject(surveyResponseList);
				result.FormInfo = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToFormInfoDTO(surveyInfoBO);
				//SurveyResponseBO Request = new SurveyResponseBO();

				//foreach (var item in pRequest.Criteria.SurveyAnswerIdList)
				//    {
				//    Request.ResponseId = item;
				//    Request.UserId = criteria.UserId;

				//    result.SurveyResponseList.Add(Epi.Web.Enter.Common.ObjectMapping.Mapper.ToDataTransferObject(Implementation.GetSurveyResponseByUserId(Request)));
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

				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new Web.EF.EntityDaoFactory();
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyResponseDao = entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse surveyResponseImplementation = new Epi.Web.BLL.SurveyResponse(surveyResponseDao);


				// Validate client tag, access token, and user credentials
				if (!ValidRequest(request, result, Validate.All))
				{
					return result;
				}

				SurveyResponseBO surveyResponseBO = surveyResponseImplementation.GetSurveyResponseStateById(request.Criteria);
				SurveyAnswerDTO surveyAnswerDTO = surveyResponseBO != null ? surveyResponseBO.ToSurveyAnswerDTO() : null;
				result.SurveyResponseList = new List<Web.Enter.Common.DTO.SurveyAnswerDTO>();
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

			// Validate client tag, access token, and user credentials
			if (!ValidRequest(surveyAnswerRequest, response, Validate.All))
			{
				return response;
			}

			// Transform SurveyResponse data transfer object to SurveyResponse business object

			SurveyResponseBO surveyResponseBO = surveyAnswerRequest.SurveyAnswerList[0].ToSurveyResponseBO();

			surveyResponseBO.UserId = surveyAnswerRequest.Criteria.UserId;
			surveyResponseBO.CurrentOrgId = surveyAnswerRequest.Criteria.UserOrganizationId;

			// Validate SurveyResponse business rules

			if (surveyAnswerRequest.Action != "Delete")
			{
				//if (!SurveyResponse.Validate())
				//{
				//    response.Acknowledge = AcknowledgeType.Failure;

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
			else if (surveyAnswerRequest.Action.Equals("Update", StringComparison.OrdinalIgnoreCase))
			{
				_surveyResponseProvider.UpdateSurveyResponse(surveyResponseBO);
				response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());
			}

			else if (surveyAnswerRequest.Action.Equals("CreateChild", StringComparison.OrdinalIgnoreCase))
			{
				Epi.Web.BLL.SurveyInfo surveyInfoImplementation = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);
				SurveyInfoBO surveyInfoBO = surveyInfoImplementation.GetParentInfoByChildId(surveyResponseBO.SurveyId);

				_surveyResponseProvider.InsertChildSurveyResponse(surveyResponseBO, surveyInfoBO, surveyAnswerRequest.SurveyAnswerList[0].RelateParentId);
				response.SurveyResponseList.Add(surveyResponseBO.ToSurveyAnswerDTO());

				List<SurveyResponseBO> List = new List<SurveyResponseBO>();
				List.Add(surveyResponseBO);
				_surveyResponseProvider.InsertSurveyResponse(List, surveyAnswerRequest.Criteria.UserId, true);
			}
			else if (surveyAnswerRequest.Action.Equals("UpdateMulti", StringComparison.OrdinalIgnoreCase))
			{
				throw new NotImplementedException("UpdateMulti");
				//    Implementation.UpdateSurveyResponse(SurveyResponse);
				//    response.SurveyResponseList.Add(Mapper.ToDataTransferObject(SurveyResponse));

				//    Epi.Web.BLL.SurveyResponse Implementation1 = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);
				//    List<SurveyResponseBO> SurveyResponseBOList = Implementation1.GetResponsesHierarchyIdsByRootId(surveyAnswerRequest.SurveyAnswerList[0].ResponseId);


				//    List<SurveyResponseBO> ResultList = Implementation.UpdateSurveyResponse(SurveyResponseBOList, SurveyResponse.Status);
				//    foreach (var Obj in ResultList)
				//    {

				//        response.SurveyResponseList.Add(Mapper.ToDataTransferObject(Obj));
				//    }
			}
			else if (surveyAnswerRequest.Action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
			{
				throw new NotImplementedException("Delete");
			//    var criteria = surveyAnswerRequest.Criteria as SurveyAnswerCriteria;
			//    criteria.SurveyAnswerIdList = new List<string> { SurveyResponse.SurveyId };
			//    criteria.UserPublishKey = SurveyResponse.UserPublishKey;
			//    criteria.SurveyId = surveyAnswerRequest.Criteria.SurveyId;
			//    var survey = Implementation.GetSurveyResponseById(criteria);

			//    foreach (SurveyResponseBO surveyResponse in survey)
			//    {
			//        try
			//        {

			//            if (Implementation.DeleteSurveyResponse(surveyResponse))
			//            {
			//                response.RowsAffected += 1;
			//            }

			//        }
			//        catch
			//        {
			//            //response.RowsAffected = 0;
			//        }
			//    }
			}
			else if (surveyAnswerRequest.Action.Equals("DeleteResponse", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					throw new NotImplementedException("DeleteResponse");
					//    foreach (var item in surveyAnswerRequest.SurveyAnswerList)
					//    {
					//        try
					//        {
					//            ResponseXmlBO ResponseXmlBO = new ResponseXmlBO();
					//            ResponseXmlBO.ResponseId = item.ResponseId;
					//            Implementation.DeleteResponseXml(ResponseXmlBO);
					//            Implementation.UpdateRecordStatus(ResponseXmlBO.ResponseId.ToString(), 2);

					//        }
					//        catch
					//        {

					//        }
					//    }
				}
				catch (Exception ex)
				{
					Console.WriteLine("DeleteResponse: " + ex.ToString());
				}
			}

			return response;
			}
#if WebEnterCode
			try
			{
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = new EF.EntitySurveyResponseDao();
				Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);


				SurveyAnswerResponse response = new SurveyAnswerResponse(request.RequestId);

				// Validate client tag, access token, and user credentials
				if (!ValidRequest(request, response, Validate.All))
				{
					return response;
				}

				// Transform SurveyResponse data transfer object to SurveyResponse business object

				SurveyResponseBO SurveyResponse = Mapper.ToBusinessObject(request.SurveyAnswerList, request.Criteria.UserId)[0];

				SurveyResponse.UserId = request.Criteria.UserId;
				SurveyResponse.CurrentOrgId = request.Criteria.UserOrganizationId;
				// Validate SurveyResponse business rules

				if (request.Action != "Delete")
				{
					//if (!SurveyResponse.Validate())
					//{
					//    response.Acknowledge = AcknowledgeType.Failure;

					//    foreach (string error in SurveyResponse.ValidationErrors)
					//        response.Message += error + Environment.NewLine;

					//    return response;
					//}
				}

				// Run within the context of a database transaction. Currently commented out.
				// The Decorator Design Pattern. 
				//using (TransactionDecorator transaction = new TransactionDecorator())
				{
					if (request.Action.Equals("Create", StringComparison.OrdinalIgnoreCase))
					{
						Implementation.InsertSurveyResponse(SurveyResponse);
						response.SurveyResponseList.Add(Mapper.ToDataTransferObject(SurveyResponse));
					}
					else if (request.Action.Equals("CreateMulti", StringComparison.OrdinalIgnoreCase))
					{

						Epi.Web.BLL.SurveyResponse Implementation1 = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);
						List<SurveyResponseBO> SurveyResponseBOList = Implementation1.GetResponsesHierarchyIdsByRootId(request.SurveyAnswerList[0].ParentRecordId);


						if (!request.SurveyAnswerList[0].RecoverLastRecordVersion)
						// if we are not keeping the version of xml found currently in the SurveyResponse table (meaning getting the original copy form the ResponseXml table)
						{
							//check if any orphan records exists 
							foreach (var item in SurveyResponseBOList)
							{

								SurveyResponseBO SurveyResponseBO = Implementation.GetResponseXml(item.ResponseId);
								// before we delete the temp version we need to move it the SurveResponse table

								if (!string.IsNullOrEmpty(SurveyResponseBO.ResponseId))
								{
									SurveyResponseBO.UserId = request.Criteria.UserId;
									ResponseXmlBO ResponseXmlBO = new ResponseXmlBO();
									ResponseXmlBO.ResponseId = SurveyResponseBO.ResponseId;
									// During the delete process below: 
									//  1) Delete the record from ResponseXml table.
									//  2) Update Record status in the SurveyResponse table which fires database triggers.
									Implementation.DeleteResponseXml(ResponseXmlBO);
									Implementation.UpdateRecordStatus(ResponseXmlBO.ResponseId.ToString(), 2);

									//This will handle the status update and the swapping of the Xml
									// but for this scenario I will keep the status unchanged 
									// Implementation.DeleteSurveyResponseInEditMode(SurveyResponseBO);


								}
							}

							SurveyResponseBOList = Implementation1.GetResponsesHierarchyIdsByRootId(request.SurveyAnswerList[0].ParentRecordId);
							// Inserting a temp xml to the ResponseXml table
							response.SurveyResponseList = Mapper.ToDataTransferObject(Implementation.InsertSurveyResponse(SurveyResponseBOList, request.Criteria.UserId));
						}
						else
						{
							// load the version curently found the SurveyResponse table 

							response.SurveyResponseList = Mapper.ToDataTransferObject(SurveyResponseBOList);
						}
					}
					else if (request.Action.Equals("CreateChild", StringComparison.OrdinalIgnoreCase))
					{
						Epi.Web.BLL.SurveyInfo Implementation1 = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);
						SurveyInfoBO SurveyInfoBO = Implementation1.GetParentInfoByChildId(SurveyResponse.SurveyId);

						Implementation.InsertChildSurveyResponse(SurveyResponse, SurveyInfoBO, request.SurveyAnswerList[0].RelateParentId);
						response.SurveyResponseList.Add(Mapper.ToDataTransferObject(SurveyResponse));

						List<SurveyResponseBO> List = new List<SurveyResponseBO>();
						List.Add(SurveyResponse);
						Implementation.InsertSurveyResponse(List, request.Criteria.UserId, true);

					}
					else if (request.Action.Equals("Update", StringComparison.OrdinalIgnoreCase))
					{
						Implementation.UpdateSurveyResponse(SurveyResponse);
						response.SurveyResponseList.Add(Mapper.ToDataTransferObject(SurveyResponse));
					}
					else if (request.Action.Equals("UpdateMulti", StringComparison.OrdinalIgnoreCase))
					{
						Implementation.UpdateSurveyResponse(SurveyResponse);
						response.SurveyResponseList.Add(Mapper.ToDataTransferObject(SurveyResponse));

						Epi.Web.BLL.SurveyResponse Implementation1 = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);
						List<SurveyResponseBO> SurveyResponseBOList = Implementation1.GetResponsesHierarchyIdsByRootId(request.SurveyAnswerList[0].ResponseId);


						List<SurveyResponseBO> ResultList = Implementation.UpdateSurveyResponse(SurveyResponseBOList, SurveyResponse.Status);
						foreach (var Obj in ResultList)
						{

							response.SurveyResponseList.Add(Mapper.ToDataTransferObject(Obj));
						}
					}
					else if (request.Action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
					{
						var criteria = request.Criteria as SurveyAnswerCriteria;
						criteria.SurveyAnswerIdList = new List<string> { SurveyResponse.SurveyId };
						criteria.UserPublishKey = SurveyResponse.UserPublishKey;
						criteria.SurveyId = request.Criteria.SurveyId;
						var survey = Implementation.GetSurveyResponseById(criteria);

						foreach (SurveyResponseBO surveyResponse in survey)
						{
							try
							{

								if (Implementation.DeleteSurveyResponse(surveyResponse))
								{
									response.RowsAffected += 1;
								}

							}
							catch
							{
								//response.RowsAffected = 0;
							}
						}
					}
					else if (request.Action.Equals("DeleteResponse", StringComparison.OrdinalIgnoreCase))
					{

						foreach (var item in request.SurveyAnswerList)
						{
							try
							{
								ResponseXmlBO ResponseXmlBO = new ResponseXmlBO();
								ResponseXmlBO.ResponseId = item.ResponseId;
								Implementation.DeleteResponseXml(ResponseXmlBO);
								Implementation.UpdateRecordStatus(ResponseXmlBO.ResponseId.ToString(), 2);

							}
							catch
							{

							}
						}
					}
				}

				return response;
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
		
		public void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest)
		{
			try
			{
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyResponseDao = _entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(surveyResponseDao);

				SurveyResponseBO SurveyResponse = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToBusinessObject(surveyAnswerRequest.SurveyAnswerList, surveyAnswerRequest.Criteria.UserId)[0];

				Epi.Web.BLL.SurveyResponse Implementation1 = new Epi.Web.BLL.SurveyResponse(surveyResponseDao);

				List<SurveyResponseBO> SurveyResponseBOList = Implementation1.GetSurveyResponseById(surveyAnswerRequest.Criteria);

				List<SurveyResponseBO> ResultList = Implementation.UpdateSurveyResponse(SurveyResponseBOList, surveyAnswerRequest.Criteria.StatusId);

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

                Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = _entityDaoFactory.SurveyResponseDao;
                Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(ISurveyResponseDao);
                foreach (var response in surveyAnswerRequest.SurveyAnswerList)
                {

                    if (surveyAnswerRequest.Criteria.IsEditMode)
                    {
                        Implementation.DeleteSurveyResponseInEditMode(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId), 2);
                    }
                    else
                    {

                        Implementation.DeleteSurveyResponse(response.ToSurveyResponseBO(surveyAnswerRequest.Criteria.UserId));
                    }

                }

#if WebEnterCode
			try
			{
				SurveyAnswerResponse result = new SurveyAnswerResponse(pRequest.RequestId);


				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(ISurveyResponseDao);
				foreach (var response in pRequest.SurveyAnswerList)
				{
					if (pRequest.Criteria.IsSqlProject)
					{
						if (pRequest.Criteria.IsEditMode)
						{
							Implementation.DeleteSurveyResponseInEditMode(Mapper.ToBusinessObject(response, pRequest.Criteria.UserId), 2);
						}
						else
						{
							if (pRequest.Criteria.IsDeleteMode)
							{
								Implementation.DeleteSurveyResponse(Mapper.ToBusinessObject(response, pRequest.Criteria.UserId));
							}
							else
							{
								//do status Update
								var obj = Mapper.ToBusinessObject(response, pRequest.Criteria.UserId);
								obj.SurveyId = pRequest.Criteria.SurveyId;
								obj.Status = 0;
								Implementation.UpdateRecordStatus(obj);
							}

						}
					}
					else
					{
						if (pRequest.Criteria.IsEditMode)
						{
							Implementation.DeleteSurveyResponseInEditMode(Mapper.ToBusinessObject(response, pRequest.Criteria.UserId), 2);
						}
						else
						{

							Implementation.DeleteSurveyResponse(Mapper.ToBusinessObject(response, pRequest.Criteria.UserId));
						}



					}
				}
#endif //WebEnterCode

				return result;
				//return null;
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
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);
			List<SurveyResponseBO> SurveyResponseBOList = Implementation.GetAncestorResponseIdsByChildId(pRequest.Criteria.SurveyAnswerIdList[0]);
			SurveyAnswerResponse.SurveyResponseList = Mapper.ToDataTransferObject(SurveyResponseBOList);

			return SurveyAnswerResponse;
#endif //WebEnterCode
			}
			catch (Exception ex)
			{
			}
		}

		public SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest surveyInfoRequest)
		{
			throw new NotImplementedException();
			try
			{
#if WebEnterCode
			try
			{
				SurveyInfoResponse result = new SurveyInfoResponse(pRequest.RequestId);


				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Web.BLL.SurveyInfo implementation = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);
				Dictionary<string, int> ParentIdList = new Dictionary<string, int>();
				foreach (var item in pRequest.SurveyInfoList)
				{
					ParentIdList.Add(item.SurveyId, item.ViewId);
				}
				result.SurveyInfoList = Mapper.ToDataTransferObject(implementation.GetChildInfoByParentId(ParentIdList));


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

				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao surveyResponseDao = _entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(surveyResponseDao);

				SurveyAnswerCriteria criteria = surveyAnswerRequest.Criteria;
				//result.SurveyResponseList = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToDataTransferObject(Implementation.GetFormResponseListById(surveyAnswerRequest.Criteria.SurveyId, surveyAnswerRequest.Criteria.PageNumber, surveyAnswerRequest.Criteria.IsMobile));
				result.SurveyResponseList = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToDataTransferObject(Implementation.GetFormResponseListById(criteria));
				surveyAnswerRequest.Criteria.FormResponseCount = result.SurveyResponseList.Count;
				//Query The number of records

				//result.NumberOfPages = Implementation.GetNumberOfPages(surveyAnswerRequest.Criteria.SurveyId, surveyAnswerRequest.Criteria.IsMobile);
				//result.NumberOfResponses = Implementation.GetNumberOfResponses(surveyAnswerRequest.Criteria.SurveyId);

				result.NumberOfPages = Implementation.GetNumberOfPages(surveyAnswerRequest.Criteria);
				result.NumberOfResponses = Implementation.GetNumberOfResponses(surveyAnswerRequest.Criteria);

				//Get form info 
				Epi.Web.Enter.Interfaces.DataInterface.IFormInfoDao formInfoDao = new EntityFormInfoDao();
				Epi.Web.BLL.FormInfo ImplementationFormInfo = new Epi.Web.BLL.FormInfo(formInfoDao);
				result.FormInfo = Epi.Web.Enter.Common.ObjectMapping.Mapper.ToFormInfoDTO(ImplementationFormInfo.GetFormInfoByFormId(surveyAnswerRequest.Criteria.SurveyId, false, surveyAnswerRequest.Criteria.UserId));

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


				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = entityDaoFactory.SurveyResponseDao;
				Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(ISurveyResponseDao);

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
				Epi.Web.BLL.FormInfo ImplementationFormInfo = new Epi.Web.BLL.FormInfo(formInfoDao);
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
				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();


				IFormInfoDao FormInfoDao = entityDaoFactory.FormInfoDao;
				Epi.Web.BLL.FormInfo FormInfoImplementation = new Epi.Web.BLL.FormInfo(FormInfoDao);
				FormInfoBO FormInfoBO = FormInfoImplementation.GetFormInfoByFormId(pRequest.FormInfo.FormId, pRequest.GetMetadata, pRequest.FormInfo.UserId);
				Response.FormInfo = Mapper.ToFormInfoDTO(FormInfoBO);


				Epi.Web.Enter.Interfaces.DataInterface.IFormSettingDao IFormSettingDao = entityDaoFactory.FormSettingDao;
				Epi.Web.Enter.Interfaces.DataInterface.IUserDao IUserDao = entityDaoFactory.UserDao;
				Epi.Web.Enter.Interfaces.DataInterface.IFormInfoDao IFormInfoDao = entityDaoFactory.FormInfoDao;
				Epi.Web.BLL.FormSetting SettingsImplementation = new Epi.Web.BLL.FormSetting(IFormSettingDao, IUserDao, IFormInfoDao);
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
			throw new NotImplementedException();
			try
			{
#if WebEnterCode
			FormsHierarchyResponse FormsHierarchyResponse = new FormsHierarchyResponse();
			List<SurveyResponseBO> AllResponsesIDsList = new List<SurveyResponseBO>();
			//1- Get All form  ID's
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			Epi.Web.BLL.SurveyInfo Implementation = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);

			List<FormsHierarchyBO> RelatedFormIDsList = Implementation.GetFormsHierarchyIdsByRootId(FormsHierarchyRequest.SurveyInfo.FormId);



			//2- Get all Responses ID's

			Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao ISurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Web.BLL.SurveyResponse Implementation1 = new Epi.Web.BLL.SurveyResponse(ISurveyResponseDao);
			if (!string.IsNullOrEmpty(FormsHierarchyRequest.SurveyResponseInfo.ResponseId))
			{
				AllResponsesIDsList = Implementation1.GetResponsesHierarchyIdsByRootId(FormsHierarchyRequest.SurveyResponseInfo.ResponseId);

			}
			else
			{
				AllResponsesIDsList = null;
			}
			//3 Combining the lists.

			FormsHierarchyResponse.FormsHierarchy = Mapper.ToFormHierarchyDTO(CombineLists(RelatedFormIDsList, AllResponsesIDsList));

			return FormsHierarchyResponse;
#endif //WebEnterCode
			}
			catch (Exception ex)
			{
			}
		}

		public FormsInfoResponse GetFormsInfo(FormsInfoRequest formsInfoRequest)
		{
			throw new NotImplementedException();
			try
			{
#if WebEnterCode
			FormsInfoResponse result = new FormsInfoResponse();
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			IFormInfoDao FormInfoDao = entityDaoFactory.FormInfoDao;
			Epi.Web.BLL.FormInfo implementation = new Epi.Web.BLL.FormInfo(FormInfoDao);
			try
			{
				List<FormInfoBO> FormInfoBOList = implementation.GetFormsInfo(pRequest.Criteria.UserId, pRequest.Criteria.CurrentOrgId);
				//  result.SurveyInfoList = FormInfoBOList;

				foreach (FormInfoBO item in FormInfoBOList)
				{
					result.FormInfoList.Add(Mapper.ToFormInfoDTO(item));
				}


			}
			catch (Exception ex)
			{

			}
			return result;
#endif //WebEnterCode
			}
			catch (Exception ex)
			{
			}
		}


		public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest surveyAnswerRequest)
		{
			throw new NotImplementedException();
			try
			{
#if WebEnterCode
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);

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
			Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
			SurveyAnswerResponse SurveyAnswerResponse = new Enter.Common.Message.SurveyAnswerResponse();
			Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao SurveyResponseDao = entityDaoFactory.SurveyResponseDao;
			Epi.Web.BLL.SurveyResponse Implementation = new Epi.Web.BLL.SurveyResponse(SurveyResponseDao);
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
			throw new NotImplementedException();
			try
			{
#if WebEnterCode
			try
			{
				SurveyInfoResponse result = new SurveyInfoResponse(pRequest.RequestId);
				//Epi.Web.BLL.SurveyInfo implementation = new Epi.Web.BLL.SurveyInfo(_surveyInfoDao);

				Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory entityDaoFactory = new EF.EntityDaoFactory();
				Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao surveyInfoDao = entityDaoFactory.SurveyInfoDao;
				Epi.Web.BLL.SurveyInfo implementation = new Epi.Web.BLL.SurveyInfo(surveyInfoDao);

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
#endif //WebEnterCode
			}
			catch (Exception ex)
			{
			}
		}

		public bool HasResponse(string surveyId, string responseId)
		{
			try
			{
				var hasResponse = _surveyResponseProvider.HasResponse(surveyId, responseId);
				return hasResponse;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

#region Security Related
		public UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.PassCodeLogin(userAuthenticationRequest);
			return eweResponse;
		}

		public FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest)
		{
			var eweResponse = _eweDataService.SaveSettings(formSettingRequest);
			return eweResponse;
		}

		public OrganizationResponse SetOrganization(OrganizationRequest organizationRequestRequest)
		{
			var eweResponse = _eweDataService.SetOrganization(organizationRequestRequest);
			return eweResponse;
		}

		public UserAuthenticationResponse SetPassCode(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.SetPassCode(userAuthenticationRequest);
			return eweResponse;
		}

		public UserAuthenticationResponse GetUser(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.GetUser(userAuthenticationRequest);
			return eweResponse;
		}

		public UserResponse GetUserInfo(UserRequest userRequest)
		{
			var eweResponse = _eweDataService.GetUserInfo(userRequest);
			return eweResponse;
		}

		public UserResponse SetUserInfo(UserRequest userRequest)
		{
			var eweResponse = _eweDataService.SetUserInfo(userRequest);
			return eweResponse;
		}

		public OrganizationResponse GetUserOrganizations(OrganizationRequest organizationRequest)
		{
			var eweResponse = _eweDataService.GetUserOrganizations(organizationRequest);
			return eweResponse;
		}

		public bool UpdateUser(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.UpdateUser(userAuthenticationRequest);
			return eweResponse;
		}

		public UserAuthenticationResponse UserLogin(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.UserLogin(userAuthenticationRequest);
			return eweResponse;
		}
		public OrganizationResponse GetAdminOrganizations(OrganizationRequest organizationRequest)
		{
			var eweResponse = _eweDataService.GetAdminOrganizations(organizationRequest);
			return eweResponse;
		}

		public OrganizationResponse GetOrganizationInfo(OrganizationRequest organizationRequest)
		{
			var eweResponse = _eweDataService.GetOrganizationInfo(organizationRequest);
			return eweResponse;
		}

		public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest organizationRequest)
		{
			var eweResponse = _eweDataService.GetOrganizationsByUserId(organizationRequest);
			return eweResponse;
		}

		public OrganizationResponse GetOrganizationUsers(OrganizationRequest organizationRequest)
		{
			var eweResponse = _eweDataService.GetOrganizationUsers(organizationRequest);
			return eweResponse;
		}
		public UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest userAuthenticationRequest)
		{
			var eweResponse = _eweDataService.GetAuthenticationResponse(userAuthenticationRequest);
			return eweResponse;
		}
#endregion Security Related
	}
}
