using Epi.Cloud.DataConsistencyServices.Proxy;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistence.Extensions;
using Epi.DataPersistenceServices.CosmosDB;
using Newtonsoft.Json;

namespace Epi.Cloud.DataConsistencyServices.Services
{
	public class ResponseInfoService : IResponseInfoProxyService
	{
		public ResponseInfoService()
		{
		}
		public string GetResponseInfoData(IResponseContext responseContext)
		{

			CosmosDBCRUD formResponseCRUD = new CosmosDBCRUD();
			var formResponseProperties = formResponseCRUD.GetHierarchicalResponseListByResponseId(responseContext, /*includeDeletedRecords=*/true, /*excludeInProcessRecords=*/true);
			var formResponseDetail = formResponseProperties != null ? formResponseProperties.ToHierarchicalFormResponseDetail() : null;
			string response = JsonConvert.SerializeObject(formResponseDetail);
			return response;
		}

        public bool PutResponseInfoData(FormResponseDetail formResponseDetail)
        {
            CosmosDBCRUD formResponseCRUD = new CosmosDBCRUD();
            var formResponsePropertiesFlattenedList = formResponseDetail.ToFormResponsePropertiesFlattenedList();
            var result = formResponseCRUD.SaveFormResponsePropertiesAsync(formResponsePropertiesFlattenedList).Result;
            return true;
        }
    }
}