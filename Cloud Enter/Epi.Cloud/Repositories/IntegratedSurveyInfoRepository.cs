using System;
using System.Collections.Generic;
using System.ServiceModel;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Exception;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Repositories.Core;

namespace Epi.Cloud.MVC.Repositories
{
	public class IntegratedSurveyInfoRepository : ISurveyInfoRepository
    {
        private readonly Epi.Web.WCF.SurveyService.IEWEDataService _iDataService;

        public IntegratedSurveyInfoRepository(Epi.Web.WCF.SurveyService.IEWEDataService iDataService)
        {
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
                SurveyInfoResponse result = (SurveyInfoResponse)_iDataService.GetSurveyInfo(pRequest);
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
        public List<SurveyInfoDTO> GetList(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public SurveyInfoDTO Get(int id)
        {
            throw new NotImplementedException();
        }

        public int GetCount(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public void Insert(SurveyInfoDTO t)
        {
            throw new NotImplementedException();
        }

        public void Update(SurveyInfoDTO t)
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

        public Epi.Web.Enter.Common.Message.FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest)
        {
            return _iDataService.GetFormsHierarchy(FormsHierarchyRequest);
        }

        public SurveyAnswerResponse GetResponseAncestor(SurveyAnswerRequest SARequest)
        {

            return _iDataService.GetAncestorResponseIdsByChildId(SARequest);

        }
    }
}
