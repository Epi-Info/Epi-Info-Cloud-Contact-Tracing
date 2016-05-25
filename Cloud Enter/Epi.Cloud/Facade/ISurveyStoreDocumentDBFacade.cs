﻿using Epi.Cloud.DataEntryServices.Model;
using Epi.Web.MVC.Models;
using MvcDynamicForms.Fields;
using System.Collections.Generic;

namespace Epi.Web.MVC.Facade
{
    public interface ISurveyStoreDocumentDBFacade
    {
        //Insert new record  survey response data in to table storage.

        bool InsertSurveyResponseToDocumentDBStoreAsync(List<FieldAttributes> metadata, SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);
        SurveyQuestionandAnswer ReadSurveyInfromDocumentDocumentDB(string responseId, string PageNumber);
        SurveyQuestionandAnswer ReadSurveyAnswerByResponseID(string suveyName, string surveyID, string responseID, string pageId);
    }
}