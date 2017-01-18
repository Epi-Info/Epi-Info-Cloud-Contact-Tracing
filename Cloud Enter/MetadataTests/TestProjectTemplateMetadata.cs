using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Epi.FormMetadata.DataStructures;
using System.Linq;
//using Epi.Cloud.Interfaces.MetadataInterfaces;

namespace Epi.Web.SurveyManager.Test
{
    [TestClass]
    public class TestProjectTemplateMetadata
    {
        //private Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider _projectMetadataProvider;
        [TestMethod]
        public void GetProjectMetaData()
        {
            //Template projectMetadata = _projectMetadataProvider.GetProjectMetadataAsync(ProjectScope.TemplateWithNoPages).Result;
            //Assert.IsNotNull(projectMetadata);

            //projectMetadata = _projectMetadataProvider.GetProjectMetadataAsync(ProjectScope.TemplateWithAllPages).Result;
            Template projectMetadata = MockTemplateData();
            Assert.IsNotNull(projectMetadata, "Template Data is Empty");

            if (projectMetadata != null)
            {
                //// check for parentView

                //var parentView = projectMetadata.Project.Views[0];
                foreach (var view in projectMetadata.Project.Views)
                {
                    Assert.IsNotNull(view.FormId,"FormId is Null");
                    Assert.IsTrue(view.ParentFormId != null ? projectMetadata.Project.Views.Any(v => v.FormId == view.ParentFormId) : true, "Child view no corresponding Parent");
                    if (view.Pages.Length > 0)
                    {
                        foreach (var page in view.Pages)
                        {

                            Assert.IsNotNull(page.PageId, "PageID is null");
                            Assert.IsNotNull(page.ViewId, "ViewID is null");
                            Assert.IsTrue(page.Fields.Length > 0, " fields available ");
                        }
                    }
                }
            }

          
           
        }
                                                                                                                                     

        public Template MockTemplateData()
        {


            var json = System.IO.File.ReadAllText(@"c:\junk\ZikaMetadataFromService.json");
            Template metadataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Template>(json);

            //Template template = new Template();
            //template.Name = "name";
            //template.CreateDate = DateTime.Now.ToString();
            //template.Project = new Project() {
            //    FormDigests = new FormDigest[] { new FormDigest() { NumberOfPages = 1 }, new FormDigest() { NumberOfPages = 1 } },
            //    FormPageDigests = new PageDigest[2][],
            //    Views=new View[] { new View() { FormId="2"} },


            //};
            //template.Project.FormPageDigests[0] =new PageDigest[] { new PageDigest() { FormId = "2" } };

            return metadataObject;
        }


    }
}




