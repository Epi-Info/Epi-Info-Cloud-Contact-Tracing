namespace Epi.Cloud.Common.MessageBase
{
    /// <summary>
    /// Base class for all client request messages of the web service. It standardizes 
    /// communication between web services and clients with a series of common values.
    /// Derived request message classes assign values to these variables. There are no 
    /// default values. 
    /// </summary>
    public class RequestBase
    {

        /// <summary>
        /// A unique number (ideally a Guid) issued by the client representing the instance 
        /// of the request. Avoids rapid-fire processing of the same request over and over 
        /// in denial-of-service type attacks.
        /// </summary>
        public string RequestId;


        /// <summary>
        /// Crud action: Create, Read, Update, Delete
        /// </summary>
        public string Action;
    }
}
