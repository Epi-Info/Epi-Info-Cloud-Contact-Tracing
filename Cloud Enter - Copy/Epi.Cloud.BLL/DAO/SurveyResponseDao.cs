using System;
using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Criteria;
using Epi.Web.Enter.Interfaces.DataInterfaces;

namespace Epi.Cloud.BLL.DAO
{
    public class SurveyResponseDao : ISurveyResponseDao
    {
        public void DeleteSingleSurveyResponse(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
        }

        public void DeleteSurveyResponse(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
        }

        public void DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetAncestorResponseIdsByChildId(string childId)
        {
            throw new NotImplementedException();
        }

        public int GetDataAccessRule(string formId, int UserId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetFormResponseByFormId(SurveyAnswerCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetFormResponseByFormId(string formId, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public SurveyResponseBO GetFormResponseByParentRecordId(string responseId)
        {
            throw new NotImplementedException();
        }

        public int GetFormResponseCount(SurveyAnswerCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public int GetFormResponseCount(string formId)
        {
            throw new NotImplementedException();
        }

        public SurveyResponseBO GetResponse(string responseId)
        {
            throw new NotImplementedException();
        }

        public string GetResponseParentId(string responseId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string responseId, SurveyAnswerCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetResponsesByRelatedFormId(string responseId, string surveyId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string rootId)
        {
            throw new NotImplementedException();
        }

        public SurveyResponseBO GetSingleResponse(string responseId)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponse(List<string> surveyResponseIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponse(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int pageNumber = -1, int pageSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseBySurveyId(List<string> surveyIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseBySurveyIdSize(List<string> surveyIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyResponseIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1)
        {
            throw new NotImplementedException();
        }

        public bool HasResponse(SurveyAnswerCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public void InsertChildSurveyResponse(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
        }

        public void InsertSurveyResponse(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
#if WebEnterCode
            try
            {
                using (var Context = DataObjectFactory.CreateContext())
                {
                    SurveyResponse SurveyResponseEntity = new EF.SurveyResponse();
                    //   var _UserOrg  = Context.UserOrganizations.Where(x => x.UserID == SurveyResponse.UserId).First();
                    if (SurveyResponse.CurrentOrgId > 0)
                    {
                        SurveyResponseEntity = Mapper.ToEF(SurveyResponse, SurveyResponse.CurrentOrgId);
                    }
                    else
                    {
                        SurveyResponseEntity = Mapper.ToEF(SurveyResponse);
                    }
                    //SurveyResponseEntity.Users.Add(new User { UserID = 2 });
                    User User = Context.Users.FirstOrDefault(x => x.UserID == SurveyResponse.UserId);
                    SurveyResponseEntity.Users.Add(User);
                    Context.AddToSurveyResponses(SurveyResponseEntity);

                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

#endif //WebEnterCode
        }

        public bool DoesResponseExist(Guid responseId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRecordStatus(SurveyResponseBO surveyResponseBO)
        {
            throw new NotImplementedException();
        }

        public void UpdateRecordStatus(string responseId, int status)
        {
            throw new NotImplementedException();
        }

        public void UpdateSurveyResponse(SurveyResponseBO surveyResponse)
        {
            throw new NotImplementedException();
        }

#region Security API
        public void UpdatePassCode(UserAuthenticationRequestBO passcodeBO)
        {
            throw new NotImplementedException();
        }

        public UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO)
        {
            throw new NotImplementedException();
        }

        public void DeleteResponse(Web.Enter.Common.BusinessObject.ResponseBO responseBO)
        {
            throw new NotImplementedException();
        }

        public void InsertResponse(Web.Enter.Common.BusinessObject.ResponseBO item)
        {
            throw new NotImplementedException();
        }
#endregion Security API
    }
}
