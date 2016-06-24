using Epi.Cloud.DataEntryServices.Model;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Models;
using System.Threading.Tasks;

namespace Epi.Web.MVC.Facade
{
    public interface ISurveyStoreDocumentDBFacade
    {
        //Insert new record  survey response data in to table storage.

        Task<bool> InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);
        SurveyQuestionandAnswer ReadSurveyInfromDocumentDocumentDB(string responseId, string PageNumber);
        SurveyQuestionandAnswer ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId);
        Task<SurveyQuestionandAnswer> ReadSurveyAnswerByResponseIDAsync(string surveyName, string surveyId, string responseId, string pageId);
        SurveyAnswerResponse DeleteResponse(Survey SARequest);
    }
}
