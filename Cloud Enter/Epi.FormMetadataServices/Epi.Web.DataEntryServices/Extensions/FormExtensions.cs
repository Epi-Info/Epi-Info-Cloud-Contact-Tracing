using System;
using System.Linq;
using Epi.Cloud.DataEntryServices.Model;
using MvcDynamicForms;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class FormExtensions
    {
        public static PageResponseProperties ToPageResponseProperties(this Form form, string responseId)
        {
            PageResponseProperties pageResponseProperties = new PageResponseProperties
            {
                GlobalRecordID = responseId,
                PageId = Convert.ToInt32(form.PageId)
            };
            pageResponseProperties.ResponseQA = form.InputFields.Where(f => !f.IsPlaceHolder).ToDictionary(k => k.Key, v => v.Response);
            return pageResponseProperties;
        }
    }
}
