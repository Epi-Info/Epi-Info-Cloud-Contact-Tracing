using Epi.Cloud.Common.DTO;
using System.Runtime.Serialization;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class PublishResponse : ResponseBase
    {

        /// <summary>
        /// Default Constructor for PublishResponse.
        /// </summary>
        public PublishResponse() { }

        /// <summary>
        /// Overloaded Constructor for PublishResponse. Sets CorrelationId.
        /// </summary>
        /// <param name="correlationId"></param>
        public PublishResponse(string correlationId) : base(correlationId) { }


        /// <summary>
        /// PublishInfo object.
        /// </summary>
        public PublishInfoDTO PublishInfo;
    }
}
