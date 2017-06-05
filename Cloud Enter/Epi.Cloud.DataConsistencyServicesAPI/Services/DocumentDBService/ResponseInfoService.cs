using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.DocumentDB;
using Newtonsoft.Json;

namespace Epi.Cloud.DataConsistencyServices.Services
{
	public class ResponseInfoService : IResponseInfoProxyService
	{
		public ResponseInfoService()
		{

		}
		public string GetResponseInfoData(IResponseContext responceContext)
		{

			DocumentDbCRUD formResponseCRUD = new DocumentDbCRUD();
			var formResponseProperties = formResponseCRUD.GetHierarchicalResponseListByResponseId(responceContext, /*includeDeletedRecords=*/true, /*excludeInProcessRecords=*/true);
			var formResponseDetail = formResponseProperties != null ? formResponseProperties.ToHierarchicalFormResponseDetail() : null;
			string response = JsonConvert.SerializeObject(formResponseDetail);
			return response;
		}
	}
}