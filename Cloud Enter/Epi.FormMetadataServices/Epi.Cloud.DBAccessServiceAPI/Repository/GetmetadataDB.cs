using System.Data;
using System.Threading.Tasks;
using System.Web.Configuration;
using Epi.Cloud.SqlServer;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DBAccessService.Repository
{
    public class GetmetadataDB
    {
        //string connStr = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;


        //Call the Cloud EF and get the meta data
        public async Task<Template> MetaDataAsync(string projectId)
        {            
            DataTable dt = new DataTable();

            // Retrive the Template level Attributes
            MetaData metaDt = new MetaData();
            Template lstMetaDataFieldsAtr = new Template();

            //Get the meta data using entity framework
            lstMetaDataFieldsAtr = metaDt.GetProjectTemplateMetadata(projectId);         

            return await Task.FromResult(lstMetaDataFieldsAtr);
        } 
    }
}