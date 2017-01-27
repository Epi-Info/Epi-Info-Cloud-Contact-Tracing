namespace Epi.Cloud.MetadataServices.Common.DataTypes
{
    public class Constants
    {
        public struct ApiEndPoints
        {
            public const string Register = "api/user/Register";
            public const string Project = "api/Project/";
            public const string PageDigest = "api/PageDigest/";
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
