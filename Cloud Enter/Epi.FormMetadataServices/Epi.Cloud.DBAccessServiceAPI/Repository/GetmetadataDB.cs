using System.Data;
using System.Threading.Tasks;
using System.Web.Configuration;
using Epi.Cloud.SqlServer;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Repository
{
    public class GetmetadataDB
    {
        string connStr = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;


        //Call the Cloud EF and get the meta data
        public async Task<ProjectTemplateMetadata> MetaDataAsync(int? pageid)
        {            
            DataTable dt = new DataTable();

            // Retrive the Template level Attributes
            MetaData metaDt = new MetaData();
            ProjectTemplateMetadata lstMetaDataFieldsAtr = new ProjectTemplateMetadata();

            //Get the meta data using entity framework passing pageid
            lstMetaDataFieldsAtr = metaDt.GetFieldsByPageAsData();         

            return await Task.FromResult(lstMetaDataFieldsAtr);
        } 
    }
}