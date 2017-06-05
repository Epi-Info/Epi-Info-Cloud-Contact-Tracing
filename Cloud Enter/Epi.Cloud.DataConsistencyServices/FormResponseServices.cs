using Epi.Cloud.DataConsistencyServices.Common;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Common.Interfaces;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.DataConsistencyServices
{
	public class FormResponseServices : IResponseServices
	{
		private readonly ISurveyPersistenceFacade _surveyPersistenceFacade;

		public FormResponseServices(ISurveyPersistenceFacade surveyPersistenceFacade)
		{
			_surveyPersistenceFacade = surveyPersistenceFacade;
		}

		public FormResponseDetail GetHierarchicalResponse(IResponseContext responceContext)
		{
			var hierarchicalFormResponseDetail = _surveyPersistenceFacade.GetHierarchicalResponsesByResponseId(responceContext);
			return hierarchicalFormResponseDetail;
		}
	}
}
