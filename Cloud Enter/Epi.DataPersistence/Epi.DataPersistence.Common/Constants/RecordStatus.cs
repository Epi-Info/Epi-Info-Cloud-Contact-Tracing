namespace Epi.DataPersistence.Constants
{
	public struct RecordStatus
	{
        public const int PhysicalDelete = int.MinValue;
        public const int Restore = -1;
        public const int Deleted = 0;
		public const int InProcess = 1;
		public const int Saved = 2;
		public const int Completed = 3;
		public const int Downloaded = 4;
	}
}
