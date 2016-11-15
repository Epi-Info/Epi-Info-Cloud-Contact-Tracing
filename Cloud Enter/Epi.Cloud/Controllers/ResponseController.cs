using System;
using System.Web.Mvc;

namespace Epi.Web.Controllers
{
    public class ResponseController : Controller
    {
        [HttpGet]
        public ActionResult Index(string surveyId, string responseId, int pageNumber = 1)
        {
            try
            {
                TempData[Epi.Cloud.Common.Constants.Constant.RESPONSE_ID] = responseId;

                return RedirectToRoute(new { Controller = "Survey", Action = "Index", surveyId = surveyId, PageNumber = pageNumber });
            }
            catch (Exception ex)
            {
                Epi.Web.Utility.ExceptionMessage.SendLogMessage(ex, this.HttpContext);

                return RedirectToAction(Epi.Cloud.Common.Constants.Constant.EXCEPTION_PAGE);
            }
        }
    }
}
