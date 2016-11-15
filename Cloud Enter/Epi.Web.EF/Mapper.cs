using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Web.EF
{
    /// <summary>
    /// Maps Entity Framework entities to business objects and vice versa.
    /// </summary>
    public class Mapper
    {
        /// <summary>
        /// Maps SurveyMetaData entity to SurveyInfoBO business object.
        /// </summary>
        /// <param name="entity">A SurveyMetaData entity to be transformed.</param>
        /// <returns>A SurveyInfoBO business object.</returns>
        public static SurveyInfoBO Map(SurveyMetaData entity)
        {
            SurveyInfoBO result = new SurveyInfoBO();

            result.SurveyId = entity.SurveyId.ToString();
            result.SurveyName = entity.SurveyName;
            result.SurveyNumber = entity.SurveyNumber;
            result.IntroductionText = entity.IntroductionText;
            result.ExitText = entity.ExitText;
            result.OrganizationName = entity.OrganizationName;
            result.DepartmentName = entity.DepartmentName;
            result.ClosingDate = entity.ClosingDate;
            result.DateCreated = entity.DateCreated;
            result.IsDraftMode = entity.IsDraftMode;
            result.StartDate = entity.StartDate;
            result.IsSqlProject = (bool)entity.IsSQLProject;
            result.OwnerId = entity.OwnerId;
            if (entity.UserPublishKey != null)
            {
                // result.UserPublishKey = (Guid)entity.UserPublishKey.Value;
                result.UserPublishKey = entity.UserPublishKey;
            }
            result.SurveyType = entity.SurveyTypeId;
            result.ParentId = entity.ParentId.ToString();
            if (entity.ViewId != null)
            {
                result.ViewId = (int)entity.ViewId;
            }
            //result. = (bool)entity.ShowAllRecords;
            result.IsShareable = (bool)entity.IsShareable;
            return result;
        }

        public static FormInfoBO ToFormInfoBO(SurveyMetaData entity)
        {
            return new FormInfoBO
            {
                FormId = entity.SurveyId.ToString(),
                FormNumber = entity.SurveyNumber,
                FormName = entity.SurveyName,
                OrganizationName = entity.OrganizationName,
                OrganizationId = entity.OrganizationId,
                IsDraftMode = entity.IsDraftMode,
                UserId = entity.OwnerId,
                ParentId = (entity.ParentId != null) ? entity.ParentId.ToString() : ""

            };
        }

        /// <summary>
        /// Maps the Entity User to BO
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="user"></param>
        public static UserBO MapToUserBO(User user, int Role = 0)
        {
            UserBO Result = new UserBO();
            Result.UserId = user.UserID;
            Result.UserName = user.UserName;
            Result.EmailAddress = user.EmailAddress;
            Result.FirstName = user.FirstName;
            Result.LastName = user.LastName;
            Result.PhoneNumber = user.PhoneNumber;
            Result.ResetPassword = user.ResetPassword;
            Result.Role = Role;
            if (!string.IsNullOrEmpty(user.UGuid.ToString()))
            {
                Result.UGuid = Guid.Parse(user.UGuid.ToString());
            }
            return Result;
        }

        /// <summary>
        /// Maps SurveyMetaData entity to FormInfoBO business object.
        /// </summary>
        /// <param name="entity">A SurveyMetaData entity to be transformed.</param>
        /// <returns>A FormInfoBO business object.</returns>
        public static FormInfoBO MapToFormInfoBO(SurveyMetaData entity, User UserEntity, bool getMetadata = false)
        {
            FormInfoBO result = new FormInfoBO();
            result.IsSQLProject = (entity.IsSQLProject == null) ? false : (bool)entity.IsSQLProject;
            result.FormId = entity.SurveyId.ToString();
            result.FormName = entity.SurveyName;
            result.FormNumber = entity.SurveyNumber;
            result.OrganizationName = entity.OrganizationName;
            result.OrganizationId = entity.OrganizationId;
            result.IsDraftMode = entity.IsDraftMode;
            result.UserId = entity.OwnerId;

            if (entity.IsShareable != null)
            {
                result.IsShareable = (bool)entity.IsShareable;
            }
            if (entity.DataAccessRuleId != null)
            {
                // result. =  entity.DataAccessRuleId;
            }
            result.OwnerFName = UserEntity.FirstName;
            result.OwnerLName = UserEntity.LastName;

            if (getMetadata)
            {
                result.Xml = entity.TemplateXML;
            }
            result.ParentId = entity.ParentId.ToString();
            return result;
        }

        public static List<SurveyInfoBO> Map(List<SurveyMetaData> entities)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            foreach (SurveyMetaData surveyMetaData in entities)
            {
                result.Add(Map(surveyMetaData));
            }

            return result;
        }

        public static OrganizationBO Map(Organization entity)
        {
            return new OrganizationBO
            {
                Organization = entity.Organization1,
                IsEnabled = entity.IsEnabled,
                OrganizationKey = entity.OrganizationKey,
                OrganizationId = entity.OrganizationId


            };
        }
        public static OrganizationBO Map(string Organization)
        {
            return new OrganizationBO
            {
                Organization = Organization



            };
        }
        public static Organization ToEF(OrganizationBO pBo)
        {
            return new Organization
            {
                Organization1 = pBo.Organization,
                IsEnabled = pBo.IsEnabled,
                OrganizationKey = pBo.OrganizationKey,
            };
        }

        public static ResponseDisplaySetting ToColumnName(KeyValuePair<int, string> ColumnList, Guid FormId)
        {
            return new ResponseDisplaySetting
            {
                SortOrder = ColumnList.Key + 1,
                ColumnName = ColumnList.Value,
                FormId = FormId
            };
        }

        public static List<SurveyInfoBO> Map(IQueryable<SurveyMetaData> iQueryable)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            foreach (SurveyMetaData Obj in iQueryable)
            {
                result.Add(Map(Obj));

            }
            return result;
        }

        public static ResponseDisplaySetting Map(string FormId, int i, string Column)
        {
            ResponseDisplaySetting ResponseDisplaySetting = new ResponseDisplaySetting();
            ResponseDisplaySetting.FormId = new Guid(FormId);
            ResponseDisplaySetting.ColumnName = Column;
            ResponseDisplaySetting.SortOrder = i;
            return ResponseDisplaySetting;

        }

        public static User ToUserEntity(UserBO User)
        {
            User UserEntity = new User();
            UserEntity.EmailAddress = User.EmailAddress;
            UserEntity.UserName = User.EmailAddress;
            UserEntity.LastName = User.LastName;
            UserEntity.FirstName = User.FirstName;
            UserEntity.PhoneNumber = User.PhoneNumber;
            UserEntity.ResetPassword = User.ResetPassword;
            UserEntity.PasswordHash = User.PasswordHash;
            UserEntity.UGuid = User.UGuid;
            return UserEntity;
        }

        public static UserOrganization ToUserOrganizationEntity(bool IsActive, UserBO User, OrganizationBO Organization)
        {
            UserOrganization UserOrganization = new UserOrganization();
            UserOrganization.Active = IsActive;

            UserOrganization.RoleId = User.Role;

            User UserInfo = new EF.User();
            Organization OrganizationInfo = new EF.Organization();
            UserInfo.EmailAddress = User.EmailAddress;
            UserInfo.UserName = User.EmailAddress;
            UserInfo.LastName = User.LastName;
            UserInfo.FirstName = User.FirstName;
            UserInfo.PhoneNumber = User.PhoneNumber;
            UserInfo.ResetPassword = User.ResetPassword; //false;
            UserInfo.PasswordHash = User.PasswordHash; //"PassWord1";
            UserInfo.UGuid = User.UGuid;
            UserOrganization.User = UserInfo;


            OrganizationInfo.Organization1 = Organization.Organization;
            OrganizationInfo.IsEnabled = Organization.IsEnabled;
            OrganizationInfo.OrganizationKey = Organization.OrganizationKey;

            UserOrganization.Organization = OrganizationInfo;

            return UserOrganization;
        }

        public static UserOrganization ToUserOrganizationEntity(bool IsActive, int UserId, int RoleId, OrganizationBO Organization)
        {
            UserOrganization UserOrganization = new UserOrganization();
            UserOrganization.Active = IsActive;

            UserOrganization.RoleId = RoleId;
            UserOrganization.UserID = UserId;

            Organization OrganizationInfo = new EF.Organization();



            OrganizationInfo.Organization1 = Organization.Organization;
            OrganizationInfo.IsEnabled = Organization.IsEnabled;
            OrganizationInfo.OrganizationKey = Organization.OrganizationKey;
            OrganizationInfo.OrganizationId = Organization.OrganizationId;
            UserOrganization.Organization = OrganizationInfo;

            return UserOrganization;
        }
        public static UserOrganization ToUserOrganizationEntity(UserBO User, OrganizationBO Organization)
        {
            UserOrganization UserOrganization = new UserOrganization();
            UserOrganization.Active = User.IsActive;
            UserOrganization.RoleId = User.Role;
            UserOrganization.OrganizationID = Organization.OrganizationId;


            return UserOrganization;
        }

        public static EIDatasource Map(DbConnectionStringBO ConnectionString)
        {
            EIDatasource Datasource = new EIDatasource();
            Datasource.DatabaseType = ConnectionString.DatabaseType;
            Datasource.DatabaseUserID = ConnectionString.DatabaseUserID;
            Datasource.DatasourceID = ConnectionString.DatasourceID;
            Datasource.DatasourceServerName = ConnectionString.DatasourceServerName;
            Datasource.InitialCatalog = ConnectionString.InitialCatalog;
            Datasource.Password = ConnectionString.Password;
            Datasource.SurveyId = ConnectionString.SurveyId;
            Datasource.PersistSecurityInfo = ConnectionString.PersistSecurityInfo;
            return Datasource;
        }
    }
}
