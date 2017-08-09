using System;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.MVC.Models
{

    /// <summary>
    /// The Survey Model that will be pumped to view
    /// </summary>
    public class SurveyAnswerModel
    {
        public SurveyAnswerModel()
        {
        }

        public string SurveyId { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public int Status { get; set; }

        public string ResponseId { get; set; }
        public string FormName { get; set; }
        public string FormId { get; set; }

        public string ParentResponseId { get; set; }
        public string ParentFormName { get; set; }
        public string ParentFormId { get; set; }

        public string RootResponseId { get; set; }
        public string RootFormName { get; set; }
        public string RootFormId { get; set; }
    }

    public static class SurveyAnswerModelExtensions
    {
        public static SurveyAnswerModel ToSurveyAnswerModel(this IResponseContext responseContext, SurveyAnswerModel surveyAnswerModel = null)
        {
            if (surveyAnswerModel == null) surveyAnswerModel = new SurveyAnswerModel();

            surveyAnswerModel.RootFormId = responseContext.RootFormId;
            surveyAnswerModel.RootFormName = responseContext.RootFormName;
            surveyAnswerModel.RootResponseId = responseContext.RootResponseId;

            surveyAnswerModel.ParentFormId = responseContext.ParentFormId;
            surveyAnswerModel.ParentFormName = responseContext.ParentFormName;
            surveyAnswerModel.ParentResponseId = responseContext.ParentResponseId;

            surveyAnswerModel.FormId = responseContext.FormId;
            surveyAnswerModel.FormName = responseContext.FormName;
            surveyAnswerModel.ResponseId = responseContext.ResponseId;

            return surveyAnswerModel;
        }
    }
}