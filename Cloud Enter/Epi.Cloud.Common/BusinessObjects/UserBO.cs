using System;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class UserBO
    {
        public int UserId { get; set; }

		public string UserName { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string PasswordHash { get; set; }

		public bool ResetPassword { get; set; }

		public string EmailAddress { get; set; }

		public string PhoneNumber { get; set; }

		public int Role { get; set; }

		public OperationMode Operation { get; set; }

		public bool IsActive { get; set; }

		public int UserHighestRole { get; set; }

		public Guid UGuid { get; set; }
	}
}
