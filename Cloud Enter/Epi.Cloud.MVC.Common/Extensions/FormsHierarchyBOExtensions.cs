using System.Collections.Generic;
using System.Linq;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Cloud.MVC.Extensions;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class FormsHierarchyBOExtensions
    {
        public static FormsHierarchyDTO ToFormsHierarchyDTO(this FormsHierarchyBO formsHierarchyBO)
        {
            return new FormsHierarchyDTO
            {
                RootFormId = formsHierarchyBO.RootFormId,
                FormId = formsHierarchyBO.FormId,
                ViewId = formsHierarchyBO.ViewId,
                IsSqlProject = formsHierarchyBO.IsSqlProject,
                IsRoot = formsHierarchyBO.IsRoot,
                SurveyInfo = formsHierarchyBO.SurveyInfo.ToSurveyInfoDTO(),
                ResponseIds = formsHierarchyBO.ResponseIds != null ? formsHierarchyBO.ResponseIds.ToSurveyAnswerDTOList() : null
            };
        }

        public static List<FormsHierarchyDTO> ToFormsHierarchyDTOList(this List<FormsHierarchyBO> allChildIDsList)
        {
            return allChildIDsList.Select(formsHierarchyBO => formsHierarchyBO.ToFormsHierarchyDTO()).ToList();
        }
    }
}
