using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.EF
{
    public class EntityFormSettingDao : IFormSettingDao_EF
    {
        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId)
        {
            List<FormSettingBO> formSettingList = new List<FormSettingBO>();
            foreach (string formId in formIds)
            {
                formSettingList.Add(GetFormSettings(formId, currentOrgId));
            }
            return formSettingList;
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId)
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

                    ////  Available Orgnization list 
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
                        User User = context.Users.FirstOrDefault(x => x.UserName == item.Value);
                        response.Users.Add(User);

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

        public List<string> GetAllColumnNames(string FormId)
        {
            Guid Id = new Guid(FormId);
            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    List<string> columns = (from c in context.SurveyMetaDataTransforms
                                            where c.SurveyId == Id &&
                                            //(c.FieldTypeId != 2 && c.FieldTypeId != 3 && c.FieldTypeId != 17 && c.FieldTypeId != 20 && c.FieldTypeId != 21) //filter non-data fields.
                                            (c.FieldTypeId != 2 && c.FieldTypeId != 3 && c.FieldTypeId != 4 && c.FieldTypeId != 13 && c.FieldTypeId != 20 &&  c.FieldTypeId != 21) //filter non-data fields.
                                            orderby c.FieldName
                                            select c.FieldName).ToList();
                    return columns;
                }
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
                foreach (var org in selectedOrgList)
                {
                    using (var context = DataObjectFactory.CreateContext())
                    {
                        int orgId = int.Parse(org.Value);

                        var adminList = context.UserOrganizations.Where(x => x.OrganizationID == orgId && x.RoleId == Roles.Administrator && x.Active == true).ToList();

                        int i = 0;
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


        // vvvvvvvvvvvvvvvvvvvvvvvvvvv Implemented in FormInfoServices/DAO/FormSettingDao vvvvvvvvvvvvvvvvvvvvvvvvvvv //

        public void UpdateColumnNames(FormSettingBO FormSettingBO, string FormId)
        {
            throw new NotImplementedException("Epi.Web.EF.UpdateColumnNames. Implemented in Epi.Cloud.FormInfoServices/DAO/FormSettingDao");
#if false
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
                    foreach (var item in FormSettingBO.ColumnNameList)
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
#endif
        }

        public FormSettingBO GetFormSettings()
        {
            throw new NotImplementedException("Epi.Web.EF.GetFormSettings. Implemented in Epi.Cloud.FormInfoServices/DAO/FormSettingDao.");
#if false
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
#endif
        }

        public void DeleteDraftRecords(string FormId)
        {
            throw new NotImplementedException("Epi.Web.EF.DeleteDraftRecords");
        }
    }
}
