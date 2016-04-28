using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Epi.Cloud.DBAccessService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var url = "https://epicloudenterdbaccessapi.azurewebsites.net/api/project/2";
            //var url = "https://localhost:44379/api/project/2";
            //var syncClient = new WebClient();
            //var content = syncClient.DownloadString(url);
            //ViewBag.Title = content;

            return View();
        }
    }
}
