using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.XPath;
using Epi.Web.Enter.Common.BusinessObject;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlClient;
namespace Epi.Web.BLL
{
   /// <summary>
   /// 
   /// </summary>
  
    public class Publisher
    {
        private Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao SurveyInfoDao;
        private Epi.Web.Enter.Interfaces.DataInterfaces.IOrganizationDao OrganizationDao;
        Dictionary<int, int> ViewIds = new Dictionary<int, int>();
        Dictionary<int, string> ViewIdNames = new Dictionary<int, string>();
        Dictionary<int, List<string>> ViewColumnNames = new Dictionary<int, List<string>>();
        #region"Public members"
        /// <summary>
        ///  This class is used to process the object sent from the WCF service “SurveyManager”, 
        ///  save survey info into SurvayMetaData Table 
        ///  and then returns a URL, StatusText and IsPublished indicator.
        /// </summary>
        /// <param name="pRequestMessage"></param>
        /// <returns></returns>
        /// 
        public Publisher(Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao pSurveyInfoDao, Epi.Web.Enter.Interfaces.DataInterfaces.IOrganizationDao pPrganizationDao)
        {
            this.SurveyInfoDao = pSurveyInfoDao;
            this.OrganizationDao = pPrganizationDao;
        }
        public Publisher()
        {
            
        }
        public SurveyRequestResultBO PublishSurvey(SurveyInfoBO  pRequestMessage)
        {
        SurveyRequestResultBO result = new SurveyRequestResultBO();
            this.ViewIds = pRequestMessage.RelateViewIds;
            this.ViewIdNames = pRequestMessage.ViewIdNames;
            if (pRequestMessage.ViewIdNames.Count>1)
            {
            result = PublishRelatedFormSurvey(pRequestMessage);
            }
        else
            {
            result = Publish(pRequestMessage);
                 
            }
            return result;
        }      
        public SurveyRequestResultBO RePublishSurvey(SurveyInfoBO pRequestMessage)
        {
            this.ViewIds = pRequestMessage.RelateViewIds;
            this.ViewIdNames = pRequestMessage.ViewIdNames;
            SurveyRequestResultBO result = new SurveyRequestResultBO();
            if (pRequestMessage.ViewIdNames.Count > 1)
                {
                result = RePublishRelatedFormSurvey(pRequestMessage);
                }
            else
                {
                result = RePublish(pRequestMessage);
                }
            return result;
        }

        private static bool ValidateSurveyFields(SurveyInfoBO pRequestMessage)
        {

            bool isValid = true;


            if (pRequestMessage.ClosingDate == null)
            {

                isValid = false;

            }
            
            //else if (string.IsNullOrEmpty(pRequestMessage.XML) || string.IsNullOrWhiteSpace(pRequestMessage.XML))
            //{

            //    isValid = false;
            //}
            else if (string.IsNullOrEmpty(pRequestMessage.SurveyName))
                {

                isValid = false;
                }
            
            else if ( string.IsNullOrEmpty(pRequestMessage.UserPublishKey.ToString()))
            {

                isValid = false;
            }


 
            return isValid;        
        }
     
        /// <summary>
        /// validate the Organization key passed with the list of Organization keys retrieved from database 
        /// through EF
        /// </summary>
        /// <param name="OrganizationKey"></param>
        /// <returns></returns>
        private bool ValidateOrganizationKey(Guid gOrganizationKey)
        {
            string strOrgKeyEncrypted = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(gOrganizationKey.ToString());
            List<OrganizationBO> OrganizationBoList = this.OrganizationDao.GetOrganizationInfoByOrgKey(strOrgKeyEncrypted);
            if (OrganizationBoList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;    
            }
            
           
        }

        #endregion

