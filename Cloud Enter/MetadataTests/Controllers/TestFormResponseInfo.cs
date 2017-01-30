using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Web.MVC.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Facades.Interfaces;
using Epi.Web.MVC.Models;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Epi.Cloud.MVC.Extensions;
using Moq;
using Epi.Web.MVC.Utility;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.DataPersistence.Common.Interfaces;
using Epi.Cloud.DataEntryServices;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.Metadata.Tests
{
    [TestClass()]
    public class TestFormResponseInfo 
    {

        private  ISecurityFacade _securityFacade;
        private  Epi.Cloud.CacheServices.IEpiCloudCache _cacheServices;

        private  ISurveyResponseDao _surveyResponseDao;

        private IEnumerable<AbridgedFieldInfo> _pageFields;
        protected ISurveyFacade _surveyFacade;
        protected Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider _projectMetadataProvider;
        private string _requiredList = "";

        public TestFormResponseInfo()
        {

        }
        public TestFormResponseInfo(ISurveyFacade surveyFacade,
                              ISecurityFacade securityFacade,
                              Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider projectMetadataProvider,
                              Epi.Cloud.CacheServices.IEpiCloudCache iCacheServices,
                              ISurveyResponseDao surveyResponseDao)
        {
            _surveyFacade = surveyFacade;
            _securityFacade = securityFacade;
            _projectMetadataProvider = projectMetadataProvider;
            _cacheServices = iCacheServices;
            _surveyResponseDao = surveyResponseDao;
        }
        static Mock<ISurveyFacade> surveyFacade;
        static Mock<IDataEntryService> dataEntryService;
        static Mock<ISurveyInfoService> surveyInfoService;
        static Mock<IFormSettingsService> formSettingsService;
        static Mock<ISecurityFacade> securityFacade;
        static Mock<SurveyResponseHelper> surveyResponseXML;
        static Mock<IProjectMetadataProvider> projectMetadataProvider;
        static Mock<ISecurityDataService> securityDataService;
        static Mock<ISurveyPersistenceFacade> surveyPersistenceFacade;
        static Mock<IFormInfoDao> formInfoDao;
        static Mock<ISurveyInfoDao> surveyInfoDao;
        static Mock<SurveyResponseProvider> surveyResponseProvider;
        static Mock<IFormSettingDao> formSettingDao;
        static Mock<IUserDao> userDao;



        [TestInitialize()]
        public void Initialize()
        {
            surveyFacade = new Mock<ISurveyFacade>( dataEntryService, surveyInfoService, surveyInfoService, formSettingsService);
            dataEntryService = new Mock<IDataEntryService>();
            surveyInfoService = new Mock<ISurveyInfoService>();
            formSettingsService = new Mock<IFormSettingsService>();
            securityFacade = new Mock<ISecurityFacade>();
            surveyResponseXML = new Mock<SurveyResponseHelper>();
            projectMetadataProvider = new Mock<IProjectMetadataProvider>();
            securityDataService = new Mock<ISecurityDataService>();
            surveyPersistenceFacade = new Mock<ISurveyPersistenceFacade>();
            formInfoDao = new Mock<IFormInfoDao>();
            surveyInfoDao = new Mock<ISurveyInfoDao>();
            surveyResponseProvider = new Mock<SurveyResponseProvider>();
            formSettingDao = new Mock<IFormSettingDao>();
            userDao = new Mock<IUserDao>();

                

        }



        public void GetProjectMetadataTest()
        {
            var surveyFacade = new Mock<ISurveyFacade>();
            var securityFacade = new Mock<ISecurityFacade>();
            var projectMetadataProvider = new Mock<Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider>();
            var iCacheServices = new Mock<Epi.Cloud.CacheServices.IEpiCloudCache>();
            var surveyResponseDao = new Mock<ISurveyResponseDao>();
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            string UserId = Epi.Common.Security.Cryptography.Encrypt("1014");
            mockSession.SetupGet(s => s[SessionKeys.UserId]).Returns(UserId); //somevalue
            mockSession.SetupGet(s => s[SessionKeys.CurrentOrgId]).Returns(1);
            //surveyid=2e1d01d4-f50d-4f23-888b-cd4b7fc9884b
            //formid=63035d12-0386-4e52-a16e-afcadd1d1d7c //257b05f2-dab2-c8e3-caed-92f0f6a88169
            mockSession.SetupGet(s => s[SessionKeys.ProjectId]).Returns("257b05f2-dab2-c8e3-caed-92f0f6a88169"); //somevalue
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);
            HomeController hmc = new HomeController(surveyFacade.Object, securityFacade.Object, projectMetadataProvider.Object, iCacheServices.Object, surveyResponseDao.Object);
            hmc.ControllerContext = mockControllerContext.Object;
            // Create fake Controller Context
            //var sessionItems = new SessionStateItemCollection();
            //sessionItems[SessionKeys.UserId] = "1014";
            //sessionItems[SessionKeys.ProjectId] = "f2aed655-b0a5-4f5e-8071-267f035e87a5";
            //hmc.ControllerContext = new FakeControllerContext(hmc, sessionItems);
            FormResponseInfoModel infomdl = new FormResponseInfoModel();
            infomdl = hmc.GetFormResponseInfoModel("2e1d01d4-f50d-4f23-888b-cd4b7fc9884b",1,"","",1);
            //var info = 2e1d01d4-f50d-4f23-888b-cd4b7fc9884b
            Assert.IsNotNull(infomdl, "FormResponse is Empty");
        }
    }
}