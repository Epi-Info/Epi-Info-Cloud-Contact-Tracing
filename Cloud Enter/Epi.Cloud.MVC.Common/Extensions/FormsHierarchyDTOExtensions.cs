﻿using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class FormsHierarchyDTOExtensions
    {
        public static List<RelateModel> ToRelateModel(this List<FormsHierarchyDTO> formsHierarchyDTOList, string FormId)
        {
            List<RelateModel> List = new List<RelateModel>();

            // Common.DTO.FormsHierarchyDTO FormsHierarchyDTO = FormsHierarchy.Single(X => X.FormId == FormId);
            foreach (var formsHierarchyDTO in formsHierarchyDTOList)
            {
                RelateModel RelateModel = new RelateModel();
                RelateModel.RootFormId = formsHierarchyDTO.RootFormId;
                RelateModel.FormId = formsHierarchyDTO.FormId;
                RelateModel.ViewId = formsHierarchyDTO.ViewId;
                RelateModel.IsSqlProject = formsHierarchyDTO.IsSqlProject;
                RelateModel.IsRoot = formsHierarchyDTO.IsRoot;
                RelateModel.ResponseIds = formsHierarchyDTO.ResponseIds.ToSurveyAnswerModel();
                List.Add(RelateModel);
            }
            return List;
        }

        public static List<RelateModel> ToRelateModel(this SurveyAnswerDTO surveyAnswerDTO)
        {
            List<RelateModel> List = new List<RelateModel>();
            RelateModel RelateModel = new RelateModel();
            RelateModel.RootFormId = surveyAnswerDTO.RootFormId;
            RelateModel.FormId = surveyAnswerDTO.FormId;
            RelateModel.ViewId = surveyAnswerDTO.ViewId;
            RelateModel.IsSqlProject = true;
            RelateModel.IsRoot = surveyAnswerDTO.IsRootResponse;
            RelateModel.ResponseIds.Add(surveyAnswerDTO.ToSurveyAnswerModel());
            List.Add(RelateModel);
            return List;
        }
    }
}
