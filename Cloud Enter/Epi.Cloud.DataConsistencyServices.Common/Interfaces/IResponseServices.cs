using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.DataConsistencyServices.Common
{
	public interface IResponseServices
	{
		FormResponseDetail GetHierarchialResponse(IResponseContext responceContext);
	}
}
