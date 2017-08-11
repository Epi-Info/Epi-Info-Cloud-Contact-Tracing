using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Resources;
using Epi.FormMetadata.DataStructures;

namespace Epi.Web.EF
{
    public class EntityFormSettingDao : MetadataAccessor, IFormSettingDao_EF
    {
        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId, bool userAndOrgInfoOnly)
        {
            List<FormSettingBO> formSettingList = new List<FormSettingBO>();
            foreach (string formId in formIds)
            {
                formSettingList.Add(GetFormSettings(formId, currentOrgId, userAndOrgInfoOnly));
            }
            return formSettingList;
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId, bool userAndOrgInfoOnly)
        {
            FormSettingBO formSettingBO = new FormSettingBO { FormId = formId };
            Dictionary<int, string> availableUsers = new Dictionary<int, string>();
            Dictionary<int, string> selectedUsers = new Dictionary<int, string>();
            Dictionary<int, string> availableOrgs = new Dictionary<int, string>();
            Dictionary<int, string> selectedOrgs = new Dictionary<int, string>();
            try
            {
                Guid id = new Guid(formId);
                using (var Context = DataObjectFactory.CreateContext())
                {
                    SurveyMetaData selectedUserQuery = Context.SurveyMetaDatas.First(x => x.SurveyId == id);

                    var selectedOrgId = currentOrgId;
                    var query = (from user in selectedUserQuery.Users
                                 join userorg in Context.UserOrganizations
                                 on user.UserID equals userorg.UserID
                                 where userorg.Active == true && userorg.OrganizationID == selectedOrgId
                                 select user).Distinct().OrderBy(user => user.UserName);

                    foreach (var user in query)
                    {
                        selectedUsers.Add(user.UserID, user.UserName);
                    }

                    formSettingBO.AssignedUserList = selectedUsers;

                    // --------------------------------------------------------------------------------------------- \\

                    var userQuery = (from user in Context.Users
                                     join userorg in Context.UserOrganizations
                                     on user.UserID equals userorg.UserID
                                     where userorg.Active == true && userorg.OrganizationID == selectedOrgId
                                     //orderby user.UserName
                                     select user).Distinct().OrderBy(user => user.UserName);

                    foreach (var user in userQuery)
                    {
                        if (!selectedUsers.ContainsValue(user.UserName) && user.UserID != selectedUserQuery.OwnerId)
                        {
                            availableUsers.Add(user.UserID, user.UserName);
                        }
                    }

                    formSettingBO.UserList = availableUsers;

                    // --------------------------------------------------------------------------------------------- \\

                    //// Select Orgnization list 
                    var OrganizationQuery = Context.Organizations.Where(c => c.SurveyMetaDatas.Any(a => a.SurveyId == id)).ToList();

                    foreach (var org in OrganizationQuery)
                    {
                        selectedOrgs.Add(org.OrganizationId, org.Organization1);
                    }
                    formSettingBO.SelectedOrgList = selectedOrgs;

                    // --------------------------------------------------------------------------------------------- \\

                    ////  Available Organization list 
                    IQueryable<Organization> OrganizationList = Context.Organizations.ToList().AsQueryable();
                    foreach (var Org in OrganizationList)
                    {
                        if (!selectedOrgs.ContainsValue(Org.Organization1) && Org.IsEnabled == true)
                        {
                            availableOrgs.Add(Org.OrganizationId, Org.Organization1);
                        }
                    }
                    formSettingBO.AvailableOrgList = availableOrgs;

                    // --------------------------------------------------------------------------------------------- \\

                    if (!userAndOrgInfoOnly)
                    {
                        //// Response Grid Column Name List
                        Dictionary<int, string> ColumnNameList = new Dictionary<int, string>();

                        var Query = from response in Context.ResponseDisplaySettings
                                    where response.FormId == id
                                    select response;

                        var DataRow = Query;

                        // If there are no grid display columns currently defined
                        // then initialize the list to the first 5 fields in the form.
                        if (DataRow.Count() == 0)
                        {
                            var sortOrder = 1;
                            var responseGridColumnNames = GetFieldDigests(formId)
                                .Where(f => !FieldDigest.NonDataFieldTypes.Any(t => t == f.FieldType))
                                .Take(5)
                                .Select(f => ResponseDisplaySetting.CreateResponseDisplaySetting(id, f.TrueCaseFieldName, sortOrder++));
                            foreach (var responseDisplaySetting in responseGridColumnNames)
                            {
                                Context.AddToResponseDisplaySettings(responseDisplaySetting);
                                ColumnNameList.Add(responseDisplaySetting.SortOrder, responseDisplaySetting.ColumnName);
                            }
                            Context.SaveChanges();
                        }
                        else
                        {
                            foreach (var Row in DataRow)
                            {
                                ColumnNameList.Add(Row.SortOrder, Row.ColumnName);
                            }
                        }
                        formSettingBO.ResponseGridColumnNameList = ColumnNameList;

                        // --------------------------------------------------------------------------------------------- \\

                        //// Selected Data Access Rule Id
                        var MetaData = from r in Context.SurveyMetaDatas
                                       where r.SurveyId == id
                                       select new
                                       {
                                           Id = r.DataAccessRuleId
                                       };

                        var selectedDataAccessRuleId = int.Parse(MetaData.First().Id.ToString());
                        formSettingBO.SelectedDataAccessRule = selectedDataAccessRuleId;

                        // --------------------------------------------------------------------------------------------- \\

                        //// Data Access Rules 
                        Dictionary<int, string> DataAccessRuleIds = new Dictionary<int, string>();
                        Dictionary<string, string> DataAccessRuleDescription = new Dictionary<string, string>();

                        DataAccessessRulesHelper.GetDataAccessRules(out DataAccessRuleIds, out DataAccessRuleDescription);
                        formSettingBO.DataAccessRuleIds = DataAccessRuleIds;
                        formSettingBO.DataAccessRuleDescription = DataAccessRuleDescription;

                        //IQueryable<DataAccessRule> RuleIDs = Context.DataAccessRules.ToList().AsQueryable();
                        //foreach (var Rule in RuleIDs)
                        //{
                        //    DataAccessRuleIds.Add(Rule.RuleId, Rule.RuleName);
                        //    DataAccessRuleDescription.Add(Rule.RuleName, Rule.RuleDescription);
                        //}

                        // --------------------------------------------------------------------------------------------- \\

                        //// All Column Names From MetadataAccessor
                        int k = 1;
                        formSettingBO.FormControlNameList = GetAllColumnNames(formId).ToDictionary(key => k++, value => value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return formSettingBO;
        }

        public void UpdateFormMode(FormInfoBO formInfoBO, FormSettingBO formSettingBO = null)
        {
            try
            {
                Guid id = new Guid(formInfoBO.FormId);

                //Update Form Mode
                using (var context = DataObjectFactory.CreateContext())
                {
                    var query = from response in context.SurveyMetaDatas
                                where response.SurveyId == id
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

        public void UpdateSettingsList(FormSettingBO formSettingBO, string formId, int CurrentOrg = -1)
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
                        if (user.UserOrganizations.Where(x => x.OrganizationID == CurrentOrg).Count() > 0)
                        {
                            response.Users.Remove(user);
                        }
                    }
                    context.SaveChanges();

                    //insert new users
                    foreach (var item in formSettingBO.AssignedUserList)
                    {
                        User User = context.Users.FirstOrDefault(x => x.UserName == item.Value);
                        response.Users.Add(User);
                    }
                    context.SaveChanges();

                    //Remove old Orgs
                    var orgHashSet = new HashSet<int>(response.Organizations.Select(x => x.OrganizationId));
                    var orgs = context.Organizations.Where(t => orgHashSet.Contains(t.OrganizationId)).ToList();

                    foreach (Organization org in orgs)
                    {
                        response.Organizations.Remove(org);
                    }
                    context.SaveChanges();

                    //insert new Orgs
                    List<User> orgAdmins = new List<User>();

                    foreach (var item in formSettingBO.SelectedOrgList)
                    {
                        int orgId = int.Parse(item.Value);
                        Organization org = context.Organizations.FirstOrDefault(x => x.OrganizationId == orgId);
                        response.Organizations.Add(org);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public List<string> GetAllColumnNames(string FormId)
        {
            Guid Id = new Guid(FormId);
            try
            {
                List<string> columns = GetFieldDigests(FormId)
                               .Where(f => !FieldDigest.NonDataFieldTypes.Any(t => t == f.FieldType))
                               .Select(f =>  f.TrueCaseFieldName).ToList();
                return columns;
                             
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public Dictionary<int, string> GetOrgAdmins(Dictionary<int, string> selectedOrgList)
        {
            Dictionary<int, string> orgAdmins = new Dictionary<int, string>();

            try
            {
                int i = 0;
                foreach (var org in selectedOrgList)
                {
                    using (var context = DataObjectFactory.CreateContext())
                    {
                        int orgId = int.Parse(org.Value);

                        var adminList = context.UserOrganizations.Where(x => x.OrganizationID == orgId && x.RoleId == Roles.Administrator && x.Active == true).ToList();
                       
                        foreach (var item in adminList)
                        {
                            orgAdmins.Add(i++, item.User.EmailAddress);
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
            // TODO: Refactor to remove dependency on SurveyMetadatas

            List<UserBO> boList = new List<UserBO>();
            Dictionary<int, string> orgAdmins = new Dictionary<int, string>();
            Guid id = new Guid(formId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    SurveyMetaData Response = context.SurveyMetaDatas.First(x => x.SurveyId == id);
                    var orgHashSet = new HashSet<int>(Response.Organizations.Select(x => x.OrganizationId));
                    var orgs = context.Organizations.Where(t => orgHashSet.Contains(t.OrganizationId)).ToList();

                    foreach (var org in orgs)
                    {
                        var adminList = context.UserOrganizations.Where(x => x.OrganizationID == org.OrganizationId && x.RoleId == Roles.Administrator && x.Active == true);
                        foreach (var admin in adminList)
                        {
                            UserBO userBO = new UserBO();
                            userBO.EmailAddress = admin.User.EmailAddress;
                            userBO.UserId = admin.User.UserID;
                            boList.Add(userBO);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return boList;
        }

        public void SoftDeleteForm(string formId)
        {
            // TODO: Refactor to remove dependency on SurveyMetadatas

            Guid Id = new Guid(formId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    var query = from response in context.SurveyMetaDatas
                                where response.SurveyId == Id
                                select response;

                    var dataRow = query.Single();
                    dataRow.ParentId = Id;


                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public void UpdateResponseGridColumnNames(FormSettingBO FormSettingBO, string FormId)
        {
            Guid Id = new Guid(FormId);
            try
            {
                using (var Context = DataObjectFactory.CreateContext())
                {
                    IQueryable<ResponseDisplaySetting> ColumnList = Context.ResponseDisplaySettings.Where(x => x.FormId == Id);

                    //Delete old columns
                    foreach (var item in ColumnList)
                    {
                        Context.ResponseDisplaySettings.DeleteObject(item);
                    }
                    Context.SaveChanges();

                    //insert new columns
                    ResponseDisplaySetting ResponseDisplaySettingEntity = new ResponseDisplaySetting();
                    foreach (var item in FormSettingBO.ResponseGridColumnNameList)
                    {

                        ResponseDisplaySettingEntity = Mapper.ToColumnName(item, Id);
                        Context.AddToResponseDisplaySettings(ResponseDisplaySettingEntity);
                    }

                    Context.SaveChanges();
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
            using (var context = DataObjectFactory.CreateContext())
            {
                try
                {
                    ////  Available DataAccess Rule Ids  list 

                    IQueryable<DataAccessRule> ruleIDs = context.DataAccessRules.ToList().AsQueryable();
                    foreach (var rule in ruleIDs)
                    {

                        dataAccessRuleIds.Add(rule.RuleId, rule.RuleName);
                        dataAccessRuleDescription.Add(rule.RuleName, rule.RuleDescription);

                    }

                    formSettingBO.SelectedDataAccessRule = selectedDataAccessRuleId;
                    formSettingBO.DataAccessRuleDescription = dataAccessRuleDescription;
                    formSettingBO.DataAccessRuleIds = dataAccessRuleIds;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            return formSettingBO;
        }

        public void DeleteDraftRecords(string FormId)
        {
            throw new NotImplementedException("Epi.Web.EF.DeleteDraftRecords");
        }
    }
}
