using System;
using System.Linq;
using Epi.Cloud.DataEntryServices.Model;
using MvcDynamicForms;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class FormExtensions
    {
        public static Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties ToPageResponseProperties(this Form form, string responseId)
        {
			Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties pageResponseProperties = new Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties
            {
                GlobalRecordID = responseId,
                PageId = Convert.ToInt32(form.PageId)
            };
            pageResponseProperties.ResponseQA = form.InputFields.Where(f => !f.IsPlaceHolder).ToDictionary(k => k.Key, v => v.Response);
            return pageResponseProperties;
        }
    }
}
