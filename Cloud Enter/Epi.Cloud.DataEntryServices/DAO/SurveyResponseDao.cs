using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.DataEntryServices.Helpers;
using Epi.Cloud.DataEntryServices.Interfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Interfaces.DataInterfaces;

namespace Epi.Cloud.DataEntryServices.DAO
{
	/// <summary>
	/// Entity Framework implementation of the ISurveyResponseDao interface.
	/// </summary> 
	public class SurveyResponseDao : MetadataAccessor, ISurveyResponseDao
    {
        ISurveyPersistenceFacade _surveyPersistenceFacade;

        public SurveyResponseDao(IProjectMetadataProvider projectMetadataProvider,
                                 ISurveyPersistenceFacade surveyPersistenceFacade)
        {
            ProjectMetadataProvider = projectMetadataProvider;
            _surveyPersistenceFacade = surveyPersistenceFacade;
        }

        private int _dataAccessRuleId;

        /// <summary>
        /// Reads Number of responses for SqlProject
        /// </summary>
        public int SqlProjectResponsesCount { get; set; }

        /// <summary>
        /// Flag for IsSqlProject
        /// </summary>
        public bool IsSqlProject { get; set; }


        /// <summary>
        /// Gets a specific SurveyResponse.
        /// </summary>
        /// <param name="SurveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        public List<SurveyResponseBO> GetSurveyResponse(List<string> surveyResponseIdList, Guid userPublishKey, int gridgridPageNumber = -1, int gridgridPageSize = -1)
        {

            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            if (surveyResponseIdList.Count > 0)
            {
                foreach (string responseId in surveyResponseIdList.Distinct())
                {
                    var formResponseDetail = _surveyPersistenceFacade.GetFormResponseByResponseId(responseId);
                    if (formResponseDetail != null)
                    {
                        var surveyResponseBO = formResponseDetail.ToSurveyResponseBO();
                        result.Add(surveyResponseBO);
                    }
                }
            }
            else
            {
                //TODO Implement for DocumentDB
                throw new NotImplementedException("GetSurveyResponse - return all responses");
                //using (var Context = DataObjectFactory.CreateContext())
                //{

                //    result = Mapper.Map(Context.SurveyResponses.ToList());
                //}
            }

            if (gridgridPageNumber > 0 && gridgridPageSize > 0)
            {
                result.Sort(CompareByDateCreated);
                // remove the items to skip
                if (gridgridPageNumber * gridgridPageSize - gridgridPageSize > 0)
                {
                    result.RemoveRange(0, gridgridPageSize);
                }

                if (gridgridPageNumber * gridgridPageSize < result.Count)
                {
                    result.RemoveRange(gridgridPageNumber * gridgridPageSize, result.Count - gridgridPageNumber * gridgridPageSize);
                }
            }

            return result;
        }


        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyResponseIdList, Guid userPublishKey, int gridgridPageNumber = -1, int gridgridPageSize = -1, int responseMaxSize = -1)
        {
            List<SurveyResponseBO> resultRows = GetSurveyResponse(surveyResponseIdList, userPublishKey, gridgridPageNumber, gridgridPageSize);

            return resultRows;
        }