        #region "Private members"
        private string GetURL(SurveyInfoBO  pRequestMessage, Guid SurveyId)
        {
            System.Text.StringBuilder URL = new System.Text.StringBuilder();
            URL.Append(System.Configuration.ConfigurationManager.AppSettings["URL"]);
           // URL.Append("/");
            //URL.Append(pRequestMessage.SurveyNumber.ToString());
            //URL.Append("/");
           // URL.Append(SurveyId.ToString());
            return URL.ToString();
        }
     
              
        private SurveyRequestResultBO Publish(SurveyInfoBO pRequestMessage)
            {
            SurveyRequestResultBO result = new SurveyRequestResultBO();

           
                var SurveyId = Guid.NewGuid();

                if (pRequestMessage != null)
                    {

                    //if (! string.IsNullOrEmpty(pRequestMessage.SurveyNumber)  &&  ValidateOrganizationKey(pRequestMessage.OrganizationKey))
                        if (ValidateOrganizationKeyByUser(pRequestMessage.OrganizationKey, pRequestMessage.OwnerId))//EW-96
                        {

                        if (ValidateSurveyFields(pRequestMessage))
                            {
                            try
                                {
                                   // if (pRequestMessage.IsSqlProject)
                                        // this.SurveyInfoDao.ValidateServername(pRequestMessage);
                                    var BO = ToBusinessObject(pRequestMessage, SurveyId); 
                                    this.SurveyInfoDao.InsertSurveyInfo(BO);
                                
                                //Insert Connection string..
                                if (pRequestMessage.IsSqlProject)
                                    {
                                DbConnectionStringBO DbConnectionStringBO = new DbConnectionStringBO();
                                DbConnectionStringBO = GetConnection(pRequestMessage.DBConnectionString);
                                DbConnectionStringBO.SurveyId = SurveyId;
                                this.SurveyInfoDao.InsertConnectionString(DbConnectionStringBO);
                                    }

                                List<string> List = new List<string>();
                            //    var list= ViewColumnNames.Where(x => x.Key == BO.ViewId);
                            //foreach (var item in list)
                            //{
                            //    List.Add(item.Value.ToString());
                            //}

                            //// Set Survey Settings
                            //this.SurveyInfoDao.InsertFormdefaultSettings(SurveyId.ToString(), pRequestMessage.IsSqlProject, List);
                            Dictionary<int, string> SurveyIdsList = new Dictionary<int, string>();
                            //SurveyIdsList.Add(GetViewId(pRequestMessage.XML), SurveyId.ToString());
                            SurveyIdsList.Add(BO.ViewId, SurveyId.ToString());
                            result.ViewIdAndFormIdList = SurveyIdsList;
                                result.URL = GetURL(pRequestMessage, SurveyId);
                                result.IsPulished = true;
                                }
                            catch (Exception ex)
                                {
                                System.Console.Write(ex.ToString());
                                //Entities.ObjectStateManager.GetObjectStateEntry(SurveyMetaData).Delete();
                                result.URL = "";
                                result.IsPulished = false;
                                result.StatusText = "An Error has occurred while publishing your survey.";
                                }




                            }
                        else
                            {

                            result.URL = "";
                            result.IsPulished = false;
                            result.StatusText = "One or more survey required fields are missing values.";
                            }

                        }
                    else
                        {

                        result.URL = "";
                        result.IsPulished = false;
                        result.StatusText = "Organization Key is invalid.";

                        }
                    }
               
            return result;
            }

