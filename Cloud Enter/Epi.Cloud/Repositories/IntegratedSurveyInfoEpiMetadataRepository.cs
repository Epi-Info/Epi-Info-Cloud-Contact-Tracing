using System;
using System.Collections.Generic;
using Epi.Web.MVC.Repositories.Core;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Exception;
using System.ServiceModel;
using Epi.Cloud.MetadataServices;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.CacheServices;

namespace Epi.Cloud.MVC.Repositories
{
    public class IntegratedSurveyInfoEpiMetadataRepository : RepositoryBase, ISurveyInfoRepository
    {
        private readonly IEpiCloudCache _epiCloudCache;
        private readonly Epi.Cloud.MetadataServices.IProjectMetadataProvider _projectMetadataProvider;
        private readonly Epi.Web.WCF.SurveyService.IEWEDataService _iDataService;

        public IntegratedSurveyInfoEpiMetadataRepository(IEpiCloudCache epiCloudCache,
                                     IProjectMetadataProvider projectMetadataProvider,
                                     Epi.Web.WCF.SurveyService.IEWEDataService iDataService)
        {
            _epiCloudCache = epiCloudCache;
            _projectMetadataProvider = projectMetadataProvider;
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
                SurveyInfoResponse result = null;
                result = (SurveyInfoResponse)_iDataService.GetSurveyInfo(pRequest);
                foreach (var x in result.SurveyInfoList)
                {
                    var projectId = _epiCloudCache.GetProjectIdFromSurveyId(x.SurveyId);
                    x.ProjectTemplateMetadata = _projectMetadataProvider.GetProjectMetadataAsync(projectId).Result;
                }

                return result;
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
