using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.FormMetadata.DataStructures;
using Epi.FormMetadata.Constants;

namespace Epi.Cloud.Common.Metadata
{
    public class MetadataAccessor
    {
        public static class StaticCache
        {
            // Form Digests [FormId]
            public static FormDigest[] _formDigests = null;

            // Page[FormId][PageId]
            public static Dictionary<string, Dictionary<int, Page>> _pageMetadata = new Dictionary<string, Dictionary<int, Page>>();

            // PageDigests[FormId][PageId]
            public static PageDigest[][] _pageDigests = null;

            // FieldDigests[FormId][PageId]
            public static Dictionary<string, FieldDigest[]> _fieldDigests = new Dictionary<string, FieldDigest[]>();

            public static IProjectMetadataProvider _projectMetadataProvider = null;

            // FieldAttributes[FormId][PageId][FieldName]
            public static Dictionary<string, Dictionary<int, Dictionary<string, FieldAttributes>>> _pageFieldAttributes = null;
        }

        [ThreadStatic]
        protected string _formId;


        public MetadataAccessor()
        {
        }

        public MetadataAccessor(string surveyId)
        {
            _formId = surveyId;
        }

        public string CurrentFormId { get { return _formId; } set { _formId = value; } }

        public static IProjectMetadataProvider ProjectMetadataProvider
        {
            get
            {
                if (StaticCache._projectMetadataProvider == null)
                {
                    StaticCache._projectMetadataProvider = DependencyHelper.GetService<IProjectMetadataProvider>();
                }
                return StaticCache._projectMetadataProvider;
            }

            protected set { StaticCache._projectMetadataProvider = value; }
        }

        public FormDigest[] FormDigests
        {
            get
            {
                if (StaticCache._formDigests == null) StaticCache._formDigests = ProjectMetadataProvider.GetFormDigestsAsync().Result;
                return StaticCache._formDigests;
            }
        }

        public PageDigest[][] PageDigests
        {
            get
            {
                if (StaticCache._pageDigests == null) StaticCache._pageDigests = (PageDigest[][])ProjectMetadataProvider.GetProjectPageDigestsAsync().Result.Clone();
                return StaticCache._pageDigests;
            }
        }

        public int PageIdFromPageNumber(string formId, int pageNumber)
        {
            PageDigest pageDigest = GetPageDigestByPageNumber(formId, pageNumber);
            var pageId = pageDigest.PageId;
            return pageId;
        }

        public int GetFormPageCount(string formId)
        {
            return GetPageDigests(formId).Count();
        }

        public int[] GetFormPageIds(string formId)
        {
            return GetPageDigests(formId).Select(pd => pd.PageId).ToArray();
        }

        public FieldDigest GetFieldDigestByFieldName(string formId, string fieldName)
        {
            fieldName = fieldName.ToLower();
            var fieldDigests = GetFieldDigests(formId);
            FieldDigest fieldDigest;
            fieldDigest = fieldDigests.Where(fd => fd.FieldName == fieldName).SingleOrDefault();
            return fieldDigest;
        }

		public FieldDigest[] GetFieldDigestsByFieldNames(string formId, IEnumerable<string> fieldNames)
		{
			formId = formId.ToLower();
			var fieldNameList = fieldNames.Select(n => n.ToLower()).ToArray();
			var fieldDigests = GetFieldDigests(formId).Where(fd => fieldNameList.Contains(fd.FieldName));
			return fieldDigests.ToArray();
		}

		public FieldDigest[] GetFieldDigests(string formId)
        {
            FieldDigest[] fieldDigests = null;
            if (!StaticCache._fieldDigests.TryGetValue(formId, out fieldDigests))
                StaticCache._fieldDigests[formId] = fieldDigests = ProjectMetadataProvider.GetFieldDigestsAsync(formId).Result;
            return fieldDigests;
        }

        public Page GetCurrentFormPageMetadataByPageId(int pageId)
        {
            return GetPageMetadataByPageId(_formId, pageId);
        }

        public Dictionary<int, Page> GetAllPageMetadatasByFormId(string formId)
        {
            Dictionary<int, Page> pageMetadatas = null;
            if (!StaticCache._pageMetadata.TryGetValue(formId, out pageMetadatas))
            {
                StaticCache._pageMetadata[formId] = pageMetadatas = new Dictionary<int, Page>();
            }
            return pageMetadatas;
        }

        public Page GetPageMetadataByPageId(string formId, int pageId)
        {
            Dictionary<int, Page> pageMetadatas = null;
            if (!StaticCache._pageMetadata.TryGetValue(formId, out pageMetadatas))
            {
                StaticCache._pageMetadata[formId] = pageMetadatas = new Dictionary<int, Page>();
            }

            Page pageMetadata = null;
            if (!pageMetadatas.TryGetValue(pageId, out pageMetadata))
            {
                pageMetadatas[pageId] = pageMetadata = ProjectMetadataProvider.GetPageMetadataAsync(formId, pageId).Result;
            }

            return pageMetadata;
        }