        private static SurveyInfoBO ToBusinessObject(SurveyInfoBO pRequestMessage, Guid SurveyId)
        {
            Epi.Web.Enter.Common.BusinessObject.SurveyInfoBO BO = new Epi.Web.Enter.Common.BusinessObject.SurveyInfoBO();
            BO.SurveyId = SurveyId.ToString();
            BO.ClosingDate = pRequestMessage.ClosingDate;

            BO.IntroductionText = pRequestMessage.IntroductionText;
            BO.ExitText = pRequestMessage.ExitText;
            BO.DepartmentName = pRequestMessage.DepartmentName;
            BO.OrganizationName = pRequestMessage.OrganizationName;

            BO.SurveyNumber = pRequestMessage.SurveyNumber;

            BO.XML = pRequestMessage.XML;

            BO.SurveyName = pRequestMessage.SurveyName;

            BO.SurveyType = pRequestMessage.SurveyType;
            BO.UserPublishKey = pRequestMessage.UserPublishKey;
            BO.OrganizationKey = pRequestMessage.OrganizationKey;
            BO.OrganizationKey = pRequestMessage.OrganizationKey;
            BO.TemplateXMLSize = pRequestMessage.TemplateXMLSize;
            BO.IsDraftMode = pRequestMessage.IsDraftMode;
            BO.StartDate = pRequestMessage.StartDate;
            BO.ViewId = pRequestMessage.ViewId;
            //BO.ParentId = pRequestMessage.ParentId;// Removed parent Id for the first step of publish
            BO.OwnerId = pRequestMessage.OwnerId;
            BO.IsSqlProject = pRequestMessage.IsSqlProject;
            BO.IsShareable = pRequestMessage.IsShareable;
            BO.DataAccessRuleId = pRequestMessage.DataAccessRuleId;
            BO.RelateViewIds = pRequestMessage.RelateViewIds;
            //Insert Survey MetaData
            return BO;
        }

        private DbConnectionStringBO GetConnection(string ConnectionString)
            {
            DbConnectionStringBO DbConnectionStringBO = new Enter.Common.BusinessObject.DbConnectionStringBO();
            //string connStr = "Data Source=SERVERx;Initial Catalog=DBx;User ID=u;Password=p";
          // string connStr =  "Data Source=ETIEX-022/SQLEXPRESS;Initial Catalog=TestEpi;Integrated Security=True";
            var csb = new SqlConnectionStringBuilder(ConnectionString);

            DbConnectionStringBO.DatasourceServerName = csb.DataSource;
            DbConnectionStringBO.InitialCatalog= csb.InitialCatalog;
            DbConnectionStringBO.Password = csb.Password;
            DbConnectionStringBO.DatabaseUserID = csb.UserID;
            DbConnectionStringBO.PersistSecurityInfo = csb.IntegratedSecurity.ToString();

            DbConnectionStringBO.DatabaseType = "SQL";
            return DbConnectionStringBO;
            }

        private SurveyRequestResultBO RePublish(SurveyInfoBO pRequestMessage)
            {

            SurveyRequestResultBO result = new SurveyRequestResultBO();
            
                var SurveyId = new Guid(pRequestMessage.SurveyId);

                if (pRequestMessage != null)
                    {

                    //if (! string.IsNullOrEmpty(pRequestMessage.SurveyNumber)  &&  ValidateOrganizationKey(pRequestMessage.OrganizationKey))                    
                        if (ValidateOrganizationKeyByUser(pRequestMessage.OrganizationKey,pRequestMessage.OwnerId))//EW-96
                        {

                        if (ValidateSurveyFields(pRequestMessage))
                            {
                            try
                                {

                                    //if (pRequestMessage.IsSqlProject)
                                       // this.SurveyInfoDao.ValidateServername(pRequestMessage);
                                    this.SurveyInfoDao.UpdateSurveyInfo(ToBusinessObject(pRequestMessage, SurveyId));
                                ////Insert Connection string..
                                //DbConnectionStringBO DbConnectionStringBO = new DbConnectionStringBO();
                                //DbConnectionStringBO = GetConnection(pRequestMessage.DBConnectionString);
                                //DbConnectionStringBO.SurveyId = SurveyId;
                                //this.SurveyInfoDao.InsertConnectionString(DbConnectionStringBO);
                                var BO = ToBusinessObject(pRequestMessage, SurveyId);                           
                            List<string> List = new List<string>();
                            //var list = ViewColumnNames.Where(x => x.Key == BO.ViewId);
                            //foreach (var item in list)
                            //{
                            //    List.Add(item.Value.ToString());
                            //}
                            //this.SurveyInfoDao.InsertFormdefaultSettings(SurveyId.ToString(), pRequestMessage.IsSqlProject, List);
                            Dictionary<int, string> SurveyIdsList = new Dictionary<int, string>();
                            //SurveyIdsList.Add(GetViewId(pRequestMessage.XML), SurveyId.ToString());
                            SurveyIdsList.Add(BO.ViewId, SurveyId.ToString());
                            result.ViewIdAndFormIdList = SurveyIdsList;
                                result.URL = GetURL(pRequestMessage, SurveyId);
                                result.IsPulished = true;
                                }
                            catch (Exception ex)
                                {
                                System.Console.Write(ex.ToString());
                                //Entities.ObjectStateManager.GetObjectStateEntry(SurveyMetaData).Delete();
                                result.URL = "";
                                result.IsPulished = false;
                                result.StatusText = "An Error has occurred while publishing your survey.";
                                }




                            }
                        else
                            {

                            result.URL = "";
                            result.IsPulished = false;
                            result.StatusText = "One or more survey required fields are missing values.";
                            }

                        }
                    else
                        {

                        result.URL = "";
                        result.IsPulished = false;
                        result.StatusText = "Organization Key is invalid.";

                        }
                    }
                
            return result;
            }

