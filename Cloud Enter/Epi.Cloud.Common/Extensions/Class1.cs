using System.Linq;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class FormResponseDetailExtension
    {
        public static FormResponseDetail ToCopy(this FormResponseDetail source)
        {
            var formResponseDetail = new FormResponseDetail
            {
                ChildFormResponseDetailList = source.ChildFormResponseDetailList != null ? source.ChildFormResponseDetailList.Select(c => c).ToList() : null,
                DisabledFieldsList = source.DisabledFieldsList,
                FirstSaveLogonName = source.FirstSaveLogonName,
                FirstSaveTime = source.FirstSaveTime,
                FormId = source.FormId,
                FormName = source.FormName,
                HiddenFieldsList = source.HiddenFieldsList,
                HighlightedFieldsList = source.HighlightedFieldsList,
                IsDraftMode = source.IsDraftMode,
                IsLocked = source.IsLocked,
                IsNewRecord = source.IsNewRecord,
                IsRelatedView = source.IsRelatedView,
                LastPageVisited = source.LastPageVisited,
                LastSaveLogonName = source.LastSaveLogonName,
                LastSaveTime = source.LastSaveTime,
                PageIds = source.PageIds.Select(id => id).ToList(),
                PageResponseDetailList = source.PageResponseDetailList != null ? source.PageResponseDetailList.Select(p => p).ToList() : null,
                ParentFormId = source.ParentFormId,
                ParentFormName = source.ParentFormName,
                ParentResponseId = source.ParentResponseId,
                RecStatus = source.RecStatus,
                RequiredFieldsList = source.RequiredFieldsList,
                ResponseId = source.ResponseId,
                RootFormId = source.RootFormId,
                RootFormName = source.RootFormName,
                RootResponseId = source.RootResponseId,
                UserId = source.UserId,
                UserName = source.UserName,
                UserOrgId = source.UserOrgId
            };
            return formResponseDetail;
        }
    }
}
