using System;
using System.Collections.Generic;
using System.Linq;

namespace Epi.DataPersistence.DataStructures
{
	public partial class FormResponseDetail
	{
		//private MetadataAccessor _metadataAccessor;

		public FormResponseDetail()
		{
			RequiredFieldsList = string.Empty;
			HiddenFieldsList = string.Empty;
			HighlightedFieldsList = string.Empty;
			DisabledFieldsList = string.Empty;
			PageResponseDetailList = new List<PageResponseDetail>();
			ChildFormResponseDetailList = new List<FormResponseDetail>();
			PageIds = new List<int>();
		}

		public void AddPageResponseDetail(PageResponseDetail pageResponseDetail)
		{
			var existingItem = PageResponseDetailList.SingleOrDefault(p => p.PageId == pageResponseDetail.PageId);
			if (existingItem != null) PageResponseDetailList.Remove(existingItem);
			FormId = FormId ?? pageResponseDetail.FormId;
			FormName = FormName ?? pageResponseDetail.FormName;
			pageResponseDetail.FormId = FormId;
			pageResponseDetail.FormName = FormName;
			if (pageResponseDetail.PageNumber < 1)
			{
				// TODO: Remove dependancy on MetadataAccessor
				//if (_metadataAccessor == null) _metadataAccessor = new MetadataAccessor(FormId);
				//var pageDigest = _metadataAccessor.GetPageDigestByPageId(FormId, pageResponseDetail.PageId);
				//pageResponseDetail.PageNumber = pageDigest.PageNumber;
			}
			PageResponseDetailList.Add(pageResponseDetail);
		}

		public void AddChildFormResponseDetail(FormResponseDetail childFormResponseDetail)
		{
			var existingItem = ChildFormResponseDetailList.SingleOrDefault(f => f.FormId == childFormResponseDetail.FormId);
			if (existingItem != null) ChildFormResponseDetailList.Remove(childFormResponseDetail);
			childFormResponseDetail.ParentFormId = FormId;
			ChildFormResponseDetailList.Add(childFormResponseDetail);
		}

		public List<FormResponseDetail> FlattenHierarchy()
		{
			var flattenedHierarchy = new List<FormResponseDetail>();
			flattenedHierarchy.Add(this);
			foreach (var child in ChildFormResponseDetailList)
			{
				FlattenChildHierarchy(child, flattenedHierarchy);
			}
			return flattenedHierarchy;
		}

		private List<FormResponseDetail> FlattenChildHierarchy(FormResponseDetail childFormResponseDetail, List<FormResponseDetail> flattenedHierarchy)
		{
			flattenedHierarchy.Add(childFormResponseDetail);
			foreach (var child in childFormResponseDetail.ChildFormResponseDetailList)
			{
				FlattenChildHierarchy(child, flattenedHierarchy);
			}
			return flattenedHierarchy;
		}

		public Dictionary<string, string> FlattenedResponseQA(Func<string, string> keyModifier = null)
		{
			var flattenedResponseQA = new Dictionary<string, string>();
			foreach (var pageResponseDetail in PageResponseDetailList)
			{
				foreach (var qa in pageResponseDetail.ResponseQA)
					flattenedResponseQA[keyModifier != null ? keyModifier(qa.Key) : qa.Key] = qa.Value;
			}
			return flattenedResponseQA;
		}

		public PageResponseDetail GetPageResponseDetailByPageId(int pageId)
		{
			var pageResponseDetail = PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
			return pageResponseDetail;
		}
		public PageResponseDetail GetPageResponseDetailByPageNumber(int pageNumber)
		{
			var pageResponseDetail = PageResponseDetailList.SingleOrDefault(p => p.PageNumber == pageNumber);
			return pageResponseDetail;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageNumber"></param>
		/// <returns>
		/// If found return the index of the entry. 
		/// If not found then return the one's complement of the insertion index.
		/// </returns>
		public int GetPageResponseDetailIndexByPageNumber(int pageNumber)
		{
			int result = int.MinValue;
			for (int index = 0; index < PageResponseDetailList.Count; ++index)
			{
				if (PageResponseDetailList[index].PageNumber == pageNumber) return index;
				if (result == int.MinValue && PageResponseDetailList[index].PageNumber > pageNumber) result = ~index;
			}
			return result;
		}

		public bool MergePageResponseDetail(PageResponseDetail pageResponseDetail)
		{
			// Assume that the page has been updated
			var hasBeenUpdated = true;

			if (PageResponseDetailList.Count > 0)
			{
				// Check if the page already exists in the PageResponseDetail.
				// Either the index to the existing page response detail will be returned
				// or a negative number which is the one's complement of the insertion point will be returned. 
				int index = GetPageResponseDetailIndexByPageNumber(pageResponseDetail.PageNumber);
				if (index >= 0)
				{
					// Check to see if the page has been updated
					hasBeenUpdated = !(PageResponseDetailList[index].Equals(pageResponseDetail));
					if (hasBeenUpdated)
					{
						pageResponseDetail.HasBeenUpdated = hasBeenUpdated;
						PageResponseDetailList[index] = pageResponseDetail;
					}
				}
				else
				{
					hasBeenUpdated = true;
					pageResponseDetail.HasBeenUpdated = hasBeenUpdated;

					// The one's complement is the insertion index
					index = ~index;

					if (index >= PageResponseDetailList.Count)
					{
						PageResponseDetailList.Add(pageResponseDetail);
					}
					else
					{
						PageResponseDetailList.Insert(index, pageResponseDetail);
					}
				}
			}
			else
			{
				hasBeenUpdated = true;
				pageResponseDetail.HasBeenUpdated = hasBeenUpdated;
				PageResponseDetailList.Add(pageResponseDetail);
			}

			if (!PageIds.Contains(pageResponseDetail.PageId))
			{
				PageIds.Add(pageResponseDetail.PageId);
				PageIds.Sort();
			}

			return hasBeenUpdated;
		}
	}
}
