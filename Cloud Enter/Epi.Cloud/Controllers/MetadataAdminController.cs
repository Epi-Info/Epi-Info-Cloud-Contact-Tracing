using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Epi.Cloud.MetadataServices.Common.MetadataBlobService;
using Epi.Cloud.Common.Configuration;
using Epi.Web.MVC.Models;
using Epi.Cloud.MetadataServices.Common;
using System.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.Common.Constants;
using Epi.Cloud.CacheServices;

namespace Epi.Web.MVC.Controllers
{
    public class MetadataAdminController : Controller
    {
        private readonly IEpiCloudCache _epiCloudCache;

        static string containerName = ConfigurationManager.AppSettings[AppSettings.Key.MetadataBlogContainerName];
        MetadataBlobCRUD _metadataBlobCRUD = new MetadataBlobCRUD(containerName);
        protected List<KeyValuePair<int, string>> Columns = new List<KeyValuePair<int, string>>();

        public MetadataAdminController(IEpiCloudCache epiCloudCache)
        {
            _epiCloudCache = epiCloudCache;
        }


        [HttpGet]
        public ActionResult Index()
        {
            MetadataAdmin metaadmin = new MetadataAdmin();


            Dictionary<int, string> ColumnNameList = new Dictionary<int, string>();

            ColumnNameList.Add(0, BlobMetadataKeys.ProjectId);
            ColumnNameList.Add(1, BlobMetadataKeys.ProjectName);
            ColumnNameList.Add(2, BlobMetadataKeys.PublishDate);

            metaadmin.Columns = ColumnNameList;


            var BlobList = _metadataBlobCRUD.GetBlobList(Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata);

            metaadmin.PageSize = 1;
            metaadmin.CurrentPage = 1;
            metaadmin.NumberOfPages = 1;
            metaadmin.NumberOfResponses = 1;

            foreach (var blob in BlobList)
            {
                Dictionary<string, string> metaProp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(blob);
                ResponseModel res = new ResponseModel();

                res.Column0Key = BlobMetadataKeys.ProjectId;
                res.Column0 = metaProp[BlobMetadataKeys.ProjectId];
                res.Column1Key = BlobMetadataKeys.ProjectName;
                res.Column1 = metaProp[BlobMetadataKeys.ProjectName];
                res.Column2Key = BlobMetadataKeys.PublishDate;
                res.Column2 = metaProp[BlobMetadataKeys.PublishDate];

                metaadmin.BlobResponsesList.Add(res);
            }

            metaadmin.MetadataAdminModel = metaadmin;

            return View(metaadmin);
        }

        [HttpPost]
        public ActionResult Index(string id)
        {


            return View(Epi.Cloud.Common.Constants.Constant.INDEX_PAGE);
        }

        [HttpGet]
        public ActionResult ClearMetadaCache(Guid ProjectId)
        {            
            _epiCloudCache.ClearAllCache(ProjectId);
            return View(Epi.Cloud.Common.Constants.Constant.INDEX_PAGE);
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteBlob(Guid blobname)
        {
            bool IsDeleted = _metadataBlobCRUD.DeleteBlob(blobname.ToString("N"));
            return View(Epi.Cloud.Common.Constants.Constant.INDEX_PAGE);
        }


        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadBlob(string Cache)
        {
            MetadataProvider metadataProvider = new MetadataProvider();
            var metaData = metadataProvider.RetrieveProjectMetadataViaAPIAsync(Guid.Empty).Result;
            return View(Epi.Cloud.Common.Constants.Constant.INDEX_PAGE);
        }

    }
}