#define useEF_FormSettingFacade

using System.Web.Mvc;
using Epi.Cloud.MVC.Utility;
using Microsoft.Practices.Unity;
using Unity.Mvc3;

namespace Epi.Cloud.MVC
{
    public static class Bootstrapper
    {
        public static void Initialize()
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


			container.RegisterType<SurveyResponseBuilder, SurveyResponseBuilder>()
                .Configure<InjectedMembers>()
                .ConfigureInjectionFor<SurveyResponseBuilder>(new InjectionConstructor());

			// Logger
			container.RegisterType<Epi.Common.Diagnostics.ILogger, Epi.Common.Diagnostics.Logger>();

            container.RegisterType<Epi.Cloud.Common.Message.SurveyInfoRequest, Epi.Cloud.Common.Message.SurveyInfoRequest>();
            container.RegisterType<Epi.Cloud.Common.Message.SurveyAnswerRequest, Epi.Cloud.Common.Message.SurveyAnswerRequest>();

            container.RegisterType<Epi.Cloud.Common.DTO.SurveyAnswerDTO, Epi.Cloud.Common.DTO.SurveyAnswerDTO>();
			container.RegisterType<Epi.Cloud.Common.DTO.PassCodeDTO, Epi.Cloud.Common.DTO.PassCodeDTO>();

			container.RegisterType<Epi.Cloud.DataEntryServices.SurveyResponseProvider, Epi.Cloud.DataEntryServices.SurveyResponseProvider>();

			// DAOs
            container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.ISurveyInfoDao, Epi.Cloud.SurveyInfoServices.DAO.SurveyInfoDao>();
			container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.ISurveyResponseDao, Epi.Cloud.DataEntryServices.DAO.SurveyResponseDao>();
            container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IFormInfoDao, Epi.Cloud.SurveyInfoServices.DAO.FormInfoDao>();
			//container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IFormSettingDao, Epi.Cloud.SurveyInfoServices.DAO.FormSettingDao>();
            container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IFormSettingDao_EF, Epi.Web.EF.EntityFormSettingDao>();
            container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IUserDao, Epi.Web.EF.EntityUserDao>();
			container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IOrganizationDao, Epi.Web.EF.EntityOrganizationDao>();

			// Facades
			container.RegisterType<Epi.Cloud.Facades.Interfaces.ISurveyFacade, Epi.Cloud.Facades.SurveyFacade>();
			container.RegisterType<Epi.Cloud.Facades.Interfaces.ISecurityFacade, Epi.Cloud.Facades.SecurityFacade>();
#if useEF_FormSettingFacade
            container.RegisterType<Epi.Cloud.Facades.Interfaces.IFormSettingFacade, Epi.Cloud.Facades.EF_FormSettingFacade>();
#else            
            container.RegisterType<Epi.Cloud.Facades.Interfaces.IFormSettingFacade, Epi.DataPersistenceServices.CosmosDB.Facades.CosmosDB_EF_FormSettingFacade>();
#endif
            // Data Service Interfaces
            container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IDataEntryService, Epi.Cloud.DataEntryServices.DataEntryService>();
			container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.ISurveyInfoService, Epi.Cloud.SurveyInfoServices.SurveyInfoService>();
			container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.ISecurityDataService, Epi.Cloud.DataEntryServices.SecurityDataService>();
			container.RegisterType<Epi.Cloud.Interfaces.DataInterfaces.IFormSettingsService, Epi.Cloud.DataEntryServices.FormSettingsService>();

			// Registered Instances
			var epiCloudCache = new Epi.Cloud.CacheServices.EpiCloudCache();
            var projectMetadataProvider = new Epi.Cloud.MetadataServices.ProjectMetadataProvider(epiCloudCache);
            var docDB_SurveyFacade = new Epi.PersistenceServices.CosmosDB.DocDB_SurveyPersistenceFacade();

			container.RegisterInstance<Epi.Cloud.CacheServices.IEpiCloudCache>(epiCloudCache);
            container.RegisterInstance<Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider>(projectMetadataProvider);
            container.RegisterInstance<Epi.DataPersistence.Common.Interfaces.ISurveyPersistenceFacade >(docDB_SurveyFacade);

			container.RegisterControllers();

            return container;
        }
    }
}