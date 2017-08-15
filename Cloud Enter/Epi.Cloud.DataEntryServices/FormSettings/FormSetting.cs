using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.EmailServices;

namespace Epi.Web.BLL
{
    public class FormSetting : MetadataAccessor
    {
        private readonly IFormSettingFacade _formSettingFacade;
        private readonly IUserDao _userDao;

        public FormSetting(IFormSettingFacade formSettingFacade, IUserDao userDao)
        {
            _formSettingFacade = formSettingFacade;
            _userDao = userDao;
        }

        public List<FormSettingBO> GetFormSettingsList(List<string> formIds, int currentOrgId = -1)
        {
            List<FormSettingBO> result = _formSettingFacade.GetFormSettingsList(formIds, currentOrgId);
            return result;
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId = -1)
        {
            FormSettingBO result = _formSettingFacade.GetFormSettings(formId, currentOrgId);

            return result;
        }

        public string SaveSettings(bool isDraftMode, FormSettingDTO formSettingDTO, int CurrentOrg = -1)
        {
            var formId = formSettingDTO.FormId;
            var isShareable = formSettingDTO.IsShareable;
            var dataAccessRuleId = formSettingDTO.SelectedDataAccessRule;

            string message = "";
            FormSettingBO formSettingBO = new FormSettingBO { FormId = formId };
            formSettingBO.AssignedUserList = formSettingDTO.AssignedUserList;
            formSettingBO.SelectedOrgList = formSettingDTO.SelectedOrgList;
            formSettingBO.DeleteDraftData = formSettingDTO.DeleteDraftData;
            try
            {
                List<UserBO> formCurrentUsersList = _userDao.GetUserByFormId(formSettingDTO.FormId);
                Dictionary<int, string> assignedOrgAdminList = _formSettingFacade.GetOrgAdmins(formSettingDTO.SelectedOrgList);// about to share with
                List<UserBO> CurrentOrgAdminList = _formSettingFacade.GetOrgAdminsByFormId(formSettingDTO.FormId);// shared with 
                _formSettingFacade.UpdateSettingsList(formSettingBO, formSettingDTO.FormId,CurrentOrg);

                // Clear all Draft records
                if (formSettingDTO.DeleteDraftData)
                {
                    _formSettingFacade.DeleteDraftRecords(formSettingDTO.FormId);
                }

                List<UserBO> AdminList = _userDao.GetAdminsBySelectedOrgs(formSettingBO, formSettingDTO.FormId);

                if (formSettingDTO.AssignedUserList.Count() > 0 && AppSettings.Key.SendEmailToAssignedUsers.GetBoolValue())
                {
                    SendEmail(formSettingDTO.AssignedUserList, formSettingDTO.FormId, formCurrentUsersList);
                }

                // Send Email to organization admin when a form is shared with that organization
                SendEmail(assignedOrgAdminList, formSettingDTO.FormId, CurrentOrgAdminList, true);

                message = "Success";

                UpdateMetadataIfNecessary(formId, isDraftMode, isShareable, dataAccessRuleId);

            }
            catch (Exception Ex)
            {
                message = "Error";
                throw Ex;

            }
            return message;
        }

        public void UpdateFormSettings(bool isDraftMode, FormSettingDTO formSettingDTO)
        {
            var formId = formSettingDTO.FormId;
            var isShareable = formSettingDTO.IsShareable;
            var dataAccessRuleId = formSettingDTO.SelectedDataAccessRule;

            FormSettingBO formSettingBO = new FormSettingBO { FormId = formSettingDTO.FormId };
            formSettingBO.ResponseGridColumnNameList = formSettingDTO.ColumnNameList;
            FormInfoBO formInfoBO = new FormInfoBO();
            formInfoBO.FormId = formId;
            formInfoBO.IsDraftMode = isDraftMode;
            formInfoBO.IsShareable = isShareable;
            formInfoBO.DataAccesRuleId = dataAccessRuleId;
            _formSettingFacade.UpdateResponseGridColumnNames(formSettingBO, formSettingDTO.FormId);
            _formSettingFacade.UpdateFormMode(formInfoBO, formSettingBO);
            if (formSettingDTO.IsDisabled)
            {
                _formSettingFacade.SoftDeleteForm(formSettingDTO.FormId);
            }
        }

        private void SendEmail(Dictionary<int, String> assignedUserList, string formId, List<UserBO> formCurrentUsersList, bool shareForm = false)
        {
            try
            {
                var formDigest = GetFormDigest(formId);
                if (formDigest.IsChildForm) return;

                var formOwnerUserId = formDigest.OwnerUserId;

                UserBO userBO = _userDao.GetCurrentUser(formOwnerUserId);
                List<string> usersEmail = new List<string>();
                List<string> currentUsersEmail = new List<string>();

                foreach (UserBO user in formCurrentUsersList)
                {
                    currentUsersEmail.Add(user.EmailAddress);
                }

                if (currentUsersEmail.Count() > 0)
                {
                    foreach (var User in assignedUserList)
                    {
                        if (!currentUsersEmail.Contains(User.Value))
                        {

                            usersEmail.Add(User.Value);
                        }
                    }
                }
                else
                {
                    foreach (var user in assignedUserList)
                    {
                        usersEmail.Add(user.Value);
                    }
                }

                if (usersEmail.Count() > 0)
                {
                    Email email = new Email();
                    email.To = usersEmail;
                    email.From = userBO.EmailAddress;
                    if (shareForm)
                    {
                        //email.Subject = "An Epi Info Cloud Enter Form - " + formDigest.FormName + " has been shared with your organization.";
                        //email.Body = userBO.FirstName + " " + userBO.LastName + " has shared the following form  with your organization in Epi Info™ Cloud Enter.\n\nTitle: " + formDigest.FormName + " \n \n \nPlease click the link below to launch Epi Info™ Cloud Enter.";
                        //email.Body = email.Body.ToString() + " \n \n" + AppSettings.GetStringValue(AppSettings.Key.BaseURL);
                        email.Subject = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.FormSharedWithYourOrganization_Subject),
                            formDigest.FormName);
                        email.Body = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.FormSharedWithYourOrganization_Body),
                            userBO.FirstName, userBO.LastName, formDigest.FormName, AppSettings.GetStringValue(AppSettings.Key.BaseURL));
                    }
                    else
                    {
                        //email.Subject = "An Epi Info Cloud Enter Form - " + formDigest.FormName + " has been assigned to you";
                        //email.Body = userBO.FirstName + " " + userBO.LastName + " has assigned the following form to you in Epi Info™ Cloud Enter.\n\nTitle: " + formDigest.FormName + " \n \n \nPlease click the link below to launch Epi Info™ Cloud Enter.";
                        //email.Body = email.Body.ToString() + " \n \n" + AppSettings.GetStringValue(AppSettings.Key.BaseURL);
                        email.Subject = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.FormAssignedToYou_Subject),
                            formDigest.FormName);
                        email.Body = string.Format(ResourceProvider.GetResourceString(ResourceNamespaces.EmailMessages, EmailResourceKeys.FormAssignedToYou_Body), 
                            userBO.FirstName, userBO.LastName, formDigest.FormName, AppSettings.GetStringValue(AppSettings.Key.BaseURL));
                    }

                    EmailHandler.SendMessage(email);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