        public Dictionary<string, FieldAttributes> GetPageFieldAttributesByPageId(string formId, int pageId)
        {
            if (StaticCache._pageFieldAttributes == null)
            {
                StaticCache._pageFieldAttributes = new Dictionary<string, Dictionary<int, Dictionary<string, FieldAttributes>>>();
            }

            Dictionary<int, Dictionary<string, FieldAttributes>> pageFieldAttributesByPageId = null;
            if (!StaticCache._pageFieldAttributes.TryGetValue(formId, out pageFieldAttributesByPageId))
            {
                StaticCache._pageFieldAttributes[formId] = pageFieldAttributesByPageId = new Dictionary<int, Dictionary<string, FieldAttributes>>();
            }

            Dictionary<string, FieldAttributes> pageFieldAttributes;
            if (!pageFieldAttributesByPageId.TryGetValue(pageId, out pageFieldAttributes))
            {
                var formDigest = FormDigests.Single(d => d.FormId == formId);
                var pageMetadata = GetPageMetadataByPageId(formId, pageId);
                pageFieldAttributesByPageId[pageId] = pageFieldAttributes = FieldAttributes.MapFieldMetadataToFieldAttributes(pageMetadata, formDigest.CheckCode);
            }
            return pageFieldAttributes;
        }

        public FormDigest GetCurrentFormDigest()
        {
            return GetFormDigest(_formId);
        }

        public FormDigest GetFormDigest(string formId)
        {
            return FormDigests.SingleOrDefault(f => f.FormId == formId);
        }

        public FormDigest GetFormDigestByFormName(string formName)
        {
            formName = formName.ToLower();
            return FormDigests.SingleOrDefault(f => f.FormName.ToLower() == formName);
        }

        public PageDigest[] GetCurrentFormPageDigests()
        {
            return GetPageDigests(_formId);
        }

        public PageDigest[] GetPageDigests(string formId)
        {
            var pageDigests = PageDigests.SingleOrDefault(d => d[0].FormId == formId);
            return pageDigests;
        }

        public PageDigest GetPageDigestByPageNumber(string formId, int pageNumber)
        {
            var pageDigests = PageDigests.Single(d => d[0].FormId == formId);
            var pageDigest = pageDigests.Single(d => d.PageNumber == pageNumber);
            return pageDigest;
        }

        public PageDigest GetPageDigestByPageId(string formId, int pageId)
        {
            var pageDigests = PageDigests.Single(d => d[0].FormId == formId);
            var pageDigest = pageId > 0 ? pageDigests.Single(d => d.PageId == pageId) : pageDigests.FirstOrDefault();
            return pageDigest;
        }

        public FieldDigest[] GetFieldDigestsWithPageNumber(string formId, int pageNumber)
        {
            var pagePosition = pageNumber - 1;
            return GetFieldDigests(formId).Where(d => d.Position == pagePosition).ToArray();
        }

        public FieldDigest[] GetFieldDigestsNotWithPageNumber(string formId, int pageNumber)
        {
            var pagePosition = pageNumber - 1;
            return GetFieldDigests(formId).Where(d => d.Position != pagePosition).ToArray();
        }

        public AbridgedFieldInfo GetFieldInfoByFieldName(string formId, int pageId, string fieldName)
        {
            fieldName = fieldName.ToLower();
            var fieldDigest = GetPageDigestByPageId(formId, pageId);
            var fieldInfo = fieldDigest.Fields.Where(f => f.FieldName == fieldName).SingleOrDefault();
            return fieldInfo;
        }

        public FieldDataType GetFieldDataTypeByFieldName(string formId, int pageId, string fieldName)
        {
            var fieldInfo = GetFieldInfoByFieldName(formId, pageId, fieldName);
            return fieldInfo != null ? fieldInfo.DataType : FieldDataType.Undefined;
        }

        public FieldAttributes GetFieldAttributes(FieldDigest fieldDigest)
        {
            var formId = fieldDigest.FormId;
            var pageId = fieldDigest.PageId;
            var fieldName = fieldDigest.Field.FieldName;
            var fieldAttributes = GetFieldAttributesByPageId(formId, pageId, fieldName);
            return fieldAttributes;
        }

        public FieldAttributes GetFieldAttributesByPageId(string formId, string pageId, string fieldName)
        {
            return GetFieldAttributesByPageId(formId, Convert.ToInt32(pageId), fieldName);
        }

        public FieldAttributes GetFieldAttributesByPageId(string formId, int pageId, string fieldName)
        {
            fieldName = fieldName.ToLower();
            var pageFieldAttributes = GetPageFieldAttributesByPageId(formId, pageId);
            FieldAttributes fieldAttributes = null;
            fieldAttributes = pageFieldAttributes.TryGetValue(fieldName, out fieldAttributes) ? fieldAttributes : null;
            return fieldAttributes;
        }
    }
}
