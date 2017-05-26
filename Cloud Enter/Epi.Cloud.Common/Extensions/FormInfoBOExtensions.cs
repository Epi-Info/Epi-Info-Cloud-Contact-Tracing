
using Epi.Cloud.Common.BusinessObjects;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class FormInfoBOExtensions
    {
        public static FormSettings ToFormSettings(this FormInfoBO formInfoBO)
        {
            return new FormSettings
            {
                FormId = formInfoBO.FormId,
                FormName = formInfoBO.FormName,
                DataAccessRuleId = formInfoBO.DataAccesRuleId,
                IsDraftMode = formInfoBO.IsDraftMode,
                IsShareable = formInfoBO.IsShareable
            };
        }
    }
}
