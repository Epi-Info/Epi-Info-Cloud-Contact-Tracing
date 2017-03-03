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
            if (TempData["Message"] != null)
            {
                TempData.Keep("Message");
            }

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
            if (!string.IsNullOrEmpty(TempData["Message"].ToString()))
            {
                TempData.Keep("Message");
            }

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
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ClearMetadaCache()
        {
            try
            {
                Guid ProjectId = _epiCloudCache.GetDeployedProjectId();
                _epiCloudCache.ClearAllCache(ProjectId);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error while Clearing Cache";
            }
            TempData["Message"] = "Cache has been Cleared Successfully";
            return RedirectToAction(ViewActions.Index);
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteBlob(string ProjectId)
        {
            try
            {
                Guid BlobName = new Guid(ProjectId);
                bool IsDeleted = _metadataBlobCRUD.DeleteBlob(BlobName.ToString("N"));
                if (!IsDeleted)
                {
                    TempData["Message"] = "Blob failed to Delete";

                }
            }
            catch
            {
                TempData["Message"] = "Error while deleting Blob";
            }

            TempData["Message"] = "Blob Deleted Sucessfully";
            return RedirectToAction(ViewActions.Index);
        }

        [HttpGet]
        public ActionResult ViewBlob(string ProjectId)
        {
            try
            {
                Guid BlobName = new Guid(ProjectId);
                ViewBag.ViewDetail = _metadataBlobCRUD.DownloadText(BlobName.ToString("N"));
            }
            catch
            {
                return Json("Erorr");
            }

            return Json(new { success = true, val = ViewBag.ViewDetail }, JsonRequestBehavior.AllowGet);

        }



        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadBlob(string Cache)
        {
            try
            {
                MetadataProvider metadataProvider = new MetadataProvider();
                var metaData = metadataProvider.RetrieveProjectMetadataViaAPIAsync(Guid.Empty).Result;
                TempData["Message"] = "Blob has been uploded sucessfully";
            }
            catch
            {
                TempData["Message"] = "Error while Uploading Blob";
            }

            return RedirectToAction(ViewActions.Index);
        }

    }
}