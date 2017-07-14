using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Epi.Cloud.BLL;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Common.Exceptions;

namespace Epi.Cloud.DataEntryServices
{
    public class FormSettingsService : IFormSettingsService
	{
        private readonly IFormSettingFacade _formSettingFacade;
		private readonly IFormInfoDao _formInfoDao;
		private readonly IUserDao _userDao;

		public FormSettingsService(IFormSettingFacade formSettingFacade,
									IFormInfoDao formInfoDao,
									IUserDao userDao)
		{
			_formSettingFacade = formSettingFacade;
            _formInfoDao = formInfoDao;
			_userDao = userDao;
		}

        public List<FormSettingResponse> GetFormSettingsList(List<FormSettingRequest> formSettingRequestList)
        {
            List<FormSettingResponse> formSettingResponseList = new List<FormSettingResponse>();

            var formIds = formSettingRequestList.Select(f => f.FormInfo.FormId).ToList();
            var currentOrgId = formSettingRequestList[0].CurrentOrgId;

            Epi.Web.BLL.FormSetting formSettingImplementation = new Epi.Web.BLL.FormSetting(_formSettingFacade, _userDao);
            var formSettingBOList = formSettingImplementation.GetFormSettingsList(formIds, currentOrgId);

            for (int i = 0; i < formSettingBOList.Count(); ++i)
            {
                var formSettingRequest = formSettingRequestList[i];
                var formInfo = formSettingRequest.FormInfo;
                var formId = formInfo.FormId.ToString();
                var userId = formInfo.UserId;
                var formSettingResponse = CreateFormSettingResponse(formId, userId, formSettingBOList[i]);
                formSettingResponseList.Add(formSettingResponse);
            }
            return formSettingResponseList;
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest)
		{
			try
            {
                var formInfo = formSettingRequest.FormInfo;
                var formId = formInfo.FormId.ToString();
                var userId = formInfo.UserId;
                var currentOrgId = formSettingRequest.CurrentOrgId;

                Epi.Web.BLL.FormSetting formSettingImplementation = new Epi.Web.BLL.FormSetting(_formSettingFacade, _userDao);
                var formSettingBO = formSettingImplementation.GetFormSettings(formId, currentOrgId);
                var formSettingResponse = CreateFormSettingResponse(formId, userId, formSettingBO);
                return formSettingResponse;
            }
            catch (Exception ex)
			{
				throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
			}
		}

        private FormSettingResponse CreateFormSettingResponse(string formId, int userId, FormSettingBO formSettingBO)
        {
			FormSettingResponse response = new FormSettingResponse();

            var formInfoImplementation = new FormInfo(_formInfoDao);
            var formInfoBO = formInfoImplementation.GetFormInfoByFormId(formId, userId);

            response.FormInfo = formInfoBO.ToFormInfoDTO();
            response.FormSetting = formSettingBO.ToFormSettingDTO();

            return response;
        }

        public FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest)
		{
			FormSettingResponse response = new FormSettingResponse();
			try
			{
				Epi.Web.BLL.FormSetting formSettingImplementation = new Epi.Web.BLL.FormSetting(_formSettingFacade, _userDao);
				if (formSettingRequest.FormSetting.Count() > 0)
				{
					foreach (var item in formSettingRequest.FormSetting)
					{
						formSettingImplementation.UpdateFormSettings(formSettingRequest.FormInfo.IsDraftMode, item);

					}
					string Message = formSettingImplementation.SaveSettings(formSettingRequest.FormInfo.IsDraftMode, formSettingRequest.FormSetting[0]);
				}

				return response;
			}
			catch (Exception ex)
			{
				throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
			}
		}
	}
}
