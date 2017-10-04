using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;

namespace Epi.DataPersistence.Common
{
    public class ResponseGridQueryResult
    {
        public List<FormResponseDetail> FormResponseDetailList { get; set; }
        public string QuerySetToken { get; set; }
        public int NumberOfResponsesReturnedByQuery { get; set; }
        public int NumberOfResponsesPerPage { get; set; }
        public int NumberOfResponsesOnSelectedPage { get; set; }
        public int PageNumber { get; set; }
        public int NumberOfPages { get; set; }
    }
}
