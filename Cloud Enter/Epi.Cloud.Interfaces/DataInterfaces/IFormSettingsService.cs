using System.Collections.Generic;
using Epi.Cloud.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface IFormSettingsService
	{
		FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest);
        List<FormSettingResponse> GetFormSettingsList(List<FormSettingRequest> formSettingRequestList);
        FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest);
	}
}
