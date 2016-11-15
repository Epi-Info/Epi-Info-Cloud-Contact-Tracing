using System.Collections.Generic;
using System.Web.Mvc;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Model;

namespace Epi.Web.MVC.Models
{
    public class FormResponseInfoModel
    {
        public FormSettingResponse FormSettingResponse;
        public FormInfoModel FormInfoModel;
        public UserModel UserModel;
        public List<ResponseModel> ResponsesList;
        public List<KeyValuePair<int, string>> Columns;
        public List<KeyValuePair<int, FieldDigest>> ColumnDigests;
        public int NumberOfPages;
        public int CurrentPage;
        public int PageSize;
        public int NumberOfResponses;
        public int ViewId;
        public string ParentResponseId;
        public string sortOrder;
        public string sortfield;
        public SearchBoxModel SearchModel;
        public List<SelectListItem> SearchColumns1;
        public List<SelectListItem> SearchColumns2;
        public List<SelectListItem> SearchColumns3;
        public List<SelectListItem> SearchColumns4;
        public List<SelectListItem> SearchColumns5;

        public FormResponseInfoModel()
        {
            FormInfoModel = new FormInfoModel();
            ResponsesList = new List<ResponseModel>();
            Columns = new List<KeyValuePair<int, string>>();
            ColumnDigests = new List<KeyValuePair<int, FieldDigest>>();
        }
    }
}