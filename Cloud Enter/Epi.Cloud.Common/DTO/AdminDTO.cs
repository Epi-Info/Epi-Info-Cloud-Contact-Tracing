namespace Epi.Cloud.Common.DTO
{
    public class AdminDTO
    {
        private string _AdminEmail;
        private string _OrganizationId;
        private bool _IsActive;


        public string AdminEmail
        {
            get { return _AdminEmail; }
            set { _AdminEmail = value; }
        }


        public string OrganizationId
        {
            get { return _OrganizationId; }
            set { _OrganizationId = value; }
        }


        public bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }


    }
}
