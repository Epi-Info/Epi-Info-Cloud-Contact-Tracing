using System.Runtime.Serialization;

namespace Epi.Common.Exception
{
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class CustomFaultException
    {
        private string _customMessage;
        private string _source;
        private string _stackTrace;
        private string _helpLink;

        public CustomFaultException()
        {
        }

        public CustomFaultException(System.Exception ex)
        {
            CustomMessage = ex.Message;
            Source = ex.Source;
            StackTrace = ex.StackTrace;
            HelpLink = ex.HelpLink;
        }

        [DataMember]
        public string CustomMessage
        {
            get { return _customMessage; }
            set { _customMessage = value; }
        }

        [DataMember]
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        [DataMember]
        public string StackTrace
        {
            get { return _stackTrace; }
            set { _stackTrace = value; }
        }

        [DataMember]
        public string HelpLink
        {
            get { return _helpLink; }
            set { _helpLink = value; }
        }
    }
}
