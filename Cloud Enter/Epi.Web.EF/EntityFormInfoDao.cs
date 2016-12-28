using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Interfaces.DataInterfaces;
using System.Data;
using Epi.Cloud.Common.Constants;

namespace Epi.Web.EF
{
    public class EntityFormInfoDao : IFormInfoDao
    {
        public List<FormInfoBO> GetFormInfo(int UserId, int CurrentOrgId)
        {
            List<FormInfoBO> FormList = new List<FormInfoBO>();
            FormInfoBO FormInfoBO;

            try
            {
                int Id = UserId;

                using (var Context = DataObjectFactory.CreateContext())
                {
                    User CurrentUser = Context.Users.Single(x => x.UserID == Id);

                    var UserOrganizations = CurrentUser.UserOrganizations.Where(x => x.RoleId == Roles.OrgAdministrator);

                    List<string> Assigned = GetAssignedForms(Context, CurrentUser);

                    List<KeyValuePair<int, string>> Shared = GetSharedForms(CurrentOrgId, Context);

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

                    var items = from FormInfo in Context.SurveyMetaDatas
                                join UserInfo in Context.Users
                                on FormInfo.OwnerId equals UserInfo.UserID
                                into temp
                                where FormInfo.ParentId == null

                                from UserInfo in temp.DefaultIfEmpty()
                                select new { FormInfo, UserInfo };


                    foreach (var item in items)
                    {
                        FormInfoBO = Mapper.MapToFormInfoBO(item.FormInfo, item.UserInfo, false);

                        if (item.UserInfo.UserID == Id)
                        {
                            FormInfoBO.IsOwner = true;
                            FormList.Add(FormInfoBO);
                        }
                        else
                        {
                            //Only Share or Assign
                            if (SharedForms.Where(x => x.Value == FormInfoBO.FormId).Count() > 0)
                            {
                                FormInfoBO.IsShared = true;
                                FormInfoBO.UserId = Id;
                                FormInfoBO.OrganizationId = SharedForms.FirstOrDefault(x => x.Value.Equals(FormInfoBO.FormId)).Key;
                                FormList.Add(FormInfoBO);
                            }
                            else if (Assigned.Contains(FormInfoBO.FormId))
                            {
                                FormInfoBO.IsOwner = false;
                                var UserOrgId = this.GetUserOrganization(FormInfoBO.FormId, UserId);
                                if (UserOrgId > -1)
                                {
                                    FormInfoBO.OrganizationId = this.GetUserOrganization(FormInfoBO.FormId, UserId);
                                }
                                FormList.Add(FormInfoBO);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

			return FormList;
        }

        public FormInfoBO GetFormByFormId(string FormId, bool getMetadata, int UserId)
        {
            FormInfoBO FormInfoBO = new FormInfoBO();

            try
            {
                Guid Id = new Guid(FormId);

                using (var Context = DataObjectFactory.CreateContext())
                {
                    var items = from FormInfo in Context.SurveyMetaDatas
                                join UserInfo in Context.Users
                                on FormInfo.OwnerId equals UserInfo.UserID
                                into temp
                                from UserInfo in temp.DefaultIfEmpty()
                                where FormInfo.SurveyId == Id
                                select new { FormInfo, UserInfo };
                    SurveyMetaData Response = Context.SurveyMetaDatas.First(x => x.SurveyId == Id);
                    var _Org = new HashSet<int>(Response.Organizations.Select(x => x.OrganizationId));
                    var Orgs = Context.Organizations.Where(t => _Org.Contains(t.OrganizationId)).ToList();

					bool IsShared = false;

                    foreach (var org in Orgs)
                    {
                        var UserInfo = Context.UserOrganizations.Where(x => x.OrganizationID == org.OrganizationId && x.UserID == UserId && x.RoleId == Roles.OrgAdministrator);
                        if (UserInfo.Count() > 0)
                        {
                            IsShared = true;
                            break;

                        }
                    }

                    foreach (var item in items)
                    {
                        FormInfoBO = Mapper.MapToFormInfoBO(item.FormInfo, item.UserInfo, getMetadata);
                        FormInfoBO.IsShared = IsShared;

                        if (item.UserInfo.UserID == UserId)
                        {
                            FormInfoBO.IsOwner = true;
                        }
                        else
                        {
                            FormInfoBO.IsOwner = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

			return FormInfoBO;
        }

        public FormInfoBO GetFormByFormId(string FormId)
        {
            FormInfoBO FormInfoBO = new FormInfoBO();

            try
            {
                Guid Id = new Guid(FormId);

                using (var Context = DataObjectFactory.CreateContext())
                {
                    SurveyMetaData SurveyMetaData = Context.SurveyMetaDatas.Single(x => x.SurveyId == Id);
                    FormInfoBO = Mapper.ToFormInfoBO(SurveyMetaData);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

			return FormInfoBO;
        }

        public bool HasDraftRecords(string FormId)
        {
            try
            {
                // TODO: DocumentDB implementation required
                Guid Id = new Guid(FormId);
                bool _HasDraftRecords = false;
                using (var Context = DataObjectFactory.CreateContext())
                {

                    var DraftRecords = Context.SurveyResponses.Where(x => x.SurveyId == Id && x.IsDraftMode == true);
                    if (DraftRecords.Count() > 0)
                    {
                        _HasDraftRecords = true;

                    }
                }

                return _HasDraftRecords;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

		private static List<KeyValuePair<int, string>> GetSharedForms(int CurrentOrgId, OSELS_EWEEntities Context)
		{
			List<KeyValuePair<int, string>> Shared = new List<KeyValuePair<int, string>>();
			IQueryable<SurveyMetaData> AllForms1 = Context.SurveyMetaDatas.Where(x => x.ParentId == null
				&& x.Organizations.Any(r => r.OrganizationId == CurrentOrgId)).Distinct();
			foreach (var form in AllForms1)
			{
				// checking if the form is shared with any organization
				SurveyMetaData Response = Context.SurveyMetaDatas.First(x => x.SurveyId == form.SurveyId);
				var _Org = new HashSet<int>(Response.Organizations.Select(x => x.OrganizationId));
				var Orgs = Context.Organizations.Where(t => _Org.Contains(t.OrganizationId)).ToList();
				//if form is shared 
				if (Orgs.Count > 0)
				{
					foreach (var org in Orgs)
					{
						KeyValuePair<int, string> Item = new KeyValuePair<int, string>(org.OrganizationId, form.SurveyId.ToString());

						Shared.Add(Item);
					}
				}
			}
			return Shared;
		}

		private static List<string> GetAssignedForms(OSELS_EWEEntities Context, User CurrentUser)
		{
			List<string> assigned = new List<string>();
			IQueryable<SurveyMetaData> AllForms = Context.SurveyMetaDatas.Where(x => x.ParentId == null
				&& x.Users.Any(r => r.UserID == CurrentUser.UserID)).Distinct();

			foreach (var form in AllForms)
			{
				if (form.Users.Contains(CurrentUser))
				{
					assigned.Add(form.SurveyId.ToString());
				}
			}
			return assigned;
		}

		private int GetUserOrganization(string SurveyId, int UserId)
		{
			try
			{
				int OrgId = -1;
				using (var Context = DataObjectFactory.CreateContext())
				{
					// get UserOrganization
					var items = from UserOrgInfo in Context.UserOrganizations
								join OrgInfo in Context.Organizations on UserOrgInfo.OrganizationID equals OrgInfo.OrganizationId into temp

								where UserOrgInfo.UserID == UserId

								from query in temp.DefaultIfEmpty()
								select new { query };

					foreach (var Item in items)
					{

						var OId = Item.query.SurveyMetaDatas.Where(x => x.SurveyId == new Guid(SurveyId));
						if (OId.Count() > 0)
						{
							OrgId = Item.query.OrganizationId;
							break;
						}
					}
				}

				return OrgId;
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}
	}
}
