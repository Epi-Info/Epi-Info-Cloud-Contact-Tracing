namespace Epi.Web.MVC.Facade
{
    public class SurveyTableStorageFacade : ISurveyTableStorageFacade
    {

        public SurveyTableStorageFacade()
        {

        }

        //#region Insert into Table Storage
        ///*//Insert new record  survey response data in to table storage.*/
        //public bool InsertSurveyResponseToTableStorageAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        //{
        //    CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
        //    SurveyEntity _surveyResponseData = new SurveyEntity(surveyInfoModel.SurveyId, responseId)
        //    {
        //        DateCreated = DateTime.UtcNow,
        //        SurveyName = surveyInfoModel.SurveyName,
        //        PageNumber = PageNumber,
        //        StatusId = 1,
        //        UserEmail = "Ananth_Raja@sra.com",
        //        DateUpdated = DateTime.UtcNow
        //    };

        //    _surveyResponseData.SurveyTableEntity = new List<SurveyTableEntity>();

        //    foreach (var field in form.InputFields)
        //    {
        //        SurveyTableEntity surveyEntity = new SurveyTableEntity();
        //        surveyEntity.QuestionName = field.Title;
        //        surveyEntity.Answer = field.Response;
        //        if (!field.IsPlaceHolder)
        //        {
        //            _surveyResponseData.SurveyTableEntity.Add(surveyEntity);
        //        }
        //    }
        //    var task =_surveyResponse.Storeservey(_surveyResponseData);

        //    return task.Result;
        //}

        //#endregion

        //#region Read from Table Storage
        ////Read surveyInfo from SurveyResponse from Table storage  SurveyId
        //public List<SurveyEntity> GetSurveyInfoBySurveyId(string _surveyId, string _tableName)
        //{
        //    List<SurveyEntity> surveyEntity = new List<SurveyEntity>();
        //    CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
        //    surveyEntity = _surveyResponse.RetrieveFromTableStorageBySurveyId(_surveyId, _tableName);
        //    return surveyEntity;
        //} 
        //#endregion

        //#region Read survey response from table storage by SurveyID and ResponseId        

        ////public SurveyEntity GetSurveyInfoBySurveyAndResponseId(SurveyEntity _surveyEntity)
        ////{

        ////    CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
        ////    _surveyEntity = _surveyResponse.RetrieveSurveyInfoFromTableStorage("asdf","asdf");
        ////    return _surveyEntity;
        ////}
        //#endregion

    }
}
