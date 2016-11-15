using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using Epi.Cloud.MetadataServices.DataTypes;
using Epi.Cloud.Common.Configuration;
using System.Configuration;

namespace Epi.Cloud.MetadataServices
{
    public abstract class MetadataProxy
    {
        private string _apiUrl;
        public MetadataProxy()
        {
            var apiUrlKey = ConfigurationHelper.GetEnvironmentResourceKey("MetadataAccessServiceAPI", "Environment.API");
            _apiUrl = ConfigurationManager.AppSettings[apiUrlKey];
        }


        protected T GetData<T>(string endpoint)
        {
            string url = FormatUrl(endpoint);
            var resp = GetClient().GetAsync(url).Result;
            return GetResponse<T>(resp);
        }

        protected T2 PostData<T1, T2>(string endpoint, T1 data)
            where T1 : class
            where T2 : class
        {
            HttpClient client = GetClient();
            var response = client.PostAsJsonAsync<T1>(FormatUrl(endpoint), data).Result;
            return GetResponse<T2>(response);
        }



        protected T2 PutData<T1, T2>(string endpoint, T1 data)
            where T1 : class
            where T2 : class
        {
            HttpClient client = GetClient();
            var response = client.PostAsJsonAsync<T1>(FormatUrl(endpoint), data).Result;
            return GetResponse<T2>(response);
        }

        protected T DeleteData<T>(string endpoint)
            where T : class
        {
            HttpClient client = GetClient();
            var response = client.DeleteAsync(FormatUrl(endpoint)).Result;
            return GetResponse<T>(response);
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
