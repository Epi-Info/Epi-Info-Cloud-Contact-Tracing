namespace Epi.Cloud.SqlServer
{
    public interface IDataSource
    {
        System.Data.IDataReader GetDataTableReader(string pSQL);
        object GetScalar(string pSQL);
        bool ExecuteSQL(string pSQL);
    }
}
