namespace Epi.Cloud.Common.Constants
{
	public enum RecordStatusChangeReason
	{
		Unknown = 0,
		Update,
		DeleteResponse,
		DeleteInEditMode,
		OpenForEdit,
		SubmitOrClose,
		ReadResponse,
		CreateMulti,
		NewChild,
		Logout,
        Restore 
	}
}
