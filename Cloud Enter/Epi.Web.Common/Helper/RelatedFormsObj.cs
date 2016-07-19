using System.Xml.Linq;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.Enter.Common.Helper
{
    public class RelatedFormsObj
    {
        public FieldDigest[] FieldDigests { get; set; }
        public FormResponseDetail ResponseDetail { get; set; }

        public XDocument MetaData { get; set; }
        public XDocument Response { get; set; }
    }
}