        private SurveyRequestResultBO RePublishRelatedFormSurvey(SurveyInfoBO pRequestMessage)
            {
            SurveyRequestResultBO SurveyRequestResultBO = new Web.Enter.Common.BusinessObject.SurveyRequestResultBO();
            Dictionary<int, int> ViewIds = new Dictionary<int, int>();
            Dictionary<int, string> SurveyIds = new Dictionary<int, string>();
         
            List<SurveyInfoBO> FormsHierarchyIds = this.GetFormsHierarchyIdsByRootId(pRequestMessage.SurveyId.ToString());           
            string ParentId = "";

            // 2- call publish() with each of the views
            foreach (KeyValuePair<int, string> viewidname in ViewIdNames)
            {
              //  XDocument xdoc = XDocument.Parse(Xml);
                SurveyInfoBO SurveyInfoBO = new SurveyInfoBO();               

                SurveyInfoBO = pRequestMessage;
                SurveyInfoBO.XML = " ";
                SurveyInfoBO.SurveyName = viewidname.Value.ToString();
                SurveyInfoBO.ViewId = viewidname.Key;
                var ViewExists = FormsHierarchyIds.Where(x => x.ViewId == viewidname.Key);
                if (ViewExists.Count() > 0)
                    {
                    SurveyInfoBO pBO = FormsHierarchyIds.Single(x => x.ViewId == viewidname.Key);
                    SurveyInfoBO.SurveyId = pBO.SurveyId;
                    SurveyInfoBO.ParentId = pBO.ParentId;
                    SurveyInfoBO.UserPublishKey = pBO.UserPublishKey;
                    SurveyInfoBO.OwnerId = pRequestMessage.OwnerId;
                    SurveyInfoBO.IsSqlProject = pRequestMessage.IsSqlProject;
                    SurveyInfoBO.IsShareable = pRequestMessage.IsShareable;
                    SurveyInfoBO.DBConnectionString = pRequestMessage.DBConnectionString;
                    SurveyRequestResultBO = RePublish(SurveyInfoBO);
                    }
                else {
                        SurveyInfoBO.XML = " ";
                        SurveyInfoBO.SurveyName = viewidname.Value.ToString();
                        SurveyInfoBO.ViewId = viewidname.Key;
                        SurveyInfoBO.ParentId = ParentId;
                        SurveyInfoBO.OwnerId = pRequestMessage.OwnerId;
                        SurveyInfoBO.IsSqlProject = pRequestMessage.IsSqlProject;
                        SurveyInfoBO.IsShareable = pRequestMessage.IsShareable;
                        SurveyInfoBO.DBConnectionString = pRequestMessage.DBConnectionString;
                      SurveyRequestResultBO = Publish(SurveyInfoBO);
                     
                    }
                ParentId = SurveyRequestResultBO.ViewIdAndFormIdList[viewidname.Key];
                SurveyIds.Add(viewidname.Key, ParentId);
                
                }
            foreach (var _ViewId in this.ViewIds)
                {

                string PId = SurveyIds[_ViewId.Value].ToString();
                string SId = SurveyIds[_ViewId.Key].ToString();
                this.SurveyInfoDao.UpdateParentId(SId, _ViewId.Key, PId);

                }
        
            return SurveyRequestResultBO;
            }

