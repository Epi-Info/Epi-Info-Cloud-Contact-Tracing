using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterface
{
    public interface IDataEntryService
    {
        SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest pRequest);

        SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest pRequest);
    }
}
