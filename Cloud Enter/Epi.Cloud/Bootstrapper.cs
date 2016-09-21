using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc3;
using Epi.Web.MVC.Utility;
using System.Configuration;

namespace Epi.Web.MVC
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            IUnityContainer container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            UnityContainer container = new UnityContainer();

            // register all your components with the container here
            //Configuring constructor injection. 
            //InjectedMembers: A unity container extension that allows you to configure which constructor, property or method gets injected via API
            //ConfigureInjectionFor is the API to configure injection for a particular type e.g. DataServiceClient proxy class
            //InjectionConstructor: creates an instance of Microsoft.Practices.Unity.InjectionConstructor that looks for a constructor with the given set of parameters
            // e.g. container.RegisterType<ITestService, TestService>();            

            container.RegisterType<Epi.Web.Enter.Common.Message.SurveyInfoRequest, Epi.Web.Enter.Common.Message.SurveyInfoRequest>();
            container.RegisterType<Epi.Web.WCF.SurveyService.IEWEDataService, Epi.Web.WCF.SurveyService.EWEDataService>();
            container.RegisterType<SurveyResponseDocDb, SurveyResponseDocDb>()
                .Configure<InjectedMembers>()
                .ConfigureInjectionFor<SurveyResponseDocDb>(new InjectionConstructor());

            //container.RegisterType<Epi.Web.MVC.Repositories.Core.ISurveyInfoRepository, Epi.Web.MVC.Repositories.IntegratedSurveyInfoRepository>();
            container.RegisterType<Epi.Web.MVC.Repositories.Core.ISurveyInfoRepository, Epi.Cloud.MVC.Repositories.IntegratedSurveyInfoEpiMetadataRepository>();

            container.RegisterType<Epi.Web.Enter.Common.Message.SurveyAnswerRequest, Epi.Web.Enter.Common.Message.SurveyAnswerRequest>();

            container.RegisterType<Epi.Cloud.Interfaces.DataInterface.IDataEntryService, Epi.Cloud.DataEntryServices.DataEntryService>();
            //container.RegisterType<Epi.Web.MVC.Repositories.Core.ISurveyAnswerRepository, Epi.Cloud.MVC.Repositories.IntegratedSurveyAnswerDocumentDBRepository>();
            container.RegisterType<Epi.Web.MVC.Repositories.Core.ISurveyAnswerRepository, Epi.Web.MVC.Repositories.IntegratedSurveyAnswerRepository>();

            var epiCloudCache = new Epi.Cloud.CacheServices.EpiCloudCache();
            var projectMetadataProvider = new Epi.Cloud.MetadataServices.ProjectMetadataProvider(epiCloudCache);

            container.RegisterType<Epi.Web.Enter.Common.Diagnostics.ILogger, Epi.Web.Enter.Common.Diagnostics.Logger>();
            container.RegisterType<Epi.Web.Enter.Common.DTO.SurveyAnswerDTO, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO>();
            container.RegisterType<Epi.Web.MVC.Facade.ISurveyFacade, Epi.Web.MVC.Facade.SurveyFacade>();
            container.RegisterType<Epi.Web.MVC.Facade.ISecurityFacade, Epi.Web.MVC.Facade.SecurityFacade>();
            container.RegisterInstance<Epi.Cloud.CacheServices.IEpiCloudCache>(epiCloudCache);
            container.RegisterInstance<Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider>(projectMetadataProvider);
            container.RegisterType<Epi.Cloud.DataEntryServices.Facade.ISurveyStoreDocumentDBFacade, Epi.Cloud.DataEntryServices.Facade.SurveyDocumentDBFacade>();
            container.RegisterType<Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyResponseDao, Epi.Cloud.DataEntryServices.DAO.SurveyResponseDao>();
            container.RegisterType<Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao, Epi.Cloud.DataEntryServices.DAO.SurveyInfoDao>();
            container.RegisterType<Epi.Cloud.DataEntryServices.SurveyResponseProvider, Epi.Cloud.DataEntryServices.SurveyResponseProvider>();
            container.RegisterType<Epi.Web.Enter.Interfaces.DataInterface.IFormInfoDao, Epi.Cloud.DataEntryServices.DAO.FormInfoDao>();
            container.RegisterType<Epi.Web.Enter.Interfaces.DataInterfaces.IDaoFactory, Epi.Cloud.DataEntryServices.DaoFactory>();
            container.RegisterControllers();

            return container;
        }
    }
}