using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.MetadataServices.DataTypes
{
    public class Constants
    {
        public struct ApiEndPoints
        {
            public const string Register = "api/user/Register";
            public const string Project = "api/Project/";
        }

        public struct ConfigEntry
        {
            public const string ApiURI = "ApiURI";
        }
        public enum ResponseType
        {
            Success = 0,
            BusinessError = 1,
            SystemError = 2
        }
    }
}
