//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Epi.Cloud.DataEntryServices.DAO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Epi.Cloud.Interfaces;
//using Epi.Cloud.Interfaces.MetadataInterfaces;
//using Epi.Web.Common.Interfaces;
//using Epi.Cloud.Common.BusinessObjects;
//using Epi.Cloud.MetadataServices;

//namespace Epi.Cloud.DataEntryServices.DAO.Tests
//{
//    [TestClass()]
//    public class SurveyResponseDaoTests
//    {
//        IProjectMetadataProvider _ProjectMetadataProvider;
//        ISurveyPersistenceFacade _surveyPersistenceFacade;
//        public SurveyResponseDaoTests(IProjectMetadataProvider projectMetadataProvider,
//                              ISurveyPersistenceFacade surveyPersistenceFacade)
//        {
//            _ProjectMetadataProvider = projectMetadataProvider;
//            _surveyPersistenceFacade = surveyPersistenceFacade;
//        }

    

//        [TestMethod()]
//        public void GetSurveyResponseTest()
//        {

//            IProjectMetadataProvider projectMetadataProvider;
//            ISurveyPersistenceFacade surveyPersistenceFacade;
//            //IProjectMetadataProvider ProjectMetadata = new SurveyResponseDao();
//            //SurveyResponseDao SurveyResponsetest = new SurveyResponseDao();
//            //projectMetadataProvider = 
//            SurveyResponseDao dao = new SurveyResponseDao( _ProjectMetadataProvider , _surveyPersistenceFacade);

//            List<string> surveryResponseStringList = new List<string> { "sxy9", "sfe45", "fdfe6", "fdt7" };
//            //Guid userPublishKey = Guid.Parse("257b05f2-dab2-c8e3-caed-92f0f6a88169");
            
//            List<SurveyResponseBO> list = dao.GetSurveyResponse(surveryResponseStringList, new Guid(), gridgridPageNumber:2,gridgridPageSize:10);

      
//            Assert.IsNotNull(list.Count > 0, "List is Empty" );
                       
//        }
//    }
//}