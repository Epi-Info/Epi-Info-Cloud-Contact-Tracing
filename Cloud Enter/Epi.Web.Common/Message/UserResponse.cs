using System.Runtime.Serialization;
using Epi.Cloud.Common.DTO;
using System.Collections.Generic;

namespace Epi.Web.Enter.Common.Message
{
    public class UserResponse
    {

        [DataMember]
        public List<UserDTO> User;
        [DataMember]
        public string Message;
    }
}
