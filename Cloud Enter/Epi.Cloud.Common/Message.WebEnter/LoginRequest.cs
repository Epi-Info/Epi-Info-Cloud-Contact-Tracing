using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a login request message from a client. Contains user credentials.
    /// </summary>
    public class LoginRequest: RequestBase
    {
        /// <summary>
        /// User name credential.
        /// </summary>
        public string UserName = "";

        /// <summary>
        /// Password credential.
        /// </summary>
        public string Password = "";
    }
}
