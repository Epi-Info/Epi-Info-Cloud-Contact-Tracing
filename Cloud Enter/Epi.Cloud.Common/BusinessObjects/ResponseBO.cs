using Epi.DataPersistence.DataStructures;

namespace Epi.Coud.Common.BusinessObjects
{
    public class ResponseBO
    {
        public string ResponseId { get; set; }

        public int User { get; set; }

        public bool IsNewRecord { get; set; }

        public FormResponseDetail ResponseDetail { get; set; }
    }
}
