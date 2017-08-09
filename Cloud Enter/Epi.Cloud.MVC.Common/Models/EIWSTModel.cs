namespace Epi.Cloud.MVC.Models
{
    public class EIWSTModel
    {
        private string _DataBaseTestStatus;
        public string DBTestStatus
        {
            get { return _DataBaseTestStatus; }
            set { _DataBaseTestStatus = value; }
        }

        private string _ServiceTestStatus;
        public string STestStatus
        {
            get { return _ServiceTestStatus; }
            set { _ServiceTestStatus = value; }
        }
        private string _EFTestStatus;
        public string EFTestStatus
        {
            get { return _EFTestStatus; }
            set { _EFTestStatus = value; }
        }
        private bool _IsManagerServiceTest;
        public bool IsManagerServiceTest
        {
            get { return _IsManagerServiceTest; }
            set { _IsManagerServiceTest = value; }
        }
    }
}