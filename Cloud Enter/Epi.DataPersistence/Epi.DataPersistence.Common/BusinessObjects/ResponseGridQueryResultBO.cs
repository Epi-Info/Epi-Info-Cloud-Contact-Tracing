using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.DataPersistence.Common.BusinessObjects
{
    public class ResponseGridQueryResultBO
    {
        public List<SurveyResponseBO> SurveyResponseBOList { get; set; }
        public string QuerySetToken { get; set; }
        public int NumberOfResponsesReturnedByQuery { get; set; }
        public int NumberOfResponsesOnSelectedPage { get; set; }
        public int PageNumber { get; set; }
        public int NumberOfPages { get; set; }
    }
}
