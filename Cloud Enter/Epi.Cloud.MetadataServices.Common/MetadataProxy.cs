using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Epi.Cloud.MetadataServices.DataTypes;
using Epi.Cloud.Common.Configuration;
using System.Configuration;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.MetadataServices
{
    public abstract class MetadataProxy
    {
        private string _apiUrl;
        public MetadataProxy()
        {
            var apiUrlKey = ConfigurationHelper.GetEnvironmentResourceKey(AppSettings.Key.MetadataAccessServiceAPI, AppSettings.Key.Environment_API);
            _apiUrl = ConfigurationManager.AppSettings[apiUrlKey];
        }


        protected T GetData<T>(string endpoint)
        {
            string url = FormatUrl(endpoint);
            var resp = GetClient().GetAsync(url).Result;
            return GetResponse<T>(resp);
        }


        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            ///Token logic;
            return client;
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
