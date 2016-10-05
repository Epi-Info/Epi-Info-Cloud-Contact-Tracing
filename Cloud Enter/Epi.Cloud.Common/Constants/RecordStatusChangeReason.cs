namespace Epi.Cloud.Common.Constants
{
	public enum RecordStatusChangeReason
	{
		Unknown = 0,
		DeleteResponse,
		DeleteInEditMode,
		OpenForEdit,
		SubmitOrClose,
		ReadResponse,
		NewChild,
		Logout 
	}
}
