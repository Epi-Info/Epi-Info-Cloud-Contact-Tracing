namespace Epi.Cloud.Resources.Constants
{
    public struct EmailResourceKeys
    {
        public const string InsertUser_Subject = "InsertUser_Subject";
        public const string PasswordChanged_Body = "PasswordChanged_Body";
        public const string PasswordChanged_Subject = "PasswordChanged_Subject";
        public const string ResetPassword_Body = "ResetPassword_Body";
        public const string ResetPassword_Subject = "ResetPassword_Subject";
        public const string UpdateUserInfo_Body = "UpdateUserInfo_Body";
        public const string UpdateUserInfo_Subject = "UpdateUserInfo_Subject";

        public const string FormAssignedToYou_Body = "FormAssignedToYou_Body";
        public const string FormAssignedToYou_Subject = "FormAssignedToYou_Subject";
        public const string FormSharedWithYourOrganization_Body = "FormSharedWithYourOrganization_Body";
        public const string FormSharedWithYourOrganization_Subject = "FormSharedWithYourOrganization_Subject";

        public const string RecordLocked_Body = "RecordLocked_Body";
        public const string RecordLocked_Subject = "RecordLocked_Subject";
    }

    public struct UserAdminResourceKeys
    {
        public const string UserInformationUpdated = "UserInformationUpdated";
        public const string UserAlreadyExists = "UserAlreadyExists";
        public const string UserAdded = "UserAdded";
    }

    public struct DocumentDBSPKeys
    {
        public const string GetAllRecordsBySurveyID = "GetAllRecordsBySurveyID";
        public const string OrderBy = "OrderBy";
    }

    public struct DocumentDBUDFKeys
    {
        public const string udfWildCardCompare = "udfWildCardCompare";
        public const string udfSharingRules = "udfSharingRules";
    }

    public struct DataAccessRuleKeys
    {
        public const string Rule1 = "Rule1";
        public const string Rule2 = "Rule2";
        public const string Rule3 = "Rule3";
    }
}
