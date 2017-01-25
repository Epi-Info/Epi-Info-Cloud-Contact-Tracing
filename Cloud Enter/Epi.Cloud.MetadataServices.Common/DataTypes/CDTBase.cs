namespace Epi.Cloud.MetadataServices.Common.DataTypes
{
    public class CDTBase
    {
        public CDTBase()
        {

        }
        public CDTBase(CDTResponse response)
        {
            Response = response;
        }
        public CDTResponse Response { get; set; }

        public string UserID { get; set; }
    }
}
