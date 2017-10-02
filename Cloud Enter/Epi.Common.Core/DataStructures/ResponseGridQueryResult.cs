using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.Core.DataStructures
{
    public class ResponseGridQueryResult
    {
        public List<FormResponseDetail> FormResponseDetailList { get; set; }
        public string QuerySetToken { get; set; }
        public int NumberOfResponsesReturnedByQuery { get; set; }
        public int NumberOfResponsesOnSelectedPage { get; set; }
        public int PageNumber { get; set; }
        public int NumberOfPages { get; set; }
    }
}
