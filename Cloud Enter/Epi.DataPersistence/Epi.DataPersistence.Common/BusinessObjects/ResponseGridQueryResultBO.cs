using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.DataPersistence.Common.BusinessObjects
{
    public class ResponseGridQueryResultBO
    {
        public List<SurveyResponseBO> SurveyResponseBOList { get; set; }
        public string QuerySetToken { get; set; }
        public int NumberOfResponsesReturnedByQuery { get; set; }
        public int NumberOfResponsesPerPage { get; set; }

        public int NumberOfResponsesOnSelectedPage { get; set; }
        public int PageNumber { get; set; }
        public int NumberOfPages { get; set; }

        /// <summary>
        /// PostProcessingWasRequired is true if the response from 
        /// Cosmos DB took longer than 5 minutes and therefore had to be
        /// processes in chunks necessitating that sorting and
        /// pagination had to be performed in web server.
        /// </summary>
        public bool PostProcessingWasRequired { get; set; }
    }
}
