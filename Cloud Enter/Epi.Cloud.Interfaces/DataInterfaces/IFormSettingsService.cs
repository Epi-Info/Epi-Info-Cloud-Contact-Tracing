using System;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface IFormSettingsService
	{
		FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest);
		FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest);
	}
}
