using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Epi.Web.MVC.Models
{
    public class MetadataAdmin
    {
        public Dictionary<int, string> Columns;
        public MetadataAdmin MetadataAdminModel;
        public int NumberOfPages;
        public int CurrentPage;
        public int PageSize;
        public int NumberOfResponses;
        public int ViewId;
        public string ParentResponseId;
        public string sortOrder;
        public string sortfield;
        public List<ResponseModel> BlobResponsesList;
        public string ViewDetail;

        public MetadataAdmin()
        {
            BlobResponsesList = new List<ResponseModel>();
        }
    }
}
