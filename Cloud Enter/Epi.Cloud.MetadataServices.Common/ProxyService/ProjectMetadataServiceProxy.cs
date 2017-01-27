using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.MetadataServices.Common.ProxyService.Interfaces;
using Epi.Cloud.MetadataServices.Common.DataTypes;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;
using static Epi.Cloud.MetadataServices.Common.DataTypes.Constants;

namespace Epi.Cloud.MetadataServices.Common.ProxyService
{
    public class ProjectMetadataServiceProxy : IProjectMetadataProxy
    {
        private string _apiUrl;
        public ProjectMetadataServiceProxy()
        {
            var apiUrlKey = ConfigurationHelper.GetEnvironmentResourceKey(AppSettings.Key.MetadataAccessServiceAPI, AppSettings.Key.Environment_API);
            _apiUrl = ConfigurationManager.AppSettings[apiUrlKey];
        }

        //Forming url to call the Metadata Access API
        public async Task<Template> GetProjectMetadataAsync(string projectId)
        {
            Template projectResponse= new Template();
            string url = string.Format("{0}?ID={1}", ApiEndPoints.Project, projectId ?? "0");
            if (url != null)
            {
                projectResponse = GetData<Template>(url);
            }
            return await Task.FromResult(projectResponse);
        }

        //Forming url to call the API for pageDigest
        public async Task<PageDigest[][]> GetPageDigestMetadataAsync()
        {
            PageDigest[][] pageResponse = null;
            string url = string.Format("{0}", ApiEndPoints.PageDigest);
            if (url != null)
            {
                pageResponse = GetData<PageDigest[][]>(url);
            }
            return await Task.FromResult(pageResponse);
        }


        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            return client;
        }

        private T GetData<T>(string endpoint)
        {
            string url = FormatUrl(endpoint);
            var resp = GetClient().GetAsync(url).Result;
            return GetResponse<T>(resp);
        }

        private string FormatUrl(string endpoint)
        {
            return string.Format("{0}{1}", _apiUrl, endpoint);
        }

        private T GetResponse<T>(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(resp.Content.ReadAsStringAsync().Result);
            }
            else if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                if (typeof(T) == typeof(CDTResponse) || typeof(T).BaseType == typeof(CDTBase))
                {
                    return JsonConvert.DeserializeObject<T>(resp.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    var errorInfo = JsonConvert.DeserializeObject<CDTResponse>(resp.Content.ReadAsStringAsync().Result);
                    object data = new CDTBase(errorInfo);
                    return (T)data;
                }
            }
            else
            {
                //ThrowServiceException(resp);
            }
            return default(T);
        }
    }
}
