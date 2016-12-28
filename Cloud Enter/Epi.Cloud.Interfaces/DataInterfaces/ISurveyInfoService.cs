using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface ISurveyInfoService
	{
		SurveyInfoBO GetSurveyInfoById(string surveyId);
		SurveyInfoBO GetParentInfoByChildId(string childSurveyId);

		SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest pRequest);

		List<FormsHierarchyBO> GetFormsHierarchyIdsByRootId(string rootId);
	}
}
