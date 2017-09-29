using System.Collections.Generic;

namespace Epi.Cloud.Common.Constants
{
    public static class Constant
    {
        public struct MetaColumn
        {
            public const string UserEmail = "_UserEmail";
            public const string DateUpdated = "_DateUpdated";
            public const string DateCreated = "_DateCreated";
            public const string IsDraftMode = "IsDraftMode";
            public const string Mode = "_Mode";
        }

        public static readonly int MaxGridColumns = 5;

		public static List<string> MetadataColumnNames()
		{
			List<string> columns = new List<string>();
			columns.Add(MetaColumn.UserEmail);
			columns.Add(MetaColumn.DateUpdated);
			columns.Add(MetaColumn.DateCreated);
            // columns.Add(MetaColumn.IsDraftMode);
            columns.Add(MetaColumn.Mode);
			return columns;
		}
	}
}
