using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Epi.Cloud.Common.Constants;
using Epi.Web.EF;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Interfaces.DataInterfaces;

namespace Epi.Cloud.SurveyInfoServices.DAO
{
    public class FormInfoDao : IFormInfoDao
    {
        public List<FormInfoBO> GetFormInfo(int userId, int currentOrgId)
        {
            List<FormInfoBO> formList = new List<FormInfoBO>();
            FormInfoBO formInfoBO;

            try
            {
                using (var context = DataObjectFactory.CreateContext())
                {
                    User CurrentUser = context.Users.Single(x => x.UserID == userId);

                    var UserOrganizations = CurrentUser.UserOrganizations.Where(x => x.RoleId == Roles.Administrator);

                    List<string> Assigned = GetAssignedForms(context, CurrentUser);

                    List<KeyValuePair<int, string>> Shared = GetSharedForms(currentOrgId, context);

                    // find the forms that are shared with the current user 
                    List<KeyValuePair<int, string>> SharedForms = new List<KeyValuePair<int, string>>();
                    foreach (var item in Shared)
                    {

                        if (UserOrganizations.Where(x => x.OrganizationID == item.Key).Count() > 0)
                        {
                            KeyValuePair<int, string> Item = new KeyValuePair<int, string>(item.Key, item.Value);
                            SharedForms.Add(Item);
                        }
                    }

                    var items = from FormInfo in context.SurveyMetaDatas
                                join UserInfo in context.Users
                                on FormInfo.OwnerId equals UserInfo.UserID
                                into temp
                                where FormInfo.ParentId == null

                                from UserInfo in temp.DefaultIfEmpty()
                                select new { FormInfo, UserInfo };

                    foreach (var item in items)
                    {
                        formInfoBO = Epi.Web.EF.Mapper.MapToFormInfoBO(item.FormInfo, item.UserInfo, false);
                        if (item.UserInfo.UserID == userId)
                        {
                            formInfoBO.IsOwner = true;
                            formList.Add(formInfoBO);
                        }
                        else
                        {
                            //Only Share or Assign
                            if (SharedForms.Where(x => x.Value == formInfoBO.FormId).Count() > 0)
                            {
                                formInfoBO.IsShared = true;
                                formInfoBO.UserId = userId;
                                //FormInfoBO.OrganizationId = Shared.FirstOrDefault(x => x.Value.Equals(FormInfoBO.FormId)).Key;
                                formInfoBO.OrganizationId = SharedForms.FirstOrDefault(x => x.Value.Equals(formInfoBO.FormId)).Key;
                                formList.Add(formInfoBO);
                            }
                            else if (Assigned.Contains(formInfoBO.FormId))
                            {
                                formInfoBO.IsOwner = false;
                                var UserOrgId = this.GetUserOrganization(formInfoBO.FormId, userId);
                                if (UserOrgId > -1)
                                {
                                    formInfoBO.OrganizationId = this.GetUserOrganization(formInfoBO.FormId, userId);
                                }
                                formList.Add(formInfoBO);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return formList;
        }

        private static List<KeyValuePair<int, string>> GetSharedForms(int currentOrgId, OSELS_EWEEntities context)
        {
            List<KeyValuePair<int, string>> shared = new List<KeyValuePair<int, string>>();
            IQueryable<SurveyMetaData> AllForms1 = context.SurveyMetaDatas.Where(x => x.ParentId == null
                && x.Organizations.Any(r => r.OrganizationId == currentOrgId)
                ).Distinct();
            foreach (var form in AllForms1)
            {
                // checking if the form is shared with any organization
                SurveyMetaData Response = context.SurveyMetaDatas.First(x => x.SurveyId == form.SurveyId);
                var _Org = new HashSet<int>(Response.Organizations.Select(x => x.OrganizationId));
                var Orgs = context.Organizations.Where(t => _Org.Contains(t.OrganizationId)).ToList();
                //if form is shared 
                if (Orgs.Count > 0)
                {
                    foreach (var org in Orgs)
                    {
                        KeyValuePair<int, string> Item = new KeyValuePair<int, string>(org.OrganizationId, form.SurveyId.ToString());

                        shared.Add(Item);
                    }
                }
            }
            return shared;
        }

        private static List<string> GetAssignedForms(OSELS_EWEEntities context, User currentUser)
        {
            List<string> assignedForms = new List<string>();
            IQueryable<SurveyMetaData> AllForms = context.SurveyMetaDatas.Where(x => x.ParentId == null
                && x.Users.Any(r => r.UserID == currentUser.UserID)
                ).Distinct();
            foreach (var form in AllForms)
            {
                if (form.Users.Contains(currentUser))
                {
                    assignedForms.Add(form.SurveyId.ToString());
                }
            }
            return assignedForms;
        }

        private int GetUserOrganization(string surveyId, int userId)
        {
            try
            {
                int orgId = -1;
                using (var context = DataObjectFactory.CreateContext())
                {
                    // get UserOrganization
                    var items = from UserOrgInfo in context.UserOrganizations
                                join OrgInfo in context.Organizations on UserOrgInfo.OrganizationID equals OrgInfo.OrganizationId into temp

                                where UserOrgInfo.UserID == userId

                                from query in temp.DefaultIfEmpty()
                                select new { query };
                    foreach (var Item in items)
                    {

                        var oIds = Item.query.SurveyMetaDatas.Where(x => x.SurveyId == new Guid(surveyId));
                        if (oIds.Count() > 0)
                        {
                            orgId = Item.query.OrganizationId;
                            break;
                        }
                    }

                    //var OId = items.Where(x => x.query.SurveyMetaDatas.Where(z => z.SurveyId == new Guid(SurveyId)) == items1);
                }

                return orgId;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public FormInfoBO GetFormByFormId(string formId, bool getMetadata, int userId)
        {
            FormInfoBO formInfoBO = new FormInfoBO();

            try
            {
                Guid Id = new Guid(formId);

                using (var context = DataObjectFactory.CreateContext())
                {
                    var items = from FormInfo in context.SurveyMetaDatas
                                join UserInfo in context.Users
                                on FormInfo.OwnerId equals UserInfo.UserID
                                into temp
                                from UserInfo in temp.DefaultIfEmpty()
                                where FormInfo.SurveyId == Id
                                select new { FormInfo, UserInfo };
                    SurveyMetaData Response = context.SurveyMetaDatas.First(x => x.SurveyId == Id);
                    var _Org = new HashSet<int>(Response.Organizations.Select(x => x.OrganizationId));
                    var Orgs = context.Organizations.Where(t => _Org.Contains(t.OrganizationId)).ToList();

                    bool isShared = false;

                    foreach (var org in Orgs)
                    {


                        var userInfo = context.UserOrganizations.Where(x => x.OrganizationID == org.OrganizationId && x.UserID == userId && x.RoleId == Roles.Administrator);
                        if (userInfo.Count() > 0)
                        {
                            isShared = true;
                            break;

                        }

                    }

                    foreach (var item in items)
                    {

                        formInfoBO = Mapper.MapToFormInfoBO(item.FormInfo, item.UserInfo, getMetadata);
                        formInfoBO.IsShared = isShared;

                        if (item.UserInfo.UserID == userId)
                        {
                            formInfoBO.IsOwner = true;
                        }
                        else
                        {
                            formInfoBO.IsOwner = false;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return formInfoBO;
        }

        public FormInfoBO GetFormByFormId(string formId)
        {
            FormInfoBO formInfoBO = new FormInfoBO();

            try
            {
                Guid Id = new Guid(formId);

                using (var Context = DataObjectFactory.CreateContext())
                {

                    SurveyMetaData SurveyMetaData = Context.SurveyMetaDatas.Single(x => x.SurveyId == Id);
                    formInfoBO = Mapper.ToFormInfoBO(SurveyMetaData);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return formInfoBO;
        }

        public bool HasDraftRecords(string formId)
        {
            try
            {
                bool hasDraftRecords = false;

                // TODO: DocumentDB implementation required
                //Guid Id = new Guid(formId);
                //using (var Context = DataObjectFactory.CreateContext())
                //{

                //    var DraftRecords = Context.SurveyResponses.Where(x => x.SurveyId == Id && x.IsDraftMode == true);
                //    if (DraftRecords.Count() > 0)
                //    {
                //        hasDraftRecords = true;

                //    }
                //}

                return hasDraftRecords;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
	}
}
