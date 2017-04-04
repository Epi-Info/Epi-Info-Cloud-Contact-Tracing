using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Epi.Cloud.CloudOperation
{
    public class WebJobHandler
    {
        

        /// <summary>
        /// Enable and Disable Web Job
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="jobName"></param>
        /// <param name="webJobStatus"></param>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static bool EnableandDisableWebJob(string userName,string passWord,string jobName,string webJobStatus, string URL)
        {
            try
            {
                string authorization = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes($"{userName}:{passWord}"));
                string WebJobUrlwithStatus = URL + jobName +"/"+ webJobStatus;
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
                    var res = client.PostAsync(WebJobUrlwithStatus, null).Result;
                    if(res.StatusCode==HttpStatusCode.OK)
                    {
                        return true;
                    }
                   
                }

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }


}
