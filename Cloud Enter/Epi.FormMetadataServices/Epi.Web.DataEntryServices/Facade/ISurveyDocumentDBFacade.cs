using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epi.Cloud.DataEntryServices.Facade
{
    public interface ISurveyStoreDocumentDBFacade
    {
        //Insert new record  survey response data in to table storage.

        Task<bool> InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);
        FormQuestionandAnswer ReadSurveyInfromDocumentDocumentDB(string responseId, string PageNumber);
        FormQuestionandAnswer ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId);
        SurveyAnswerResponse DeleteResponse(Survey SARequest);

        FormsHierarchyDTO GetChildRecordByChildFormId(string ChildFormId, string RelateParentId, string DbName, List<string> Params);
        bool SaveFormPropertiesToDocumentDB(ProjectDigest ProjectMetaData, bool Status, int UserId, string ResponseId,string RelateParentId);
        SurveyAnswerResponse GetSurveyAnswerResponse(string SurveyName,string responseId, string FormId, int UserId ,string PageId);


    }
}
