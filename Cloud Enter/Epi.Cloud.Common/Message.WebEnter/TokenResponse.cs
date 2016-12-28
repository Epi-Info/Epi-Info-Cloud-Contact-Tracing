using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a security token response message from web service to client.
    /// </summary>
    public class TokenResponse : ResponseBase
    {
        /// <summary>
        /// Default Constructor for TokenResponse.
        /// </summary>
        public TokenResponse() { }

        /// <summary>
        /// Overloaded Constructor for TokenResponse. Sets CorrelationId.
        /// </summary>
        /// <param name="correlationId"></param>
        public TokenResponse(string correlationId) : base(correlationId) { }

        /// <summary>
        /// Security token returned to the consumer
        /// </summary>
        public string AccessToken;
    }
}

