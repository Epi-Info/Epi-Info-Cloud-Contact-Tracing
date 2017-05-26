using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.FormMetadata.Constants;
using Epi.Web.EF;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DataEntryServices.DAO
{
    public class FormSettingDao : IFormSettingDao
    {
        public FormSettingBO GetFormSettings(string FormId, int CurrentOrgId)
        {
            FormSettingBO formSettingBO = new FormSettingBO();
            Dictionary<int, string> columnNameList = new Dictionary<int, string>();
            Dictionary<int, string> availableUsers = new Dictionary<int, string>();
            Dictionary<int, string> selectedUsers = new Dictionary<int, string>();
            Dictionary<int, string> availableOrgs = new Dictionary<int, string>();
            Dictionary<int, string> selectedOrgs = new Dictionary<int, string>();
            Dictionary<int, string> dataAccessRuleIds = new Dictionary<int, string>();
            Dictionary<string, string> dataAccessRuleDescription = new Dictionary<string, string>();
            int selectedDataAccessRuleId;
            try
            {
                Guid id = new Guid(FormId);
                using (var Context = DataObjectFactory.CreateContext())
                {
                    var Query = from response in Context.ResponseDisplaySettings
                                where response.FormId == id
                                select response;

                    var DataRow = Query;

                    foreach (var Row in DataRow)
                    {

                        columnNameList.Add(Row.SortOrder, Row.ColumnName);

                    }

                    SurveyMetaData SelectedUserQuery = Context.SurveyMetaDatas.First(x => x.SurveyId == id);

                    var SelectedOrgId = CurrentOrgId;
                    var query = (from user in SelectedUserQuery.Users
                                 join userorg in Context.UserOrganizations
                             on user.UserID equals userorg.UserID
                                 where userorg.Active == true &&
                                    userorg.OrganizationID == SelectedOrgId
                                 select user).Distinct().OrderBy(user => user.UserName);

                    foreach (var user in query)
                    {
                        selectedUsers.Add(user.UserID, user.UserName);
                    }

                    var UserQuery = (from user in Context.Users
                                     join userorg in Context.UserOrganizations
                                     on user.UserID equals userorg.UserID
                                     where userorg.Active == true &&
                                        userorg.OrganizationID == SelectedOrgId
                                     select user).Distinct().OrderBy(user => user.UserName);



                    foreach (var user in UserQuery)
                    {
                        if (!selectedUsers.ContainsValue(user.UserName) && user.UserID != SelectedUserQuery.OwnerId)
                        {
                            availableUsers.Add(user.UserID, user.UserName);
                        }
                    }

                    //// Select Orgnization list 
                    var OrganizationQuery = Context.Organizations.Where(c => c.SurveyMetaDatas.Any(a => a.SurveyId == id)).ToList();


                    foreach (var org in OrganizationQuery)
                    {

                        selectedOrgs.Add(org.OrganizationId, org.Organization1);
                    }
                    ////  Available Orgnization list 

                    IQueryable<Organization> OrganizationList = Context.Organizations.ToList().AsQueryable();
                    foreach (var Org in OrganizationList)
                    {
                        if (!selectedOrgs.ContainsValue(Org.Organization1) && Org.IsEnabled == true)
                        {
                            availableOrgs.Add(Org.OrganizationId, Org.Organization1);
                        }
                    }
                    //// Select DataAccess Rule Ids  list 
                    var MetaData = Context.SurveyMetaDatas.Where(a => a.SurveyId == id).Single();


                    selectedDataAccessRuleId = int.Parse(MetaData.DataAccessRuleId.ToString());
                    ////  Available DataAccess Rule Ids  list 

                    IQueryable<DataAccessRule> RuleIDs = Context.DataAccessRules.ToList().AsQueryable();
                    foreach (var Rule in RuleIDs)
                    {

                        dataAccessRuleIds.Add(Rule.RuleId, Rule.RuleName);
                        dataAccessRuleDescription.Add(Rule.RuleName, Rule.RuleDescription);

                    }
                    formSettingBO.ColumnNameList = columnNameList;
                    formSettingBO.UserList = availableUsers;
                    formSettingBO.AssignedUserList = selectedUsers;
                    formSettingBO.AvailableOrgList = availableOrgs;
                    formSettingBO.SelectedOrgList = selectedOrgs;
                    formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
                    formSettingBO.SelectedDataAccessRule = selectedDataAccessRuleId;
                    formSettingBO.DataAccessRuleDescription = dataAccessRuleDescription;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return formSettingBO;
        }

        public void UpdateColumnNames(FormSettingBO FormSettingBO, string FormId)
        {
            Guid Id = new Guid(FormId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {

                    IQueryable<ResponseDisplaySetting> ColumnList = context.ResponseDisplaySettings.Where(x => x.FormId == Id);

                    //Delete old columns
                    foreach (var item in ColumnList)
                    {
                        context.ResponseDisplaySettings.DeleteObject(item);
                    }
                    context.SaveChanges();

                    //insert new columns

                    ResponseDisplaySetting ResponseDisplaySettingEntity = new ResponseDisplaySetting();
                    foreach (var item in FormSettingBO.ColumnNameList)
                    {

                        ResponseDisplaySettingEntity = Mapper.ToColumnName(item, Id);
                        context.AddToResponseDisplaySettings(ResponseDisplaySettingEntity);
                    }
                    context.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }


        }
        public void UpdateFormMode(FormInfoBO formInfoBO)
        {
            try
            {
                Guid Id = new Guid(formInfoBO.FormId);

                //Update Form Mode
                using (var context = DataObjectFactory.CreateContext())
                {
                    var query = from response in context.SurveyMetaDatas
                                where response.SurveyId == Id
                                select response;

                    var dataRow = query.Single();
                    dataRow.IsDraftMode = formInfoBO.IsDraftMode;
                    dataRow.IsShareable = formInfoBO.IsShareable;
                    dataRow.DataAccessRuleId = formInfoBO.DataAccesRuleId;

                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void UpdateSettingsList(FormSettingBO formSettingBO, string formId)
        {

            Guid id = new Guid(formId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    SurveyMetaData response = context.SurveyMetaDatas.First(x => x.SurveyId == id);

                    //Remove old Users
                    var userHashSet = new HashSet<string>(response.Users.Select(x => x.UserName));
                    var users = context.Users.Where(t => userHashSet.Contains(t.UserName)).ToList();

                    foreach (User user in users)
                    {
                        response.Users.Remove(user);
                    }
                    context.SaveChanges();

                    //insert new users
                    foreach (var item in formSettingBO.AssignedUserList)
                    {
                        User user = context.Users.FirstOrDefault(x => x.UserName == item.Value);
                        response.Users.Add(user);
                    }
                    context.SaveChanges();

                    //Remove old Orgs
                    var _Org = new HashSet<int>(response.Organizations.Select(x => x.OrganizationId));
                    var Orgs = context.Organizations.Where(t => _Org.Contains(t.OrganizationId)).ToList();

                    foreach (Organization org in Orgs)
                    {
                        response.Organizations.Remove(org);
                    }
                    context.SaveChanges();

                    //insert new Orgs
                    List<User> OrgAdmis = new List<User>();

                    foreach (var item in formSettingBO.SelectedOrgList)
                    {
                        int OrgId = int.Parse(item.Value);
                        Organization Org = context.Organizations.FirstOrDefault(x => x.OrganizationId == OrgId);
                        response.Organizations.Add(Org);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public FormSettingBO GetFormSettings()
        {
            FormSettingBO formSettingBO = new FormSettingBO();

            Dictionary<int, string> dataAccessRuleIds = new Dictionary<int, string>();
            Dictionary<string, string> dataAccessRuleDescription = new Dictionary<string, string>();
            int selectedDataAccessRuleId = -1; ;
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    ////  Available DataAccess Rule Ids  list 
                    IQueryable<DataAccessRule> RuleIDs = context.DataAccessRules.ToList().AsQueryable();
                    foreach (var Rule in RuleIDs)
                    {
                        dataAccessRuleIds.Add(Rule.RuleId, Rule.RuleName);
                        dataAccessRuleDescription.Add(Rule.RuleName, Rule.RuleDescription);
                    }

                    formSettingBO.SelectedDataAccessRule = selectedDataAccessRuleId;
                    formSettingBO.DataAccessRuleDescription = dataAccessRuleDescription;
                    formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return formSettingBO;
        }

        public List<string> GetAllColumnNames(string formId)
        {
            var metadataAccessor = new MetadataAccessor();
            var skipTypes = MetadataAccessor.NonQueriableFieldTypes;
            var columnNameList = metadataAccessor.GetFieldDigests(formId).Where(f => !skipTypes.Any(t => f.FieldType == t)).Select(f => f.TrueCaseFieldName).OrderBy(n => n).ToList();
            return columnNameList;
        }

        public Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList)
        {
            Dictionary<int, string> orgAdmins = new Dictionary<int, string>();

            int i = 0;
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    foreach (var org in selectedOrgList)
                    {
                        int orgId = int.Parse(org.Value);

                        var adminList = context.UserOrganizations.Where(x => x.OrganizationID == orgId && x.RoleId == Roles.Administrator && x.Active == true).ToList();

                        foreach (var item in adminList)
                        {
                            orgAdmins.Add(i, item.User.EmailAddress);
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return orgAdmins;
        }

        public List<UserBO> GetOrgAdminsByFormId(string formId)
        {

            List<UserBO> userBOList = new List<UserBO>();
            Dictionary<int, string> orgAdmins = new Dictionary<int, string>();
            Guid id = new Guid(formId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    SurveyMetaData response = context.SurveyMetaDatas.First(x => x.SurveyId == id);
                    var orgHashSet = new HashSet<int>(response.Organizations.Select(x => x.OrganizationId));
                    var orgs = context.Organizations.Where(t => orgHashSet.Contains(t.OrganizationId)).ToList();

                    foreach (var org in orgs)
                    {
                        var adminList = context.UserOrganizations.Where(x => x.OrganizationID == org.OrganizationId && x.RoleId == Roles.Administrator && x.Active == true);
                        foreach (var admin in adminList)
                        {
                            UserBO userBO = new UserBO();
                            userBO.EmailAddress = admin.User.EmailAddress;
                            userBO.UserId = admin.User.UserID;
                            userBOList.Add(userBO);
                       }

                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return userBOList;
        }

        public void SoftDeleteForm(string formId)
        {
            Guid id = new Guid(formId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    var Query = from response in context.SurveyMetaDatas
                                where response.SurveyId == id
                                select response;

                    var dataRow = Query.Single();
                    dataRow.ParentId = id;

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void DeleteDraftRecords(string formId)
        {
            throw new NotImplementedException("DeleteDraftRecords");
        }
    }
}
