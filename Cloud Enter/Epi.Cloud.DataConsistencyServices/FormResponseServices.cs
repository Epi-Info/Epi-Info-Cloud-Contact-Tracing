using Epi.Cloud.DataConsistencyServices.Common;
using Epi.DataPersistence.DataStructures;
using Epi.Web.Common.Interfaces;

namespace Epi.Cloud.DataConsistencyServices
{
	public class FormResponseServices : IResponseServices
	{
		private readonly ISurveyPersistenceFacade _surveyPersistenceFacade;

		public FormResponseServices(ISurveyPersistenceFacade surveyPersistenceFacade)
		{
			_surveyPersistenceFacade = surveyPersistenceFacade;
		}

		public FormResponseDetail GetHierarchialResponse(string responseId)
		{
			var hierarchialFormResponseDetail = _surveyPersistenceFacade.GetHierarchialResponsesByResponseId(responseId);
			return hierarchialFormResponseDetail;
		}
	}
}
