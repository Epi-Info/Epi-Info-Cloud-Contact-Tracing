using System;
using System.Collections.Generic;
using System.Linq;
//using Epi.Cloud.Common.Metadata;
using Epi.FormMetadata.DataStructures;

namespace Epi.FormMetadata.Extensions
{
	public static class ProjectTemplateMetadataExtensions
	{

		public static FormDigest[] ToFormDigests(this Template projectTemplateMetadata)
		{
			var formDigests = new List<FormDigest>();
            var rootFormId = projectTemplateMetadata.Project.Views.Where(v => v.ParentFormId == null).First().FormId;

            foreach (var view in projectTemplateMetadata.Project.Views)
			{
                var formDigest = new FormDigest
                {
                    RootFormId = rootFormId,
					ViewId = view.ViewId,
					FormId = view.FormId,
					FormName = view.Name,
					ParentFormId = view.ParentFormId,
					OrganizationId = view.OrganizationId,
					OrganizationName = view.OrganizationName,
					OrganizationKey = view.OrganizationKey,
					OwnerUserId = view.OwnerUserId,

					NumberOfPages = view.Pages.Length,
					Orientation = view.Orientation,
					Height = view.Height.HasValue ? view.Height.Value : 0,
					Width = view.Width.HasValue ? view.Width.Value : 0,

					CheckCode = view.CheckCode,

					DataAccessRuleId = view.DataAccessRuleId,
					IsDraftMode = view.IsDraftMode

				};

                formDigest.FieldNameToPageIdDirectory = new Dictionary<string, int>();
                foreach (var page in view.Pages)
                {
                    foreach (var field in page.Fields)
                    {
                        formDigest.FieldNameToPageIdDirectory.Add(field.Name.ToLower(), Convert.ToInt32(field.PageId));
                    }
                }
                formDigests.Add(formDigest);
			}

			return formDigests.ToArray();
		}

		public static PageDigest[][] ToPageDigests(this Template projectTemplateMetadata)
		{
			List<PageDigest[]> projectPageDigests = new List<PageDigest[]>();
			var viewIdToViewMap = new Dictionary<int, View>();
			foreach (var view in projectTemplateMetadata.Project.Views)
			{
				viewIdToViewMap[view.ViewId] = view;
				var pages = new Page[0];
				pages = pages.Union(view.Pages).ToArray();
				int numberOfPages = pages.Length;
				var pageDigests = new PageDigest[numberOfPages];
				for (int i = 0; i < numberOfPages; ++i)
				{
					var pageMetadata = pages[i];
					string pageName = pageMetadata.Name;
					int pageId = pageMetadata.PageId.Value;
					int position = pageMetadata.Position;
					int viewId = pageMetadata.ViewId;
					bool isRelatedView = viewIdToViewMap[viewId].IsRelatedView;
					string formId = viewIdToViewMap[viewId].FormId;
					string formName = viewIdToViewMap[viewId].Name;
					pageDigests[i] = new PageDigest(pageName, pageId, position, formId, formName, viewId, isRelatedView, pageMetadata.Fields);
				}
				projectPageDigests.Add(pageDigests);
			}

			return projectPageDigests.ToArray();
		}
#if false
        public static FieldAttributes[][] ToFlattenedFieldAttributes(this Template projectTemplateMetadata)
        {
            List<PageDigest[]> projectPageDigests = new List<PageDigest[]>();
            var viewIdToViewMap = new Dictionary<int, View>();
            foreach (var view in projectTemplateMetadata.Project.Views)
            {
                viewIdToViewMap[view.ViewId] = view;
                var pages = new Page[0];
                pages = pages.Union(view.Pages).ToArray();
                int numberOfPages = pages.Length;
                var pageDigests = new PageDigest[numberOfPages];
                for (int i = 0; i < numberOfPages; ++i)
                {
                    var pageMetadata = pages[i];
                    string pageName = pageMetadata.Name;
                    int pageId = pageMetadata.PageId.Value;
                    int position = pageMetadata.Position;
                    int viewId = pageMetadata.ViewId;
                    bool isRelatedView = viewIdToViewMap[viewId].IsRelatedView;
                    string formId = viewIdToViewMap[viewId].FormId;
                    string formName = viewIdToViewMap[viewId].Name;
                    pageDigests[i] = new PageDigest(pageName, pageId, position, formId, formName, viewId, isRelatedView, pageMetadata.Fields);
                }
                projectPageDigests.Add(pageDigests);
            }

            return projectPageDigests.ToArray();
        }
#endif
    }
}
