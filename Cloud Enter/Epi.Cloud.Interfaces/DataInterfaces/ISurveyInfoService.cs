using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface ISurveyInfoService
	{
		SurveyInfoBO GetSurveyInfoByFormId(string formId);
		SurveyInfoBO GetParentInfoByChildFormId(string childFormId);

		SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest pRequest);

		List<FormsHierarchyBO> GetFormsHierarchyIdsByRootFormId(string rootFormId);
	}
}
