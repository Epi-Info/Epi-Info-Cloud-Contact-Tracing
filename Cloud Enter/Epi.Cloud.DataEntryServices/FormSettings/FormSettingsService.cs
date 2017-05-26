using System;
using System.Linq;
using System.ServiceModel;
using Epi.Cloud.BLL;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Common.Exception;

namespace Epi.Cloud.DataEntryServices
{
    public class FormSettingsService : IFormSettingsService
	{
		private readonly IFormInfoDao _formInfoDao;
		private readonly IFormSettingDao _formSettingDao;
		private readonly IUserDao _userDao;

		public FormSettingsService(IFormSettingDao formSettingDao,
									IFormInfoDao formInfoDao,
									IUserDao userDao)
		{
			_formSettingDao = formSettingDao;
			_formInfoDao = formInfoDao;
			_userDao = userDao;
		}

		public FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest)
		{
			FormSettingResponse response = new FormSettingResponse();
			try
			{
				var formInfoImplementation = new FormInfo(_formInfoDao);
				var formInfoBO = formInfoImplementation.GetFormInfoByFormId(formSettingRequest.FormInfo.FormId, formSettingRequest.FormInfo.UserId);
				response.FormInfo = formInfoBO.ToFormInfoDTO();

				Epi.Web.BLL.FormSetting formSettingImplementation = new Epi.Web.BLL.FormSetting(_formSettingDao, _userDao);
                var formSettingBO = formSettingImplementation.GetFormSettings(formSettingRequest.FormInfo.FormId.ToString(), formSettingRequest.CurrentOrgId);

                response.FormSetting = formSettingBO.ToFormSettingDTO();

				return response;
			}
			catch (Exception ex)
			{
				throw new FaultException<CustomFaultException>(new CustomFaultException(ex));
			}
		}

		public FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest)
		{
			FormSettingResponse response = new FormSettingResponse();
			try
			{
				Epi.Web.BLL.FormSetting formSettingImplementation = new Epi.Web.BLL.FormSetting(_formSettingDao, _userDao);
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
