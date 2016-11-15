using System.Collections.Generic;

namespace Epi.Cloud.Common.Constants
{
    public static class Constant
    {
        public enum OperationMode
        {
            NoChange = 0,
            UpdatePassword = 1,
            UpdateUserInfo = 2
        }

        public enum EmailCombinationEnum
        {
            ResetPassword = 1,
            PasswordChanged = 2,
            UpdateUserInfo = 3,
            InsertUser = 4,
            UpdateOrganization = 5,
            InsertOrganization = 6
        }

		/*sql commands*/
		public const string UPDATE = "Update";
		public const string UpdateMulti = "UpdateMulti";
		public const string CREATE = "Create";
		public const string CREATECHILD = "CreateChild";
		public const string SELECT = "Select";
		public const string CREATECHILDINEDITMODE = "CreateChildInEditMode";
		public const string DELETE = "Delete";
		public const string DELETERESPONSE = "DeleteResponse";

		public const string SURVEY_ID = "SurveyId";
		public const string QUESTION_ID = "QuestionId";
		public const string RESPONSE_ID = "ResponseId";
		public const string CURRENT_PAGE = "CurrentPage";

		/*view names*/
		public const string INDEX_PAGE = "Index";
		public const string SURVEY_INTRODUCTION = "SurveyIntroduction";
		public const string SURVAY_PAGE = "Survey";
		public const string EXCEPTION_PAGE = "Exception";


		/*controllers*/
		public const string HOME_CONTROLLER = "Home";
		public const string SURVEY_CONTROLLER = "Survey";

		/*action methods*/
		public const string INDEX = "Index";

		public static List<string> MetaDaTaColumnNames()
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
