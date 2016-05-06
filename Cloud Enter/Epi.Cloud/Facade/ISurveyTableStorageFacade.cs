using Epi.Web.DataEntryServices.Model;
using Epi.Web.MVC.Models;
using System.Collections.Generic;

namespace Epi.Web.MVC.Facade
{
    //Doing Survey response CRUD  operation in Table Storage
    public interface ISurveyTableStorageFacade
    {
        //Insert new record  survey response data in to table storage.
        void InsertSurveyResponseToTableStorage(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);

        //Get the SurveyInfo from table storage by SurveyId
        List<SurveyEntity> GetSurveyInfoBySurveyId(string _surveyId, string _tableName);

        //Get the SurveyInfo from Table Storage by SurveyId,ResponseId 
        //SurveyEntity GetSurveyInfoBySurveyAndResponseId(SurveyEntity _surveyEntity);


    }
}

