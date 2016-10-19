using System;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Metadata;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.BLL
{

    public class SurveyInfo
    {
        private Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao _surveyInfoDao;
        Dictionary<int, int> _viewIds = new Dictionary<int, int>();

        MetadataAccessor _metadataAcessor;

        public SurveyInfo(Epi.Web.Enter.Interfaces.DataInterfaces.ISurveyInfoDao surveyInfoDao)
        {
            this._surveyInfoDao = surveyInfoDao;
        }

        public MetadataAccessor MetadataAccessor
        {
            get { return _metadataAcessor = _metadataAcessor ?? new MetadataAccessor(); }
        }

        public SurveyInfoBO GetSurveyInfoById(string pId)
        {
            List<string> IdList = new List<string>();
            if (!string.IsNullOrEmpty(pId))
            {
                IdList.Add(pId);
            }
            List<SurveyInfoBO> result = this._surveyInfoDao.GetSurveyInfo(IdList);

            return result.Count > 0 ? result[0] : null;
        }

        /// <summary>
        /// Gets SurveyInfo based on criteria
        /// </summary>
        /// <param name="SurveyInfoId">Unique SurveyInfo identifier.</param>
        /// <returns>SurveyInfo.</returns>
        public List<SurveyInfoBO> GetSurveyInfoById(List<string> pIdList)
        {
            List<SurveyInfoBO> result = this._surveyInfoDao.GetSurveyInfo(pIdList);
            return result;
        }

        public PageInfoBO GetSurveySizeInfo(List<string> pIdList, int BandwidthUsageFactor, int pResponseMaxSize = -1)
        {
            List<SurveyInfoBO> SurveyInfoBOList = this._surveyInfoDao.GetSurveySizeInfo(pIdList, -1, -1, pResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(SurveyInfoBOList, BandwidthUsageFactor, pResponseMaxSize);
            return result;
        }

        public bool IsSurveyInfoValidByOrgKeyAndPublishKey(string SurveyId, string Okey, Guid publishKey)
        {
            string EncryptedKey = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(Okey);
            List<SurveyInfoBO> result = this._surveyInfoDao.GetSurveyInfoByOrgKeyAndPublishKey(SurveyId, EncryptedKey, publishKey);

            return result != null && result.Count > 0;
        }

        public bool IsSurveyInfoValidByOrgKey(string SurveyId, string pOrganizationKey)
        {
            string EncryptedKey = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(pOrganizationKey);
            List<SurveyInfoBO> result = this._surveyInfoDao.GetSurveyInfoByOrgKey(SurveyId, EncryptedKey);

            return result != null && result.Count > 0;
        }

        /// <summary>
        /// Gets SurveyInfo based on criteria
        /// </summary>
        /// <param name="SurveyInfoId">Unique SurveyInfo identifier.</param>
        /// <returns>SurveyInfo.</returns>
        public List<SurveyInfoBO> GetSurveyInfo(List<string> SurveyInfoIdList, DateTime pClosingDate, string Okey, int pSurveyType = -1, int pPageNumber = -1, int pPageSize = -1)
        {
            string EncryptedKey = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(Okey);
            List<SurveyInfoBO> result = this._surveyInfoDao.GetSurveyInfo(SurveyInfoIdList, pClosingDate, EncryptedKey, pSurveyType, pPageNumber, pPageSize);
            return result;
        }

        public PageInfoBO GetSurveySizeInfo(List<string> SurveyInfoIdList, DateTime pClosingDate, string Okey, int BandwidthUsageFactor, int pSurveyType = -1, int pPageNumber = -1, int pPageSize = -1, int pResponseMaxSize = -1)
        {
            string EncryptedKey = Epi.Web.Enter.Common.Security.Cryptography.Encrypt(Okey);

            List<SurveyInfoBO> SurveyInfoBOList = this._surveyInfoDao.GetSurveySizeInfo(SurveyInfoIdList, pClosingDate, EncryptedKey, pSurveyType, pPageNumber, pPageSize, pResponseMaxSize);

            PageInfoBO result = new PageInfoBO();

            result = Epi.Web.BLL.Common.GetSurveySize(SurveyInfoBOList, BandwidthUsageFactor, pResponseMaxSize);
            return result;
        }

        public SurveyInfoBO InsertSurveyInfo(SurveyInfoBO pValue)
        {
            SurveyInfoBO result = pValue;
            _surveyInfoDao.InsertSurveyInfo(pValue);
            return result;
        }

        public SurveyInfoBO UpdateSurveyInfo(SurveyInfoBO surveyInfoBO)
        {
            throw new NotImplementedException("UpdateSurveyInfo");

            //SurveyInfoBO result = surveyInfoBO;
            //if (ValidateSurveyFields(surveyInfoBO))
            //{
            //    if (IsRelatedForm(MetadataAccessor.GetFormDigest(surveyInfoBO.SurveyId)))
            //    {
            //        List<SurveyInfoBO> FormsHierarchyIds = this.GetFormsHierarchyIds(surveyInfoBO.SurveyId.ToString());

            //        // 1- breck down the xml to n views
            //        List<string> XmlList = new List<string>();
            //        XmlList = XmlChunking(surveyInfoBO.XML);

            //        // 2- call publish() with each of the views
            //        foreach (string Xml in XmlList)
            //        {
            //            XDocument xdoc = XDocument.Parse(Xml);
            //            SurveyInfoBO SurveyInfoBO = new SurveyInfoBO();
            //            XElement ViewElement = xdoc.XPathSelectElement("Template/Project/View");
            //            int ViewId;
            //            int.TryParse(ViewElement.Attribute("ViewId").Value.ToString(), out ViewId);

            //            GetRelateViewIds(ViewElement, ViewId);

            //            SurveyInfoBO = surveyInfoBO;
            //            SurveyInfoBO.XML = Xml;
            //            SurveyInfoBO.SurveyName = ViewElement.Attribute("Name").Value.ToString();
            //            SurveyInfoBO.ViewId = ViewId;

            //            SurveyInfoBO pBO = FormsHierarchyIds.Single(x => x.ViewId == ViewId);
            //            SurveyInfoBO.SurveyId = pBO.SurveyId;
            //            SurveyInfoBO.ParentId = pBO.ParentId;
            //            SurveyInfoBO.UserPublishKey = pBO.UserPublishKey;
            //            SurveyInfoBO.OwnerId = surveyInfoBO.OwnerId;

            //            this.SurveyInfoDao.UpdateSurveyInfo(SurveyInfoBO);
            //        }
            //    }
            //    else
            //    {
            //        this.SurveyInfoDao.UpdateSurveyInfo(surveyInfoBO);
            //    }
            //    result.StatusText = "Successfully updated survey information.";
            //}
            //else
            //{
            //    result.StatusText = "One or more survey required fields are missing values.";
            //}

            //return result;
        }

        public bool DeleteSurveyInfo(SurveyInfoBO surveyInfoBO)
        {
            bool result = false;

            this._surveyInfoDao.DeleteSurveyInfo(surveyInfoBO);
            result = true;

            return result;
        }
        private static bool ValidateSurveyFields(SurveyInfoBO surveyInfoBO)
        {

            bool isValid = true;


            if (surveyInfoBO.ClosingDate == null)
            {

                isValid = false;

            }


            else if (string.IsNullOrEmpty(surveyInfoBO.SurveyName))
            {

                isValid = false;
            }




            return isValid;
        }


        public List<SurveyInfoBO> GetChildInfoByParentId(Dictionary<string, int> parentIdList)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            foreach (KeyValuePair<string, int> item in parentIdList)
            {
                result = this._surveyInfoDao.GetChildInfoByParentId(item.Key, item.Value);
            }
            return result;
        }

        public SurveyInfoBO GetParentInfoByChildId(string ChildId)
        {
            SurveyInfoBO result = new SurveyInfoBO();

            result = this._surveyInfoDao.GetParentInfoByChildId(ChildId);

            return result;
        }

        public List<Web.Common.DTO.FormsHierarchyBO> GetFormsHierarchyIdsByRootId(string rootId)
        {
            List<SurveyInfoBO> SurveyInfoBOList = new List<SurveyInfoBO>();
            List<FormsHierarchyBO> result = new List<FormsHierarchyBO>();

            SurveyInfoBOList = this._surveyInfoDao.GetFormsHierarchyIdsByRootId(rootId);
            foreach (var item in SurveyInfoBOList)
            {
                FormsHierarchyBO FormsHierarchyBO = new FormsHierarchyBO();
                FormsHierarchyBO.RootFormId = rootId;
                FormsHierarchyBO.FormId = item.SurveyId;
                FormsHierarchyBO.ViewId = item.ViewId;
                FormsHierarchyBO.SurveyInfo = item;
                if (item.SurveyId == rootId)
                {
                    FormsHierarchyBO.IsRoot = true;
                }
                result.Add(FormsHierarchyBO);
            }

            return result;

        }

        private List<SurveyInfoBO> GetFormsHierarchyIds(string RootId)
        {
            List<SurveyInfoBO> FormsHierarchyIds = new List<SurveyInfoBO>();
            FormsHierarchyIds = this._surveyInfoDao.GetFormsHierarchyIdsByRootId(RootId);
            return FormsHierarchyIds;
        }

        private bool IsRelatedForm(FormDigest formDigest)
        {
            return formDigest.ParentFormId != null;
        }
    }
}