        /// <summary>
        /// Gets SurveyResponses per a SurveyId.
        /// </summary>
        /// <param name="SurveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        public List<SurveyResponseBO> GetSurveyResponseBySurveyId(List<string> surveyIdList, Guid userPublishKey, int gridgridPageNumber = -1, int gridgridPageSize = -1)
        {
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            try
            {
                foreach (string surveyResponseId in surveyIdList.Distinct())
                {
                    Guid Id = new Guid(surveyResponseId);

                    //TODO Implement for DocumentDB
                    //using (var Context = DataObjectFactory.CreateContext())
                    //{

                    //    result.Add(Mapper.Map(Context.SurveyResponses.FirstOrDefault(x => x.SurveyId == Id)));
                    //}
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            if (gridgridPageNumber > 0 && gridgridPageSize > 0)
            {
                result.Sort(CompareByDateCreated);
                // remove the items to skip
                if (gridgridPageNumber * gridgridPageSize - gridgridPageSize > 0)
                {
                    result.RemoveRange(0, gridgridPageSize);
                }

                if (gridgridPageNumber * gridgridPageSize < result.Count)
                {
                    result.RemoveRange(gridgridPageNumber * gridgridPageSize, result.Count - gridgridPageNumber * gridgridPageSize);
                }
            }

            return result;
        }

        public SurveyResponseBO GetSurveyResponseState(string responseId)
        {
            var formResponseDetail =_surveyPersistenceFacade.GetFormResponseState(responseId);
            return formResponseDetail != null ? formResponseDetail.ToSurveyResponseBO() : null;
        }

        public List<SurveyResponseBO> GetSurveyResponseBySurveyIdSize(List<string> surveyIdList, Guid userPublishKey, int gridgridPageNumber = -1, int gridgridPageSize = -1, int responseMaxSize = -1)
        {
            List<SurveyResponseBO> resultRows = GetSurveyResponseBySurveyId(surveyIdList, userPublishKey, gridgridPageNumber, gridgridPageSize);
            return resultRows;
        }

        public List<SurveyResponseBO> GetSurveyResponse(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int gridgridPageNumber = -1, int gridgridPageSize = -1)
        {
            List<SurveyResponseBO> finalresult = new List<SurveyResponseBO>();
            IEnumerable<SurveyResponseBO> result;
            List<SurveyResponse> responseList = new List<SurveyResponse>();

            if (surveyAnswerIdList.Count > 0)
            {
                foreach (string surveyResponseId in surveyAnswerIdList.Distinct())
                {
                    try
                    {
                        SurveyResponse surveyResponse = null;
                        Guid Id = new Guid(surveyResponseId);

                        //TODO Implement for DocumentDB
                        //var Context = DataObjectFactory.CreateContext();
                        //surveyResponse = Context.SurveyResponses.First(x => x.ResponseId == Id);

                        if (surveyResponse != null)
                        {
                            responseList.Add(surveyResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(surveyId))
                    {
                        //TODO Implement for DocumentDB
                        //var Context = DataObjectFactory.CreateContext();
                        //Guid Id = new Guid(surveyId);
                        //if (statusId == RecordStatus.Submitted)
                        //{
                        //    responseList = Context.SurveyResponses.Where(x => x.SurveyId == Id && x.StatusId != 4 && x.isDraftMode == isDraftMode).ToList();
                        //}
                        //else
                        //{

                        //    responseList = Context.SurveyResponses.Where(x => x.SurveyId == Id && x.isDraftMode == isDraftMode).ToList();

                        //}
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }

            if (gridgridPageSize != -1 && gridgridPageNumber != -1)
            {
                result = Mapper.Map(responseList);

                foreach (SurveyResponseBO item in result)
                {
                    List<SurveyResponseBO> ResponsesHierarchy = this.GetResponsesHierarchyIdsByRootId(item.ResponseId.ToString());

                    result.Where(x => x.ResponseId == item.ResponseId).Single().ResponseHierarchyIds = ResponsesHierarchy;

                }

                result = result.Skip((gridgridPageNumber - 1) * gridgridPageSize).Take(gridgridPageSize);
                foreach (var item in result)
                {
                    finalresult.Add(item);

                }
                return finalresult;
            }
            else
            {
                finalresult = Mapper.Map(responseList);
                foreach (SurveyResponseBO item in finalresult)
                {
                    List<SurveyResponseBO> ResponsesHierarchy = this.GetResponsesHierarchyIdsByRootId(item.ResponseId.ToString());

                    finalresult.Where(x => x.ResponseId == item.ResponseId).Single().ResponseHierarchyIds = ResponsesHierarchy;

                }

                return finalresult;
            }
        }

        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int gridPageNumber = -1, int gridPageSize = -1, int ResponseMaxSize = -1)
        {
            List<SurveyResponseBO> resultRows = GetSurveyResponse(surveyAnswerIdList, surveyId, dateCompleted, isDraftMode, statusId, gridPageNumber, gridPageSize);
            return resultRows;
        }

        public void InsertResponse(ResponseBO responseBO)
        {
            // TODO: DocumentDB implementation required
            try
            {
                Guid Id = new Guid(responseBO.ResponseId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    ResponseXml ResponseXml = Mapper.ToEF(ResponseXmlBO);
                //    Context.AddToResponseXmls(ResponseXml);

                //    //Update Status
                //    var Query = from response in Context.SurveyResponses
                //                where response.ResponseId == Id
                //                select response;

                //    var DataRow = Query.Single();
                //    DataRow.StatusId = 1;
                //    Context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }



        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>  
        /// <param name="surveyResponseBO">SurveyResponse.</param>
        public void InsertSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            try
            {
                //Save Properties to Document DB
                surveyResponseBO.DateCreated = DateTime.UtcNow;
                surveyResponseBO.DateUpdated = DateTime.UtcNow;

                var formInfoResponse = _surveyPersistenceFacade.SaveFormProperties(surveyResponseBO);
                var pageResponse = _surveyPersistenceFacade.InsertResponse(surveyResponseBO);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>  
        /// <param name="surveyResponseBO">SurveyResponse.</param>
        public void InsertChildSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            try
            {
                //TODO Implement for DocumentDB
                var response = _surveyPersistenceFacade.InsertChildResponseAsync(surveyResponseBO);
				var result = response.Result;
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    SurveyResponse SurveyResponseEntity = Mapper.ToEF(SurveyResponse, SurveyResponse.CurrentOrgId);
                //    User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);
                //    SurveyResponseEntity.Users.Add(User);
                //    Context.AddToSurveyResponses(SurveyResponseEntity);

                //    Context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// Updates a SurveyResponse.
        /// </summary>
        /// <param name="SurveyResponse">SurveyResponse.</param>
        public void UpdateSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            try
            {
                //Save Properties to Document DB
                InsertSurveyResponse(surveyResponseBO);
                if(surveyResponseBO.Status==RecordStatus.Saved)
                {
                    UpdateRecordStatus(surveyResponseBO);
                }

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    var Query = from response in Context.SurveyResponses
                //                where response.ResponseId == Id
                //                select response;

                //    var DataRow = Query.Single();

                //    if (!string.IsNullOrEmpty(SurveyResponse.RelateParentId) && SurveyResponse.RelateParentId != Guid.Empty.ToString())
                //    {
                //        DataRow.RelateParentId = new Guid(SurveyResponse.RelateParentId);
                //    }
                //    DataRow.ResponseXML = SurveyResponse.XML;
                //    //DataRow.DateCompleted = DateTime.Now;
                //    DataRow.DateCompleted = SurveyResponse.DateCompleted;
                //    DataRow.StatusId = SurveyResponse.Status;
                //    DataRow.DateUpdated = DateTime.Now;
                //    //   DataRow.ResponsePasscode = SurveyResponse.ResponsePassCode;
                //    DataRow.isDraftMode = SurveyResponse.isDraftMode;
                //    DataRow.ResponseXMLSize = RemoveWhitespace(SurveyResponse.XML).Length;
                //    Context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                //TODO: GEL Don't throw this exception. This code will be removed when DocumentDB is fully implemented.
                //throw (ex);
            }
        }
        public static string RemoveWhitespace(string xml)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@">\s*<");
            xml = regex.Replace(xml, "><");

            return xml.Trim();
        }

#if RequiresUserAuthenticationObjects
        public void UpdatePassCode(Enter.Common.BusinessObject.UserAuthenticationRequestBO passcodeBO)
        {

            try
            {
                Guid Id = new Guid(passcodeBO.ResponseId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    var Query = from response in Context.SurveyResponses
                //                where response.ResponseId == Id
                //                select response;

                //    var DataRow = Query.Single();

                //    DataRow.ResponsePasscode = passcodeBO.PassCode;
                //    Context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public Enter.Common.BusinessObject.UserAuthenticationResponseBO GetAuthenticationResponse(Enter.Common.BusinessObject.UserAuthenticationRequestBO UserAuthenticationRequestBO)
        {

            Enter.Common.BusinessObject.UserAuthenticationResponseBO UserAuthenticationResponseBO = Mapper.ToAuthenticationResponseBO(UserAuthenticationRequestBO);
            try
            {

                Guid Id = new Guid(UserAuthenticationRequestBO.ResponseId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    SurveyResponse surveyResponse = Context.SurveyResponses.First(x => x.ResponseId == Id);
                //    if (surveyResponse != null)
                //    {
                //        UserAuthenticationResponseBO.PassCode = surveyResponse.ResponsePasscode;
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return UserAuthenticationResponseBO;

        }
#endif //RequiresUserAuthenticationObjects

        /// <summary>
        /// Deletes a SurveyResponse
        /// </summary>
        /// <param name="SurveyResponse">SurveyResponse.</param>
        public void DeleteSurveyResponse(SurveyResponseBO surveyResponseBO)
        {
            try
            {
                List<SurveyResponseBO> result = new List<SurveyResponseBO>();
                Guid Id = new Guid(surveyResponseBO.ResponseId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    IQueryable<SurveyResponse> Query = Context.SurveyResponses.Where(x => x.ResponseId == Id).OrderBy(x => x.DateCreated).Traverse(x => x.SurveyResponse1).AsQueryable();
                //    result = Mapper.Map(Query);
                //    foreach (var Obj in result)
                //    {

                //        if (!string.IsNullOrEmpty(Obj.ResponseId))
                //        {
                //            Guid NewId = new Guid(Obj.ResponseId);

                //            User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);

                //            if (User == null)
                //            {
                //                var ResponseXml = Context.ResponseXmls.FirstOrDefault(x => x.ResponseId == new Guid(SurveyResponse.ResponseId));
                //                User = Context.Users.FirstOrDefault(x => x.UserID == ResponseXml.UserId);
                //            }

                //            SurveyResponse Response = Context.SurveyResponses.First(x => x.ResponseId == NewId);
                //            Response.Users.Remove(User);


                //            Context.SurveyResponses.DeleteObject(Response);

                //            Context.SaveChanges();
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }



        }

        public void DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponseBO)
        {
            string parentRecordId;

            try
            {
                List<SurveyResponseBO> result = new List<SurveyResponseBO>();
                Guid Id = new Guid(surveyResponseBO.ResponseId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    IQueryable<SurveyResponse> Query = Context.SurveyResponses.Where(x => x.ResponseId == Id).OrderBy(x => x.DateCreated).Traverse(x => x.SurveyResponse1).AsQueryable();
                //    result = Mapper.Map(Query);
                //    foreach (var Obj in result)
                //    {
                //        parentRecordId = "";
                //        if (!string.IsNullOrEmpty(Obj.ParentRecordId))
                //        {

                //            parentRecordId = Obj.ParentRecordId;
                //        }
                //        if (!string.IsNullOrEmpty(Obj.ResponseId))
                //        {
                //            Guid NewId = new Guid(Obj.ResponseId);

                //            User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);

                //            SurveyResponse Response = Context.SurveyResponses.First(x => x.ResponseId == NewId);
                //            Response.Users.Remove(User);

                //            Context.SurveyResponses.DeleteObject(Response);

                //            Context.SaveChanges();
                //        }

                //        if (!string.IsNullOrEmpty(parentRecordId))
                //        {
                //            Guid pId = new Guid(parentRecordId);
                //            User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);

                //            SurveyResponse Response = Context.SurveyResponses.First(x => x.ResponseId == pId);
                //            Response.Users.Remove(User);

                //            Context.SurveyResponses.DeleteObject(Response);

                //            Context.SaveChanges();


                //        }
                //   }


                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }



        public void DeleteSingleSurveyResponse(SurveyResponseBO SurveyResponse)
        {

            var responseId = SurveyResponse.ResponseId;
            var userId = SurveyResponse.UserId;
            _surveyPersistenceFacade.DeleteResponse(responseId, userId);
            try
            {
                Guid Id = new Guid(SurveyResponse.ResponseId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);

                //    SurveyResponse Response = Context.SurveyResponses.First(x => x.ResponseId == Id);
                //    Response.Users.Remove(User);

                //    Context.SurveyResponses.DeleteObject(Response);

                //    Context.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        private static int CompareByDateCreated(SurveyResponseBO x, SurveyResponseBO y)
        {
            return x.DateCreated.CompareTo(y.DateCreated);
        }


        public List<SurveyResponseBO> GetFormResponseByFormId(string FormId, int gridPageNumber, int gridPageSize)
        {
            throw new NotImplementedException("GetFormResponseByFormId");

            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            try
            {

                Guid Id = new Guid(FormId);

                //TODO Implement for DocumentDB
                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    IQueryable<SurveyResponse> SurveyResponseList = Context.SurveyResponses.Where(x => x.SurveyId == Id
                //        //&& string.IsNullOrEmpty(x.ParentRecordId.ToString()) == true 
                //        //&& string.IsNullOrEmpty(x.RelateParentId.ToString()) == true 
                //        && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                //        && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                //        && x.StatusId > 1).OrderByDescending(x => x.DateUpdated);

                //    SurveyResponseList = SurveyResponseList.Skip((gridPageNumber - 1) * gridPageSize).Take(gridPageSize);

                //    foreach (SurveyResponse Response in SurveyResponseList)
                //    {
                //        result.Add(Mapper.Map(Response, Response.Users.First()));
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }

        public List<SurveyResponseBO> GetFormResponseByFormId(SurveyAnswerCriteria criteria)
        {
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            _dataAccessRuleId = GetDataAccessRule(criteria.SurveyId, criteria.UserId);

            try
            {
                Guid Id = new Guid(criteria.SurveyId);

                if (criteria.IsShareable)
                {
#if ImplementSharableRules
                    //Shareable
                    using (var Context = Epi.Web.EF.DataObjectFactory.CreateContext())
                    {

                        IQueryable<SurveyResponse> SurveyResponseList;
                        switch (_dataAccessRuleId)
                        {
                            case 1: //   Organization users can only access the data of there organization
                                SurveyResponseList = Context.SurveyResponses.Where(
                                    x => x.SurveyId == Id
                                        && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                                        && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                                        && x.StatusId >= 1 && x.OrganizationId == criteria.UserOrganizationId)
                                        .OrderByDescending(x => x.DateUpdated);
                                break;
                            case 2:    // All users in host organization will have access to all data of all organizations  

                                // get All the users of Host organization
                                var Users = Context.UserOrganizations.Where(x => x.OrganizationID == criteria.UserOrganizationId && x.Active == true).ToList();
                                int Count = Users.Where(x => x.UserID == criteria.UserId).Count();
                                if (Count > 0)
                                {
                                    SurveyResponseList = Context.SurveyResponses.Where(
                                        x => x.SurveyId == Id
                                            && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                                            && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                                            && x.StatusId >= 1)
                                            .OrderByDescending(x => x.DateUpdated);
                                }
                                else
                                {

                                    SurveyResponseList = Context.SurveyResponses.Where(
                                       x => x.SurveyId == Id
                                            && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                                            && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                                            && x.StatusId >= 1 && x.OrganizationId == criteria.UserOrganizationId)
                                            .OrderByDescending(x => x.DateUpdated);
                                }
                                break;
                            case 3: // All users of all organizations can access all data 
                                SurveyResponseList = Context.SurveyResponses.Where(
                                   x => x.SurveyId == Id
                                        && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                                        && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                                        && x.StatusId >= 1)
                                        .OrderByDescending(x => x.DateUpdated);
                                break;
                            default:
                                SurveyResponseList = Context.SurveyResponses.Where(
                              x => x.SurveyId == Id
                                    && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
                                    && (x.RelateParentId == null || x.RelateParentId == Guid.Empty)
                                    && x.StatusId >= 1 && x.OrganizationId == criteria.UserOrganizationId)
                                    .OrderByDescending(x => x.DateUpdated);
                                break;
                        }
                    }
#endif //ImplementSharableRules
                }
                else
                {
                    var gridFields = criteria.FieldDigestList ?? new Dictionary<int, FieldDigest>();
                    
                    var surveyResponses = _surveyPersistenceFacade.GetAllResponsesContainingFields(gridFields);
                    if (surveyResponses != null)
                    {
                        var responseList = surveyResponses.Skip((criteria.PageNumber - 1) * criteria.GridPageSize).Take(criteria.GridPageSize);
                        result = responseList.Select(r => r.ToSurveyResponseBO()).ToList();
                    }
                }

                //SurveyResponseList = SurveyResponseList.Skip((criteria.PageNumber - 1) * criteria.PageSize).Take(criteria.PageSize);

                //foreach (SurveyResponse Response in SurveyResponseList)
                //{
                //    result.Add(Mapper.Map(Response, Response.Users.First()));
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

#if IncludeEpi7Compatibilty
        /// <summary>
        /// Builds SQL Select query by reading the columns, tablename from the EWE database.
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        //private string BuildEI7Query(string FormId, string SortOrder, string Sortfield, string EI7Connectionstring, string SearchCriteria = "", bool IsReadingResponseCount = false,
        //    int gridPageSize = 1, int gridPageNumber = 1)
        //{
        //    SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);
        //    EweConnection.Open();

        //    SqlCommand EweCommand = new SqlCommand("usp_GetResponseFieldsInfo", EweConnection);//send formid for stored procedure to look for common columns between the two tables
        //    //Stored procedure that goes queries ResponseDisplaySettings and new table SurveyResonpseTranslate(skinny table) for a given FormId

        //    EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
        //    EweCommand.Parameters["@FormId"].Value = FormId.Trim();

        //    EweCommand.CommandType = CommandType.StoredProcedure;
        //    //EweCommand.CreateParameter(  EweCommand.Parameters.Add(new SqlParameter("FormId"), FormId);



        //    SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

        //    DataSet EweDS = new DataSet();

        //    try
        //    {
        //        EweDataAdapter.Fill(EweDS);
        //        EweConnection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        EweConnection.Close();
        //        throw ex;
        //    }
        //    SqlConnection EI7Connection = new SqlConnection(EI7Connectionstring);

        //    EI7Connection.Open();

        //    SqlCommand EI7Command = new SqlCommand(" SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + EweDS.Tables[0].Rows[0][1] + "'", EI7Connection);
        //    object eI7CommandExecuteScalar;
        //    try
        //    {
        //        eI7CommandExecuteScalar = EI7Command.ExecuteScalar();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //    if (EweDS == null || EweDS.Tables.Count == 0 || EweDS.Tables[0].Rows.Count == 0
        //        || eI7CommandExecuteScalar == null)
        //    {
        //        EI7Connection.Close();
        //        return string.Empty;
        //    }

        //    StringBuilder stringBuilder = new StringBuilder();
        //    StringBuilder tableNameBuilder = new StringBuilder();
        //    StringBuilder pagingQueryBuilder = new StringBuilder();

        //    StringBuilder cteSelectBuilder = new StringBuilder();

        //    StringBuilder sortBuilder = new StringBuilder(" ORDER BY ");
        //    if (Sortfield != null && SortOrder != null)
        //    {
        //        sortBuilder.Append(Sortfield + " " + SortOrder);
        //    }
        //    else
        //    {
        //        //sortBuilder.Append(EweDS.Tables[0].Rows[0]["ColumnName"]);
        //        sortBuilder.Append("LastSaveTime"); //default sort on lastsavetime 
        //    }


        //    stringBuilder.Append(" SELECT ROW_NUMBER() OVER( " + sortBuilder.ToString() + ") RowNumber," + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".LastSaveTime," + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId,");
        //    cteSelectBuilder.Append(" RowNumber, GlobalRecordId, LastSaveTime, ");
        //    // Builds the select part of the query.
        //    foreach (DataRow row in EweDS.Tables[0].Rows)
        //    {
        //        stringBuilder.Append(row["TableName"] + "." + row["ColumnName"] + ", ");
        //        cteSelectBuilder.Append(row["ColumnName"] + ", ");

        //    }
        //    stringBuilder.Remove(stringBuilder.Length - 2, 1);
        //    cteSelectBuilder.Remove(cteSelectBuilder.Length - 2, 1);

        //    stringBuilder.Append(" FROM ");
        //    //Following code gives distinct data values.
        //    DataView view = new DataView(EweDS.Tables[0]);
        //    DataTable TableNames = view.ToTable(true, "TableName");

        //    stringBuilder.Append(TableNames.Rows[0][0]);
        //    //Builds the JOIN part of the query.
        //    for (int i = 0; i < TableNames.Rows.Count - 1; i++)
        //    {
        //        if (i + 1 < TableNames.Rows.Count)
        //        {
        //            stringBuilder.Append(" INNER JOIN " + TableNames.Rows[i + 1]["TableName"]);
        //            stringBuilder.Append(" ON " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId =" + TableNames.Rows[i + 1]["TableName"] + ".GlobalRecordId");

        //        }
        //    }
        //    stringBuilder.Append(" INNER JOIN " + EweDS.Tables[0].Rows[0]["ViewTableName"] + " ON " + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId =" + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".GlobalRecordId");

        //    if (SearchCriteria != null && SearchCriteria.Length > 0)
        //    {
        //        SearchCriteria = " WHERE " + SearchCriteria;
        //    }


        //    pagingQueryBuilder.Append("WITH CTE AS (" + stringBuilder.ToString() + SearchCriteria + ")");

        //    if (IsReadingResponseCount)
        //    {
        //        pagingQueryBuilder.Append(" SELECT COUNT(*) AS RESPONSECOUNT FROM CTE");
        //        //return pagingQueryBuilder.ToString();
        //    }
        //    else
        //    {
        //        pagingQueryBuilder.Append(" SELECT " + cteSelectBuilder.ToString() + " FROM CTE");
        //    }


        //    StringBuilder whereClause = new StringBuilder(" WHERE 1=1");

        //    //if (SearchCriteria.Length > 0)
        //    //{
        //    //    whereClause.Append(" WHERE " + SearchCriteria);
        //    //}
        //    //else
        //    //{
        //    //    whereClause.Append(" WHERE  1 = 1 ");
        //    //}


        //    pagingQueryBuilder.Append(whereClause);

        //    if (!IsReadingResponseCount)
        //    {
        //        pagingQueryBuilder.Append(" AND RowNumber between " + (((gridPageNumber * gridPageSize) - (gridPageSize)) + 1) + " AND " + ((gridPageNumber * (gridPageSize))));
        //        pagingQueryBuilder.Append(sortBuilder.ToString());
        //    }




        //    return pagingQueryBuilder.ToString();
        //}

        /// <summary>
        /// Builds SQL Select query by reading the columns, tablename from the EWE database.
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        private string BuildEI7Query(
            string FormId,
            string SortOrder,
            string Sortfield,
            string EI7Connectionstring,
            string SearchCriteria = "",
            bool IsReadingResponseCount = false,
            int gridPageSize = 1,
            int gridPageNumber = 1,
            bool IsChild = false,
            string ResponseId = "",
            int UserId = -1,
            bool IsShareable = false,
            int UserOrgId = -1,
            int DataAccessRulrId = -1

            )
        {
            SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);
            EweConnection.Open();

            SqlCommand EweCommand = new SqlCommand("usp_GetResponseFieldsInfo", EweConnection);//send formid for stored procedure to look for common columns between the two tables
            //Stored procedure that goes queries ResponseDisplaySettings and new table SurveyResonpseTranslate(skinny table) for a given FormId

            EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
            EweCommand.Parameters["@FormId"].Value = FormId.Trim();

            EweCommand.CommandType = CommandType.StoredProcedure;
            //EweCommand.CreateParameter(  EweCommand.Parameters.Add(new SqlParameter("FormId"), FormId);



            SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

            DataSet EweDS = new DataSet();

            try
            {
                EweDataAdapter.Fill(EweDS);
                EweConnection.Close();
            }
            catch (Exception ex)
            {
                EweConnection.Close();
                throw ex;
            }
            SqlConnection EI7Connection = new SqlConnection(EI7Connectionstring);

            EI7Connection.Open();
            SqlCommand EI7Command;
            try
            {
                EI7Command = new SqlCommand(" SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + EweDS.Tables[0].Rows[0][1] + "'", EI7Connection);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            object eI7CommandExecuteScalar;
            try
            {
                eI7CommandExecuteScalar = EI7Command.ExecuteScalar();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            if (EweDS == null || EweDS.Tables.Count == 0 || EweDS.Tables[0].Rows.Count == 0
                || eI7CommandExecuteScalar == null)
            {
                EI7Connection.Close();
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder tableNameBuilder = new StringBuilder();
            StringBuilder pagingQueryBuilder = new StringBuilder();

            StringBuilder cteSelectBuilder = new StringBuilder();

            StringBuilder sortBuilder = new StringBuilder(" ORDER BY ");
            if (Sortfield != null && SortOrder != null)
            {
                sortBuilder.Append(Sortfield + " " + SortOrder);
            }
            else
            {
                //sortBuilder.Append(EweDS.Tables[0].Rows[0]["ColumnName"]);
                sortBuilder.Append(" LastSaveTime DESC "); //default sort on lastsavetime 
            }


            stringBuilder.Append(" SELECT ROW_NUMBER() OVER( " + sortBuilder.ToString() + ") RowNumber," + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".LastSaveTime," + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId,");
            cteSelectBuilder.Append(" RowNumber, GlobalRecordId, LastSaveTime, ");
            // Builds the select part of the query.
            foreach (DataRow row in EweDS.Tables[0].Rows)
            {
                stringBuilder.Append(row["TableName"] + ".[" + row["ColumnName"] + "], ");
                cteSelectBuilder.Append("[" + row["ColumnName"] + "], ");

            }
            stringBuilder.Remove(stringBuilder.Length - 2, 1);
            cteSelectBuilder.Remove(cteSelectBuilder.Length - 2, 1);

            stringBuilder.Append(" FROM ");
            //Following code gives distinct data values.
            DataView view = new DataView(EweDS.Tables[0]);
            DataTable TableNames = view.ToTable(true, "TableName");

            stringBuilder.Append(TableNames.Rows[0][0]);
            //Builds the JOIN part of the query.
            for (int i = 0; i < TableNames.Rows.Count - 1; i++)
            {
                if (i + 1 < TableNames.Rows.Count)
                {
                    stringBuilder.Append(" INNER JOIN " + TableNames.Rows[i + 1]["TableName"]);
                    stringBuilder.Append(" ON " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId =" + TableNames.Rows[i + 1]["TableName"] + ".GlobalRecordId");

                }
            }
            stringBuilder.Append(" INNER JOIN " + EweDS.Tables[0].Rows[0]["ViewTableName"] + " ON " + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId =" + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".GlobalRecordId");

            stringBuilder.Append(" WHERE RECSTATUS = 1 ");
            //  User filter Start 

            // if (ConfigurationManager.AppSettings["FilterByUser"].ToUpper() == "TRUE" && UserId != -1)
            //{
            //    stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
            //    stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
            //    stringBuilder.Append(" INNER JOIN [" + EweConnection.Database + "].[dbo].SurveyResponseUser on SurveyResponse.ResponseId =SurveyResponseUser.ResponseId");
            //    stringBuilder.Append(" Where " + "UserId =" + UserId +")");
            //}

            if (IsShareable)
            {
                //if (_dataAccessRuleId != -1)
                //{
                //    stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                //    stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                //    //stringBuilder.Append(" INNER JOIN [" + EweConnection.Database + "].[dbo].SurveyResponseUser on SurveyResponse.ResponseId =SurveyResponseUser.ResponseId and SurveyResponse.SurveyId ='" + FormId+"'");
                //    //stringBuilder.Append(" INNER JOIN [" + EweConnection.Database + "].[dbo].UserOrganization on UserOrganization.UserID = SurveyResponseUser.UserId");
                //    //stringBuilder.Append(" Where " + "UserOrganization.OrganizationID =" + UserOrgId + ")");
                //    stringBuilder.Append(" Where " + "OrganizationId =" + UserOrgId + " And SurveyId ='" + FormId + "')");
                //}
                //else {

                //    stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                //    stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                //    stringBuilder.Append(" Where  SurveyId ='" + FormId + "')");

                //   }

                //Shareable
                switch (_dataAccessRuleId)
                {
                    case 1: //   Organization users can only access the data of there organization
                        stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                        stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                        stringBuilder.Append(" Where " + "OrganizationId =" + UserOrgId + " And SurveyId ='" + FormId + "')");
                        break;
                    case 2:    // All users in host organization will have access to all data of all organizations  

                        // get All the users of Host organization
                        var Context = DataObjectFactory.CreateContext();

                        Guid FormGuid = new Guid(FormId);
                        var HostOrg = Context.SurveyMetaDatas.Where(x => x.SurveyId == FormGuid).SingleOrDefault().OrganizationId;

                        var Users = Context.UserOrganizations.Where(x => x.OrganizationID == HostOrg && x.Active == true).ToList();

                        int Count = Users.Where(x => x.UserID == UserId).Count();
                        if (Count > 0)
                        {
                            stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                            stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                            stringBuilder.Append(" Where  SurveyId ='" + FormId + "')");
                        }
                        else
                        {
                            stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                            stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                            stringBuilder.Append(" Where " + "OrganizationId =" + UserOrgId + " And SurveyId ='" + FormId + "')");

                        }
                        break;
                    case 3: // All users of all organizations can access all data 
                        stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                        stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                        stringBuilder.Append(" Where  SurveyId ='" + FormId + "')");
                        break;
                    default:
                        stringBuilder.Append(" AND " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId in ");
                        stringBuilder.Append(" (Select SurveyResponse.ResponseId from [" + EweConnection.Database + "].[dbo].SurveyResponse");
                        stringBuilder.Append(" Where " + "OrganizationId =" + UserOrgId + " And SurveyId ='" + FormId + "')");
                        break;



                }
            }

            //User filter End 

            if (SearchCriteria != null && SearchCriteria.Length > 0)
            {
                stringBuilder.Append(" AND " + SearchCriteria);
            }

            if (IsChild)
            {
                stringBuilder.Append(" AND " + EweDS.Tables[0].Rows[0][4] + ".FKEY ='" + ResponseId + "'");

            }


            pagingQueryBuilder.Append("WITH CTE AS (" + stringBuilder.ToString() + ")");

            if (IsReadingResponseCount)
            {
                pagingQueryBuilder.Append(" SELECT COUNT(*) AS RESPONSECOUNT FROM CTE");
                //return pagingQueryBuilder.ToString();
            }
            else
            {
                pagingQueryBuilder.Append(" SELECT " + cteSelectBuilder.ToString() + " FROM CTE");
            }


            StringBuilder whereClause = new StringBuilder(" WHERE 1=1");

            pagingQueryBuilder.Append(whereClause);

            if (!IsReadingResponseCount && !IsChild)
            {
                pagingQueryBuilder.Append(" AND RowNumber between " + (((gridPageNumber * gridPageSize) - (gridPageSize)) + 1) + " AND " + ((gridPageNumber * (gridPageSize))));
                pagingQueryBuilder.Append(sortBuilder.ToString());
            }




            return pagingQueryBuilder.ToString();
        }
        /// <summary>
        /// Builds SQL Select query by reading the columns, tablename from the EWE database.
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        //private string BuildEI7ResponseQuery(string ResponseId, string SurveyId, string SortOrder, string Sortfield, string EI7Connectionstring, bool IsUsedForCount = false)
        //{
        //    SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);
        //    EweConnection.Open();

        //    SqlCommand EweCommand = new SqlCommand("usp_GetResponseFieldsInfo", EweConnection);//send formid for stored procedure to look for common columns between the two tables
        //    //Stored procedure that goes queries ResponseDisplaySettings and new table SurveyResonpseTranslate(skinny table) for a given FormId

        //    EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
        //    EweCommand.Parameters["@FormId"].Value = SurveyId.Trim();

        //    EweCommand.CommandType = CommandType.StoredProcedure;
        //    //EweCommand.CreateParameter(  EweCommand.Parameters.Add(new SqlParameter("FormId"), FormId);



        //    SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

        //    DataSet EweDS = new DataSet();

        //    try
        //    {
        //        EweDataAdapter.Fill(EweDS);
        //        EweConnection.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        EweConnection.Close();
        //        throw ex;
        //    }
        //    SqlConnection EI7Connection = new SqlConnection(EI7Connectionstring);

        //    EI7Connection.Open();

        //    SqlCommand EI7Command = new SqlCommand(" SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + EweDS.Tables[0].Rows[0][1] + "'", EI7Connection);
        //    object eI7CommandExecuteScalar;
        //    try
        //    {
        //        eI7CommandExecuteScalar = EI7Command.ExecuteScalar();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //    if (EweDS == null || EweDS.Tables.Count == 0 || EweDS.Tables[0].Rows.Count == 0
        //        || eI7CommandExecuteScalar == null)
        //    {
        //        EI7Connection.Close();
        //        return string.Empty;
        //    }

        //    StringBuilder stringBuilder = new StringBuilder();
        //    StringBuilder tableNameBuilder = new StringBuilder();
        //    StringBuilder sortBuilder = new StringBuilder(" ORDER BY ");
        //    if (IsUsedForCount)
        //    {
        //        stringBuilder.Append(" SELECT COUNT(*), ");
        //    }
        //    else
        //    {
        //        stringBuilder.Append(" SELECT " + EweDS.Tables[0].Rows[0][1] + ".GlobalRecordId,");
        //        if (Sortfield != null && SortOrder != null)
        //        {
        //            sortBuilder.Append(Sortfield + " " + SortOrder);
        //        }
        //        else
        //        {
        //            sortBuilder.Append(EweDS.Tables[0].Rows[0][1] + "." + EweDS.Tables[0].Rows[0][0]);
        //        }
        //        // Builds the select part of the query.
        //        foreach (DataRow row in EweDS.Tables[0].Rows)
        //        {
        //            stringBuilder.Append(row[1] + "." + row[0] + ", ");

        //        }
        //    }






        //    stringBuilder.Remove(stringBuilder.Length - 2, 1);

        //    stringBuilder.Append(" FROM ");
        //    //Following code gives distinct data values.
        //    DataView view = new DataView(EweDS.Tables[0]);
        //    DataTable TableNames = view.ToTable(true, "TableName");

        //    stringBuilder.Append(TableNames.Rows[0][0]);
        //    //Builds the JOIN part of the query.
        //    for (int i = 0; i < TableNames.Rows.Count - 1; i++)
        //    {
        //        if (i + 1 < TableNames.Rows.Count)
        //        {
        //            stringBuilder.Append(" INNER JOIN " + TableNames.Rows[i + 1][0]);
        //            stringBuilder.Append(" ON " + TableNames.Rows[0][0] + ".GlobalRecordId =" + TableNames.Rows[i + 1][0] + ".GlobalRecordId");

        //        }
        //    }

        //    stringBuilder.Append(" INNER JOIN " + EweDS.Tables[0].Rows[0][4] + " ON " + EweDS.Tables[0].Rows[0][1] + ".GlobalRecordId =" + EweDS.Tables[0].Rows[0][4] + ".GlobalRecordId");
        //    stringBuilder.Append(" WHERE " + EweDS.Tables[0].Rows[0][4] + ".FKEY ='" + ResponseId + "'");

        //    if (IsUsedForCount)
        //    {
        //        return stringBuilder.ToString();
        //    }


        //    return stringBuilder.Append(sortBuilder.ToString()).ToString();
        //}

        /// <summary>
        /// Builds SQL Select query by reading the columns, tablename from the EWE database.
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        private string BuildEI7ResponseAllFieldsQuery(string ResponseId, string SurveyId, string EI7Connectionstring, int UserId)
        {
            SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);
            EweConnection.Open();

            SqlCommand EweCommand = new SqlCommand("usp_GetResponseAllFieldsInfo", EweConnection);//Gets all the fields for given survey.

            EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
            EweCommand.Parameters["@FormId"].Value = SurveyId.Trim();

            EweCommand.CommandType = CommandType.StoredProcedure;
            //EweCommand.CreateParameter(  EweCommand.Parameters.Add(new SqlParameter("FormId"), FormId);



            SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

            DataSet EweDS = new DataSet();

            try
            {
                EweDataAdapter.Fill(EweDS);
                EweConnection.Close();
            }
            catch (Exception ex)
            {
                EweConnection.Close();
                throw ex;
            }
            SqlConnection EI7Connection = new SqlConnection(EI7Connectionstring);

            EI7Connection.Open();

            SqlCommand EI7Command = new SqlCommand(" SELECT *  FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + EweDS.Tables[0].Rows[0][1] + "'", EI7Connection);
            object eI7CommandExecuteScalar;
            try
            {
                eI7CommandExecuteScalar = EI7Command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (EweDS == null || EweDS.Tables.Count == 0 || EweDS.Tables[0].Rows.Count == 0
                || eI7CommandExecuteScalar == null)
            {
                EI7Connection.Close();
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder tableNameBuilder = new StringBuilder();
            stringBuilder.Append(" SELECT " + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId,");



            // Builds the select part of the query.
            foreach (DataRow row in EweDS.Tables[0].Rows)
            {
                stringBuilder.Append(row["TableName"] + "." + row["FieldName"] + ", ");

            }
            stringBuilder.Remove(stringBuilder.Length - 2, 1);

            stringBuilder.Append(" FROM ");
            //Following code gives distinct data values.
            DataView view = new DataView(EweDS.Tables[0]);
            DataTable TableNames = view.ToTable(true, "TableName");

            stringBuilder.Append(TableNames.Rows[0]["TableName"]);
            //Builds the JOIN part of the query.
            for (int i = 0; i < TableNames.Rows.Count - 1; i++)
            {
                if (i + 1 < TableNames.Rows.Count)
                {
                    stringBuilder.Append(" INNER JOIN " + TableNames.Rows[i + 1]["TableName"]);
                    stringBuilder.Append(" ON " + TableNames.Rows[0]["TableName"] + ".GlobalRecordId =" + TableNames.Rows[i + 1]["TableName"] + ".GlobalRecordId");

                }
            }

            //stringBuilder.Append(" INNER JOIN " + EweDS.Tables[0].Rows[0]["ViewTableName"] + " ON " + EweDS.Tables[0].Rows[0][1] + ".GlobalRecordId =" + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".GlobalRecordId");
            //stringBuilder.Append(" WHERE " + EweDS.Tables[0].Rows[0]["ViewTableName"] + ".FKEY ='" + ResponseId + "'");
            stringBuilder.Append(" WHERE " + EweDS.Tables[0].Rows[0]["TableName"] + ".GlobalRecordId ='" + ResponseId + "'");

            return stringBuilder.ToString();
        }




        /// <summary>
        /// Validates if current form is Sql Project
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        private bool IsEISQLProject(string FormId)
        {
            //SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);

            //EweConnection.Open();

            //SqlCommand EweCommand = new SqlCommand("usp_IsSQLProject", EweConnection);
            //EweCommand.CommandType = CommandType.StoredProcedure;
            //EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
            //EweCommand.Parameters["@FormId"].Value = FormId;


            //SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

            //bool IsSqlProj = false;
            //try
            //    {
            //    object issqlprj = EweDataAdapter.SelectCommand.ExecuteScalar();

            //    if (issqlprj != DBNull.Value)
            //        {
            //        IsSqlProj = Convert.ToBoolean(issqlprj);
            //        }


            //    EweConnection.Close();
            //    }
            //catch (Exception ex)
            //    {
            //    EweConnection.Close();
            //    throw ex;
            //    }

            bool IsSqlProj = false;
            Guid Id = new Guid(FormId);

            using (var Context = DataObjectFactory.CreateContext())
            {


                var Response = Context.SurveyMetaDatas.Single(x => x.SurveyId == Id);
                if (Response != null)
                {
                    IsSqlProj = (bool)Response.IsSQLProject;

                }

            }
            return IsSqlProj;
        }

        /// <summary>
        /// Reads connection string from Datasource table
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        private string ReadEI7DatabaseName(string FormId)
        {
            SqlConnection EweConnection = new SqlConnection(DataObjectFactory.EWEADOConnectionString);

            EweConnection.Open();

            SqlCommand EweCommand = new SqlCommand("usp_GetDatasourceConnectionString", EweConnection);
            EweCommand.CommandType = CommandType.StoredProcedure;
            EweCommand.Parameters.Add("@FormId", SqlDbType.VarChar);
            EweCommand.Parameters["@FormId"].Value = FormId;
            //EweCommand.Parameters["@FormId"].Value = FormId;

            SqlDataAdapter EweDataAdapter = new SqlDataAdapter(EweCommand);

            string ConnectionString;
            try
            {
                // EweDataAdapter.Fill(DSConnstr);
                ConnectionString = Convert.ToString(EweCommand.ExecuteScalar());
                EweConnection.Close();
            }
            catch (Exception ex)
            {
                EweConnection.Close();
                throw ex;
            }

            //ConnectionString = DSConnstr.Tables[0].Rows[0][0] + "";

            //return ConnectionString;

            return ConnectionString.Substring(ConnectionString.LastIndexOf('=') + 1);
        }
        public bool DoesResponseExist(Guid ResponseId)
        {
            bool Exists = false;

            try
            {



                using (var Context = DataObjectFactory.CreateContext())
                {

                    IEnumerable<SurveyResponse> SurveyResponseList = Context.SurveyResponses.Where(x => x.ResponseId == ResponseId);
                    if (SurveyResponseList.Count() > 0)
                    {
                        Exists = true;
                    }

                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Exists;
        }

        public bool HasResponse(SurveyAnswerCriteria Criteria)
        {
            bool Exists = false;
            IsSqlProject = IsEISQLProject(Criteria.SurveyId);
            if (IsSqlProject)
            {
                string tableName = ReadEI7DatabaseName(Criteria.SurveyId);

                string EI7ConnectionString = DataObjectFactory.EWEADOConnectionString.Substring(0, DataObjectFactory.EWEADOConnectionString.LastIndexOf('=')) + "=" + tableName;

                SqlConnection EI7Connection = new SqlConnection(EI7ConnectionString);

                //string EI7Query = BuildEI7ResponseQuery(Criteria.surveyAnswerIdList[0], Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString, true);

                string EI7Query = BuildEI7Query(Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString, "", true, 1, 1, true, Criteria.surveyAnswerIdList[0], Criteria.UserId, Criteria.IsShareable, Criteria.UserOrganizationId);

                SqlCommand EI7Command = new SqlCommand(EI7Query, EI7Connection);
                EI7Command.CommandType = CommandType.Text;


                EI7Connection.Open();

                try
                {
                    int count = (int)EI7Command.ExecuteScalar();

                    EI7Connection.Close();
                    if (count > 0)
                    {
                        Exists = true;
                    }


                }
                catch (Exception)
                {
                    EI7Connection.Close();
                    throw;
                }


            }
            else
            {
                try
                {



                    using (var Context = DataObjectFactory.CreateContext())
                    {

                        IQueryable<SurveyResponse> SurveyResponseList = Context.SurveyResponses.Where(x => x.ResponseId == new Guid(Criteria.surveyAnswerIdList[0]));
                        if (SurveyResponseList.Count() > 0)
                        {
                            Exists = true;
                        }

                    }

                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            return Exists;
        }
        public int GetFormResponseCount(string FormId)
        {
            int ResponseCount = 0;

            //If SqlProject read responses from property SqlProjectResponsesCount.
            IsSqlProject = IsEISQLProject(FormId);
            if (IsSqlProject)
            {
                //ResponseCount = SqlProjectResponsesCount;


                string tableName = ReadEI7DatabaseName(FormId);

                string EI7ConnectionString = DataObjectFactory.EWEADOConnectionString.Substring(0, DataObjectFactory.EWEADOConnectionString.LastIndexOf('=')) + "=" + tableName;

                SqlConnection EI7Connection = new SqlConnection(EI7ConnectionString);

                string EI7Query = BuildEI7Query(FormId, null, null, EI7ConnectionString, "", true);

                SqlCommand EI7Command = new SqlCommand(EI7Query, EI7Connection);
                EI7Command.CommandType = CommandType.Text;

                EI7Connection.Open();

                try
                {
                    ResponseCount = (int)EI7Command.ExecuteScalar();
                    EI7Connection.Close();
                }
                catch (Exception)
                {
                    EI7Connection.Close();
                    throw;
                }
            }
            else
            {


                try
                {

                    Guid Id = new Guid(FormId);

                    using (var Context = DataObjectFactory.CreateContext())
                    {

                        IQueryable<SurveyResponse> SurveyResponseList = Context.SurveyResponses.Where(x => x.SurveyId == Id
                            // && string.IsNullOrEmpty(x.ParentRecordId.ToString()) == true 
                            && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)

                            && x.StatusId > 1);
                        ResponseCount = SurveyResponseList.Count();

                    }

                }
                catch (Exception ex)
                {
                    throw (ex);
                }


            }
            return ResponseCount;


        }
#endif //IncludeEpi7Compatibilty



        public SurveyResponseBO GetFormResponseByResponseId(string ResponseId)
        {
            // TODO: DocumentDB implementation required
            SurveyResponseBO result = new SurveyResponseBO();

            try
            {
                //Guid Id = new Guid(ResponseId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    SurveyResponse Response = Context.SurveyResponses.Where(x => x.ResponseId == Id).First();
                //    result = (Mapper.Map(Response));
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

        public string GetResponseParentId(string responseId)
        {
            // TODO: DocumentDB implementation required
            SurveyResponseBO result = new SurveyResponseBO();

            try
            {
                //Guid Id = new Guid(responseId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    SurveyResponse Response = Context.SurveyResponses.Where(x => x.ResponseId == Id).First();
                //    result = (Mapper.Map(Response));
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            if (!string.IsNullOrEmpty(result.ParentRecordId))
            {
                return result.ParentRecordId;
            }
            else
            {
                return "";
            }

        }

        public SurveyResponseBO GetSingleResponse(string responseId)
        {
            // TODO: DocumentDB implementation required
            var formResponseDetail = _surveyPersistenceFacade.GetFormResponseByResponseId(responseId);
            var result = formResponseDetail.ToSurveyResponseBO();

            //    try
            //    {
            //        //Guid Id = new Guid(responseId);

            //        //using (var Context = DataObjectFactory.CreateContext())
            //        //{
            //        //    var Response = Context.SurveyResponses.Where(x => x.ResponseId == Id);//.First();
            //        //    if (Response.Count() > 0)
            //        //    {
            //        //        result = (Mapper.Map(Response.First()));
            //        //    }
            //        //}
            //    }
            //    catch (Exception ex)
            //    {
            //        throw (ex);
            //    }
            return result;
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string rootResponseId)
        {
            List<SurveyResponseBO> result = null;

            List<string> list = new List<string>();
            try
            {
                var formResponseDetail = _surveyPersistenceFacade.GetHierarchialResponsesByResponseId(rootResponseId);

				//var json = Newtonsoft.Json.JsonConvert.SerializeObject(formResponseDetail);
				//var temp = Newtonsoft.Json.JsonConvert.DeserializeObject<FormResponseDetail>(json);

				result = formResponseDetail.FlattenHierarchy().Select(d => d.ToSurveyResponseBO()).ToList();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

        public SurveyResponseBO GetFormResponseByParentRecordId(string parentRecordId)
        {
            // TODO: DocumentDB implementation required
            SurveyResponseBO result = new SurveyResponseBO();

            try
            {

                Guid Id = new Guid(parentRecordId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    var Response = Context.SurveyResponses.Where(x => x.ParentRecordId == Id);
                //    if (Response.Count() > 0)
                //    {
                //        result = (Mapper.Map(Response.Single()));
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }

        public List<SurveyResponseBO> GetAncestorResponseIdsByChildId(string childId)
        {
            // TODO: DocumentDB implementation required
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            List<string> list = new List<string>();
            try
            {

                Guid Id = new Guid(childId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    IQueryable<SurveyResponse> Query = Context.SurveyResponses.Where(x => x.ResponseId == Id).Traverse(x => Context.SurveyResponses.Where(y => x.RelateParentId == y.ResponseId)).AsQueryable();
                //    result = Mapper.Map(Query);
                //}

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string responseId, string surveyId)
        {
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            // TODO: DocumentDB implementation required
            try
            {
                //Guid RId = new Guid(responseId);
                //Guid SId = new Guid(surveyId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    result = Mapper.Map(Context.SurveyResponses.Where(x => x.RelateParentId == RId && x.SurveyId == SId)).OrderBy(x => x.DateCreated).ToList();
                //}

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return result;
        }


        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string ResponseId, SurveyAnswerCriteria Criteria)
        {
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();

            // TODO: DocumentDB implementation required

#if IncludeEpi7Compatibilty
            IsSqlProject = IsEISQLProject(Criteria.SurveyId);//Checks to see if current form is SqlProject

            if (IsSqlProject)
            {
                //make a connection to datasource table to read the connection string.
                //do a read to see which column belongs to which page/table.
                //do a read from ResponseDisplaySettings to read the column names. if for a given survey they dont exist 
                //read the first 5 columns from EI7 sql server database.

                string tableName = ReadEI7DatabaseName(Criteria.SurveyId);

                string EI7ConnectionString = DataObjectFactory.EWEADOConnectionString.Substring(0, DataObjectFactory.EWEADOConnectionString.LastIndexOf('=')) + "=" + tableName;

                SqlConnection EI7Connection = new SqlConnection(EI7ConnectionString);

                //string EI7Query = BuildEI7ResponseQuery(ResponseId, Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString);

                //  string EI7Query = BuildEI7Query(Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString, "", false, 1, 1, true, ResponseId);
                string EI7Query = BuildEI7Query(Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString, "", false, 1, 1, true, ResponseId, -1, Criteria.IsShareable, Criteria.UserOrganizationId, _dataAccessRuleId);


                SqlCommand EI7Command = new SqlCommand(EI7Query, EI7Connection);
                EI7Command.CommandType = CommandType.Text;

                SqlDataAdapter EI7Adapter = new SqlDataAdapter(EI7Command);

                DataSet EI7DS = new DataSet();

                EI7Connection.Open();

                try
                {
                    EI7Adapter.Fill(EI7DS);
                    EI7Connection.Close();
                }
                catch (Exception)
                {
                    EI7Connection.Close();
                    throw;
                }


                // List<Dictionary<string, string>> DataRows = new List<Dictionary<string, string>>();

                for (int i = 0; i < EI7DS.Tables[0].Rows.Count; i++)
                {
                    Dictionary<string, string> rowDic = new Dictionary<string, string>();
                    SurveyResponseBO SurveyResponseBO = new Enter.Common.BusinessObject.SurveyResponseBO();
                    for (int j = 0; j < EI7DS.Tables[0].Columns.Count; j++)
                    {
                        rowDic.Add(EI7DS.Tables[0].Columns[j].ColumnName, EI7DS.Tables[0].Rows[i][j].ToString());
                    }
                    //.Skip((gridPageNumber - 1) * gridPageSize).Take(gridPageSize); ;
                    //IEnumerable<KeyValuePair<string, string>> temp = rowDic.AsEnumerable();
                    //temp.Skip((gridPageNumber - 1) * gridPageSize).Take(gridPageSize); 

                    SurveyResponseBO.SqlData = rowDic;
                    result.Add(SurveyResponseBO);
                }

                //SqlProjectResponsesCount = EI7DS.Tables[0].Rows.Count;

                //result = result.Skip((Criteria.gridPageNumber - 1) * Criteria.gridPageSize).Take(Criteria.gridPageSize).ToList();
                //SurveyResponseBO.SqlResponseDataBO.SqlData = DataRows;
            }
            else
#endif //IncludeEpi7Compatibilty

            {

                // TODO: DocumentDB implementation required
                try
                {
                    //Guid RId = new Guid(ResponseId);
                    //Guid SId = new Guid(Criteria.SurveyId);

                    //using (var Context = DataObjectFactory.CreateContext())
                    //{
                    //    //result = Mapper.Map(Context.SurveyResponses.Where(x => x.RelateParentId == RId && x.SurveyId == SId)).OrderBy(x => x.DateCreated).ToList();

                    //    if (Criteria.IsShareable && _dataAccessRuleId == 1)
                    //    {
                    //        result = Mapper.Map(Context.SurveyResponses.Where(x => x.RelateParentId == RId && x.SurveyId == SId && x.OrganizationId == Criteria.UserOrganizationId)).OrderBy(x => x.DateCreated).ToList();
                    //    }
                    //    else
                    //    {
                    //        result = Mapper.Map(Context.SurveyResponses.Where(x => x.RelateParentId == RId && x.SurveyId == SId)).OrderBy(x => x.DateCreated).ToList();
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            return result;
        }


        /*
         * 
         * */

        public SurveyResponseBO GetResponse(string responseId)
        {
            // TODO: DocumentDB implementation required
            SurveyResponseBO result = new SurveyResponseBO();

            try
            {
                //Guid Id = new Guid(responseId);

                //using (var Context = DataObjectFactory.CreateContext())
                //{
                //    var Response = Context.ResponseXmls.Where(x => x.ResponseId == Id);
                //    if (Response.Count() > 0)
                //    {
                //        result = (Mapper.Map(Response.Single()));
                //    }
                //}

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }


        public void DeleteResponse(ResponseBO responseBO)
        {
            // TODO: DocumentDB implementation required

            //Guid Id = new Guid(responseBO.ResponseId);

            //using (var Context = DataObjectFactory.CreateContext())
            //{

            //    ResponseXml response = Context.ResponseXmls.FirstOrDefault(x => x.ResponseId == Id);

            //    if (response != null)
            //    {
            //        Context.ResponseXmls.DeleteObject(Response);
            //        Context.SaveChanges();
            //    }
            //}
        }

        public void UpdateRecordStatus(string responseId, int status, RecordStatusChangeReason reasonForStatusChange)
        {
			_surveyPersistenceFacade.UpdateResponseStatus(responseId, status, reasonForStatusChange);
		}

        public int GetFormResponseCount(SurveyAnswerCriteria criteria)
        {
            // TODO: DocumentDb implementation required
            int responseCount = criteria.FormResponseCount.HasValue ? criteria.FormResponseCount.Value : 0;

#if IncludeEpi7Compatibilty
            //If SqlProject read responses from property SqlProjectResponsesCount.
            IsSqlProject = IsEISQLProject(Criteria.SurveyId);
            if (IsSqlProject)
            {
                //ResponseCount = SqlProjectResponsesCount;


                string tableName = ReadEI7DatabaseName(Criteria.SurveyId);

                string EI7ConnectionString = DataObjectFactory.EWEADOConnectionString.Substring(0, DataObjectFactory.EWEADOConnectionString.LastIndexOf('=')) + "=" + tableName;

                SqlConnection EI7Connection = new SqlConnection(EI7ConnectionString);

                string EI7Query = BuildEI7Query(Criteria.SurveyId, Criteria.SortOrder, Criteria.Sortfield, EI7ConnectionString, Criteria.SearchCriteria, true, -1, -1, false, "", Criteria.UserId, Criteria.IsShareable, Criteria.UserOrganizationId, _dataAccessRuleId);

                SqlCommand EI7Command = new SqlCommand(EI7Query, EI7Connection);
                EI7Command.CommandType = CommandType.Text;

                EI7Connection.Open();

                try
                {
                    if (!string.IsNullOrEmpty(EI7Query))
                        ResponseCount = (int)EI7Command.ExecuteScalar();
                    EI7Connection.Close();
                }
                catch (Exception)
                {
                    EI7Connection.Close();
                    throw;
                }
            }
            else
#endif //IncludeEpi7Compatibilty
            //{
            //    try
            //    {

            //        Guid Id = new Guid(Criteria.SurveyId);
            //        IQueryable<SurveyResponse> SurveyResponseList;
            //        using (var Context = DataObjectFactory.CreateContext())
            //        {
            //            if (Criteria.IsShareable && _dataAccessRuleId == 1)
            //            {
            //                SurveyResponseList = Context.SurveyResponses.Where(x => x.SurveyId == Id
            //                    && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)

            //                    && x.StatusId >= 1
            //                    && x.OrganizationId == Criteria.UserOrganizationId);
            //            }
            //            else
            //            {

            //                SurveyResponseList = Context.SurveyResponses.Where(x => x.SurveyId == Id
            //                    && (x.ParentRecordId == null || x.ParentRecordId == Guid.Empty)
            //                    && x.StatusId >= 1);



            //            }
            //            ResponseCount = SurveyResponseList.Count();

            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        throw (ex);
            //    }
            //}
            return responseCount;
        }
        public void UpdateRecordStatus(SurveyResponseBO surveyResponse)
        {
            try
            {
                _surveyPersistenceFacade.UpdateResponseStatus(surveyResponse.ResponseId, surveyResponse.Status, surveyResponse.ReasonForStatusChange);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int GetDataAccessRule(string formId, int userId)
        {
            var formDigest = GetFormDigest(formId);
            int ruleId = formDigest != null ? formDigest.DataAccessRuleId : 0;
            return ruleId;
        }

        public int GetFormResponseCount(string formId)
        {
            return _surveyPersistenceFacade.GetFormResponseCount(formId);
        }

        public bool DoChildrenExistForResponseId(Guid responseId)
        {
            var responseExists = _surveyPersistenceFacade.DoChildrenExistForResponseId(responseId.ToString());
            return responseExists;
        }

        public bool HasResponse(SurveyAnswerCriteria criteria)
        {
            var responseId = criteria.SurveyAnswerIdList[0];
            var responseExists = _surveyPersistenceFacade.DoChildrenExistForResponseId(responseId);
            return responseExists;
        }

        public void UpdatePassCode(UserAuthenticationRequestBO passcodeBO)
        {
            // TODO: DocumentDB implementation required
            return;
            throw new NotImplementedException();
        }

        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO)
        {
            // TODO: Implement this correctly
            var userAuthenticationResponseBO = new UserAuthenticationResponseBO { PassCode = passcodeBO.PassCode, ResponseId = passcodeBO.ResponseId };
            return userAuthenticationResponseBO;
        }

#if false
        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponse(List<string> SurveyResponseIdList, Guid UserPublishKey, int gridPageNumber, int gridPageSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponseSize(List<string> SurveyResponseIdList, Guid UserPublishKey, int gridPageNumber, int gridPageSize, int ResponseMaxSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponseBySurveyId(List<string> SurveyIdList, Guid UserPublishKey, int gridPageNumber, int gridPageSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponseBySurveyIdSize(List<string> SurveyIdList, Guid UserPublishKey, int gridPageNumber, int gridPageSize, int ResponseMaxSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponse(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode, int statusId, int gridPageNumber, int gridPageSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetSurveyResponseSize(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode, int statusId, int gridPageNumber, int gridPageSize, int ResponseMaxSize)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetFormResponseByFormId(string formId, int gridPageNumber, int gridPageSize)
        {
            var eweResponse = eweEntitySurveyResponseDao.GetFormResponseByFormId(formId, gridPageNumber, gridPageSize);
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return eweResponse;
            }
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetFormResponseByFormId(SurveyAnswerCriteria criteria)
        {
            var eweResponse = eweEntitySurveyResponseDao.GetFormResponseByFormId(criteria);
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return eweResponse;
            }
        }

        Web.Enter.Common.BusinessObject.SurveyResponseBO ISurveyResponseDao.GetSingleResponse(string ResponseId)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetResponsesHierarchyIdsByRootId(string RootId)
        {
            var eweResponse = eweEntitySurveyResponseDao.GetResponsesHierarchyIdsByRootId(RootId);
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return eweResponse;
            }
        }

        Web.Enter.Common.BusinessObject.SurveyResponseBO ISurveyResponseDao.GetFormResponseByParentRecordId(string ResponseId)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetAncestorResponseIdsByChildId(string ChildId)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetResponsesByRelatedFormId(string ResponseId, string SurveyId)
        {
            throw new NotImplementedException();
        }

        List<Web.Enter.Common.BusinessObject.SurveyResponseBO> ISurveyResponseDao.GetResponsesByRelatedFormId(string ResponseId, SurveyAnswerCriteria Criteria)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
