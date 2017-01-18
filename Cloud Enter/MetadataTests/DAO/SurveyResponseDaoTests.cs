using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.DataEntryServices.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Interfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
//using Epi.Web.Common.Interfaces;
//using Epi.Web.Enter.Common.BusinessObject;
using Epi.Cloud.MetadataServices;
using Epi.DataPersistence.Common;
using Epi.DataPersistence.Common.Interfaces;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.BusinessObjects;
using Moq;

namespace Epi.Cloud.DataEntryServices.DAO.Tests
{
    [TestClass()]
    public class SurveyResponseDaoTests : MetadataAccessor
    {
        IProjectMetadataProvider _ProjectMetadataProvider;
        DataPersistence.Common.Interfaces.ISurveyPersistenceFacade _surveyPersistenceFacade;

        public void SurveyResponseDaoTsts(IProjectMetadataProvider projectMetadataProvider,
                              DataPersistence.Common.Interfaces.ISurveyPersistenceFacade surveyPersistenceFacade)
        {
            ProjectMetadataProvider = projectMetadataProvider;
            _surveyPersistenceFacade = surveyPersistenceFacade;
        }



        [TestMethod()]
        public void GetSurveyResponseTest()
        {
            var projectMetadataProvider = new Mock<IProjectMetadataProvider>();
            projectMetadataProvider.SetupAllProperties();
            var surveyPersistenceFacade = new Mock<ISurveyPersistenceFacade>();
            surveyPersistenceFacade.SetupAllProperties();

            //ISurveyPersistenceFacade surveyPersistenceFacade; // = new ISurveyPersistenceFacade();
            Guid projectguid = Guid.Parse("257b05f2-dab2-c8e3-caed-92f0f6a88169");

            //IProjectMetadataProvider ProjectMetadata = new SurveyResponseDao();
            //SurveyResponseDao SurveyResponsetest = new SurveyResponseDao();
            //projectMetadataProvider = 
            SurveyResponseDao dao = new SurveyResponseDao(projectMetadataProvider.Object, surveyPersistenceFacade.Object);// _surveyPersistenceFacade);ProjectMetadataProvider

            List<string> surveryResponseStringList = new List<string> { "sxy9", "sfe45", "fdfe6", "fdt7" };
            //Guid userPublishKey = Guid.Parse("257b05f2-dab2-c8e3-caed-92f0f6a88169");

            List<SurveyResponseBO> list = dao.GetSurveyResponse(surveryResponseStringList, projectguid, gridgridPageNumber: 2, gridgridPageSize: 10);



            Assert.IsNotNull(list.Count > 0, "List is Empty");

        }
    }
}
