using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.DataConsistencyServices.Common
{
    public interface IResponseServices
	{
		FormResponseDetail GetResponse(string responseId);
    }
}
