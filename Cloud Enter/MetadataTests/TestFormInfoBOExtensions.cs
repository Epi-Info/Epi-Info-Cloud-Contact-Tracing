using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.MVC.Extensions;
using System.Linq;
using Epi.Cloud.Common.DTO;

namespace MetadataTests
{
    [TestClass]
    public class TestFormInfoBOExtensions
    {
       private const int FormInfoDTOPropertyCount = 17;
       private const int FormInfoBOPropertyCount = 17;

        [TestMethod]
        public void TestFormInfoBODTO()
        {

            //var BO = new FormInfoBO();
            ////{ //FormId = "2e1d01d4-f50d-4f23-888b-cd4b7fc9884b", FormName = "Zika", UserId = 1014 }
            //var Forminfo = BO.ToFormInfoDTO();


            //int numberOfPublicProperties = typeof(FormInfoBO).GetProperties(System.Reflection.BindingFlags.Public).Count();
            //int numberOfPublicPropertiesforDTO = typeof(FormInfoDTO).GetProperties(System.Reflection.BindingFlags.Public).Count();
            ////Assert.AreEqual(BO.UserId, Forminfo.UserId);
            ////Assert.AreEqual(BO.FormId, Forminfo.FormId);


            //Assert.AreEqual(numberOfPublicProperties, numberOfPublicPropertiesforDTO);

            var BO = new FormInfoBO();
            var Forminfo = BO.ToFormInfoDTO();


            FormInfoBO objFormInfoBO = new FormInfoBO();
            
            //Class Instance 
            FormInfoDTO objFormInfoDTO = new FormInfoDTO();
            // From Extension Method
           // var objFormInfoBOExtensions = FormInfoBOExtensions.ToFormInfoDTO(BO);

           
            int numberOfPublicPropertiesofDTO = objFormInfoDTO.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofBO = objFormInfoBO.GetType().GetProperties().Count();

            Assert.AreEqual(numberOfPublicPropertiesofDTO, FormInfoDTOPropertyCount, "FormInfoDTO  properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofBO, FormInfoBOPropertyCount, "FormInfoBO  properties has been changed");
      
            //Assert.AreEqual(numberOfPublicPropertiesofDTO, numberOfPublicPropertiesofBO);

        }
    }
}

