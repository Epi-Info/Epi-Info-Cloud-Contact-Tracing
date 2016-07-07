using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.EF
{
    public partial class SurveyResponse
    {
        public FormResponseDetail ResponseDetail { get; set; }
        public Int32 UserId { get; set; }
    }
}
