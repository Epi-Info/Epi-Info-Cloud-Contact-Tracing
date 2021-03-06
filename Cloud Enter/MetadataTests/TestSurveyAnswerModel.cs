﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.Cloud.MVC.Models;
using System.Linq;
using Epi.Cloud.Common.DTO;

namespace MetadataTests
{
    [TestClass]
    public class TestSurveyAnswerModel
    {
        private const int SurveyAnswerModelPropertyCount = 13;
        private const int SurveyAnswerDTOPropertyCount = 35;
        [TestMethod]
        public void TestSurveyAnswerDTO()
        {

            SurveyAnswerModel objSurveyAnswerModel = new SurveyAnswerModel();
            SurveyAnswerDTO objSurveyAnswerDTO = new SurveyAnswerDTO();
            int numberOfPublicPropertiesofSurveyAnswerDTO = objSurveyAnswerDTO.GetType().GetProperties().Count();
            int numberOfPublicPropertiesofSurveyAnswerModel = objSurveyAnswerModel.GetType().GetProperties().Count();
            Assert.AreEqual(numberOfPublicPropertiesofSurveyAnswerModel, SurveyAnswerModelPropertyCount, "SurveyAnswerModel properties has been changed");
            Assert.AreEqual(numberOfPublicPropertiesofSurveyAnswerDTO, SurveyAnswerDTOPropertyCount, "SurveyAnswerDTO properties has been changed");

        }
    }
}