        private List<SurveyInfoBO> GetFormsHierarchyIdsByRootId(string RootId)
            {
            List<SurveyInfoBO> FormsHierarchyIds = new List<SurveyInfoBO>();
            FormsHierarchyIds = this.SurveyInfoDao.GetFormsHierarchyIdsByRootId(RootId);
            return FormsHierarchyIds;
            }

        private SurveyRequestResultBO PublishRelatedFormSurvey(SurveyInfoBO pRequestMessage)
            {

            SurveyRequestResultBO SurveyRequestResultBO = new Web.Enter.Common.BusinessObject.SurveyRequestResultBO();          
            Dictionary<int, string> SurveyIds = new Dictionary<int, string>();
            string ParentId = "";
                         

            // 2- call publish() with each of the views
            foreach (KeyValuePair<int,string> viewidname in ViewIdNames)
            {          
            SurveyInfoBO SurveyInfoBO = new SurveyInfoBO();         
 
            SurveyInfoBO = pRequestMessage;
            SurveyInfoBO.XML = " ";
            SurveyInfoBO.SurveyName = viewidname.Value.ToString();
            SurveyInfoBO.ViewId = viewidname.Key;
            SurveyInfoBO.ParentId = ParentId;
            SurveyInfoBO.OwnerId = pRequestMessage.OwnerId ;
            SurveyInfoBO.IsSqlProject = pRequestMessage.IsSqlProject;
            SurveyInfoBO.IsShareable = pRequestMessage.IsShareable;
            SurveyInfoBO.DBConnectionString = pRequestMessage.DBConnectionString;
            SurveyRequestResultBO = Publish(SurveyInfoBO);          
            if (SurveyRequestResultBO.ViewIdAndFormIdList != null)
            {
            ParentId = SurveyRequestResultBO.ViewIdAndFormIdList[viewidname.Key];
                }
            SurveyIds.Add(viewidname.Key, ParentId);

            }           
            foreach(var ViewId in this.ViewIds )
                {
              
                string PId = SurveyIds[ViewId.Value].ToString();
                string SId = SurveyIds[ViewId.Key].ToString();
                this.SurveyInfoDao.UpdateParentId(SId, ViewId.Key, PId);
           
                 }

            SurveyRequestResultBO.ViewIdAndFormIdList = SurveyIds;
            SurveyRequestResultBO.URL = SurveyRequestResultBO.URL.Remove(SurveyRequestResultBO.URL.LastIndexOf('/'));

            return SurveyRequestResultBO;
            }
   
     
        #endregion

        private List<string> GetSurveyControls(SurveyInfoBO SurveyInfoBO)
        {
            List<string> List = new List<string>();

           

            XDocument xdoc = XDocument.Parse(SurveyInfoBO.XML);

            var _FieldsTypeIDs = from _FieldTypeID in
                                 xdoc.Descendants("Field")
                                 select _FieldTypeID;

            string fieldType = "";
            int counter =0;
            foreach (var _FieldTypeID in _FieldsTypeIDs)
            {
                fieldType = _FieldTypeID.Attribute("FieldTypeId").Value;

                if (fieldType != "2" && fieldType != "21" && fieldType != "3" && fieldType != "20" && fieldType != "4" && fieldType != "13")
                {
                    List.Add(_FieldTypeID.Attribute("Name").Value.ToString());
                    counter++;
                }
                if(counter == 5){
                    break;
                }
            }
            return List;
        }

        /// <summary>
        /// validate the Organization key passed with the UserID  from database 
        /// through EF
        /// </summary>
        /// <param name="OrganizationKey"></param>
        /// /// <param name="UserID"></param>
        /// <returns></returns>
        private bool ValidateOrganizationKeyByUser(Guid gOrganizationKey,int UserID)
        {
            bool result;
            string strOrgKeyEncrypted = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(gOrganizationKey.ToString());
            result = this.OrganizationDao.IsUserExistsInOrganization(strOrgKeyEncrypted, UserID);
            return result;
        }
      
    }
}
