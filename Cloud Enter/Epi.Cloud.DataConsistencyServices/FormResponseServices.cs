using Epi.Cloud.DataConsistencyServices.Common;
using Epi.Cloud.DataEntryServices.Interfaces;
using Epi.DataPersistence.DataStructures;

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
