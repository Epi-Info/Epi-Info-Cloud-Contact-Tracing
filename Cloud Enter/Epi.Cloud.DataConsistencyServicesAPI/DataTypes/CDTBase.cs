namespace Epi.Cloud.DataConsistencyServices.DataTypes
{
    public class CDTBase
    {
        CDTResponse _Response;
        public CDTBase()
        {

        }
        public CDTBase(CDTResponse response)
        {
            _Response = response;
        }
        public CDTResponse Response { get { return _Response; } set { _Response = value; } }
        public string UserID { get; set; }
    }
}
