using Epi.DataPersistence.DataStructures;

namespace Epi.DataPersistence
{
	public class ConsistencyServiceHack
	{
		public void PersistToSqlServer(FormResponseDetail formResponseDetail)
		{
			Epi.Cloud.SqlServer.PersistToSqlServer objPersistResponse = new Cloud.SqlServer.PersistToSqlServer();
			objPersistResponse.PersistToSQLServerDB(formResponseDetail);
		}
	}
}
