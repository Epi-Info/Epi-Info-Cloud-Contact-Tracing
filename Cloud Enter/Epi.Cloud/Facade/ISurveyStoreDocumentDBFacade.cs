using Epi.Web.MVC.Models; 

namespace Epi.Web.MVC.Facade
{
   public  interface ISurveyStoreDocumentDBFacade
    {
        //Insert new record  survey response data in to table storage.
         bool InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);

    }
}
