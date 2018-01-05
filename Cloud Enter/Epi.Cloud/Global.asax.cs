using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using Epi.Cloud.Common.Configuration;
using Epi.Common.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.TraceListener;

namespace Epi.Cloud.MVC
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        ILogger _logger;

        public MvcApplication()
        {
        }

        public MvcApplication(ILogger logger)
        {
            _logger = logger;
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
         
        public static void RegisterRoutes(RouteCollection routes)
        {
    
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Android") { ContextCondition = (context => context.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0) });
            //DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Opera") { ContextCondition = (context => context.Request.UserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0) });
              

            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile") { ContextCondition = (context => context.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0) });
            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile") { ContextCondition = (context => context.Request.UserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0) });
            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile") { ContextCondition = (context => context.Request.UserAgent.IndexOf("iPad", StringComparison.OrdinalIgnoreCase) >= 0) });

            //DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("iPhone") { ContextCondition = (context => context.Request.UserAgent.IndexOf("iPhone", StringComparison.OrdinalIgnoreCase) >= 0) });
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrapper.Initialize();

           // DisplayModes.Modes.Insert(0, new  DefaultDisplayMode("Mobile") 
            //DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile") 
            //{ 
            //    ContextCondition = (ctx => ctx.Request.UserAgent != null 
            //                                && (ctx.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0 
            //                                    || ctx.Request.UserAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0 
            //                                    || ctx.Request.UserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0
            //                                    || ctx.Request.UserAgent.IndexOf("Opera", StringComparison.OrdinalIgnoreCase) >= 0
            //                                    || ctx.Request.UserAgent.IndexOf("opera", StringComparison.OrdinalIgnoreCase) >= 0 
            //                                    || ctx.Request.UserAgent.IndexOf("Opera Mini", StringComparison.OrdinalIgnoreCase) >= 0)) 
            //});


           
            
           
          
        }


        /// <summary>
        ///  HKLM\SYSTEM\CurrentControlSet\services\eventlog needs to be set 
        ///  in order to use the event log
        /// </summary>
        protected void Application_Error()
        {

            Exception exc = Server.GetLastError();

            try
            {
				//string sSource;
				//string sLog;
				//string sEvent;

				//sSource = "Epi.Web.Survey";
				//sLog = "Application";
				//sEvent = exc.Message + "\n" + exc.StackTrace;

				Epi.Web.Utility.ExceptionMessage.SendLogMessage(exc);

                try
                {
                    ILogger logger = DependencyHelper.DependencyResolver.GetService<ILogger>();
                    if (logger != null)
                    {
                        logger.Error(exc.ToString());
                    }
                    TelemetryClient telemetryClient = new TelemetryClient();
                    if (telemetryClient != null)
                    {
                        telemetryClient.TrackException(exc);
                    }
                }
                catch (Exception tex)
                {
                    Epi.Web.Utility.ExceptionMessage.SendLogMessage(tex);
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }

            this.Response.Redirect("/", true);
        }
    }
}

