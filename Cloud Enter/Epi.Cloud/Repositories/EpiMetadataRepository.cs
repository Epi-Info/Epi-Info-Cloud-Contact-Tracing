using System;
using System.Collections.Generic;
using System.Web;
using Epi.Web.MVC.Repositories.Core;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Exception;
using System.ServiceModel;
using System.Web.Caching;
using System.Configuration;
using Epi.Cloud.MetadataServices;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.MVC.Repositories
{
    public class EpiMetadataRepository : RepositoryBase, ISurveyInfoRepository
    {
        private Epi.Cloud.CacheServices.IMetadataCache _metadataCache;
        private Epi.Web.WCF.SurveyService.IEWEDataService _iDataService;

        public EpiMetadataRepository(Epi.Cloud.CacheServices.IMetadataCache metadataCache,
                                     Epi.Web.WCF.SurveyService.IEWEDataService iDataService)

        {
            _metadataCache = metadataCache;
            _iDataService = iDataService;
        }

        /// <summary>
        /// Calling the proxy client to fetch a SurveyInfoResponse object
        /// </summary>
        /// <param name="surveyid"></param>
        /// <returns></returns>
        public SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest pRequest)
        {

            try
            {
                //SurveyInfoResponse result = Client.GetSurveyInfo(pRequest);
                //SurveyInfoResponse result = _iDataService.GetSurveyInfo(pRequest);
                SurveyInfoResponse result = null;
                string surveyId = pRequest.Criteria.SurveyIdList[0].ToString();
                var metadata = _metadataCache.GetProjectTemplateMetadata(surveyId);
                if (metadata != null)
                {
                }
                else
                {
                    ProjectMetadataProvider p = new ProjectMetadataProvider();
                    ProjectTemplateMetadata projectTemplateMetadata;
                    projectTemplateMetadata = p.GetProjectMetadata("0" /* not used */).Result;

                    result = (SurveyInfoResponse)_iDataService.GetSurveyInfo(pRequest);
                    _metadataCache.SetProjectTemplateMetadata(projectTemplateMetadata);
                }
                return result;



                // return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region stubcode
        public List<Epi.Web.Enter.Common.DTO.SurveyInfoDTO> GetList(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public Epi.Web.Enter.Common.DTO.SurveyInfoDTO Get(int id)
        {
            throw new NotImplementedException();
        }

        public int GetCount(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public void Insert(Epi.Web.Enter.Common.DTO.SurveyInfoDTO t)
        {
            throw new NotImplementedException();
        }

        public void Update(Epi.Web.Enter.Common.DTO.SurveyInfoDTO t)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
        #endregion


        List<SurveyInfoResponse> IRepository<SurveyInfoResponse>.GetList(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        SurveyInfoResponse IRepository<SurveyInfoResponse>.Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(SurveyInfoResponse t)
        {
            throw new NotImplementedException();
        }

        public void Update(SurveyInfoResponse t)
        {
            throw new NotImplementedException();
        }


        public FormsInfoResponse GetFormsInfoList(FormsInfoRequest pRequestId)
        {

            FormsInfoResponse result = (FormsInfoResponse)_iDataService.GetFormsInfo(pRequestId);
            return result;
        }


        public SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest SARequest)
        {
            return _iDataService.DeleteResponse(SARequest);
        }


        public SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest SurveyInfoRequest)
        {


            return _iDataService.GetFormChildInfo(SurveyInfoRequest);

        }
        public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest)
        {

            return _iDataService.GetFormsHierarchy(FormsHierarchyRequest);



        }
        public SurveyAnswerResponse GetResponseAncestor(SurveyAnswerRequest SARequest)
        {

            return _iDataService.GetAncestorResponseIdsByChildId(SARequest);

        }
    }
}
