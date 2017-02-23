using System.Collections.Generic;

namespace Epi.Cloud.Common.Constants
{
    public static class Constant
    {
		public static List<string> MetadataColumnNames()
		{
			List<string> columns = new List<string>();
			columns.Add("_UserEmail");
			columns.Add("_DateUpdated");
			columns.Add("_DateCreated");
			// columns.Add("IsDraftMode");
			columns.Add("_Mode");
			return columns;
		}
	}
}
