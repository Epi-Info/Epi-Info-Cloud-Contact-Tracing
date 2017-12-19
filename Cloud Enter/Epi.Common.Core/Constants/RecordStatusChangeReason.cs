using Epi.Cloud.Common.Core;

namespace Epi.DataPersistence.Constants
{
	public enum RecordStatusChangeReason
	{
		Unknown = 0,

		Update,

        [NotifyConsistencyService(true)]
        DeleteResponse,

		DeleteInEditMode,

		OpenForEdit,

        [NotifyConsistencyService(true)]
        SubmitOrClose,

		ReadResponse,

		CreateMulti,

		NewChild,

		Logout,

        Restore
    }
}
