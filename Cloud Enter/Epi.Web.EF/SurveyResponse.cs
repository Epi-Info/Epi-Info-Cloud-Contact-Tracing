using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Web.EF
{
    public partial class SurveyResponse
    {
        public Dictionary<string, string> SurveyQAList { get; set; }
        public Int32 UserId { get; set; }
    }
}
