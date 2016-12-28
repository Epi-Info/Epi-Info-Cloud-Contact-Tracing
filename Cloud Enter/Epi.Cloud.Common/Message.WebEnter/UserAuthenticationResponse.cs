using System.Runtime.Serialization;
using Epi.Cloud.Common.MessageBase;
using Epi.Cloud.Common.DTO;
namespace Epi.Cloud.Common.Message
{
    public class UserAuthenticationResponse : ResponseBase
    {
        public UserAuthenticationResponse() { }

        [DataMember]
        public bool UserIsValid;
        [DataMember]
        public string PassCode;

        [DataMember]
        public UserDTO User;
    }
}
