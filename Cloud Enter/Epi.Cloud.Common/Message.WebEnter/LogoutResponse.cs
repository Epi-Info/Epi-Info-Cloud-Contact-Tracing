using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a logout response message from web service to client.
    /// </summary>
    public class LogoutResponse : ResponseBase
    {
        /// <summary>
        /// Default Constructor for LogoutResponse.
        /// </summary>
        public LogoutResponse() { }

        /// <summary>
        /// Overloaded Constructor for LogoutResponse. Sets CorrelationId.
        /// </summary>
        /// <param name="correlationId"></param>
        public LogoutResponse(string correlationId) : base(correlationId) { }
    }
}