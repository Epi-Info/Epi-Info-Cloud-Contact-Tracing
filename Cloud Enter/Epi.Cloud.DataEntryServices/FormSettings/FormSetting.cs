using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.EmailServices;
using Epi.FormMetadata.Constants;

namespace Epi.Web.BLL
{
    public class FormSetting : MetadataAccessor
    {
        private readonly IFormSettingDao _formSettingDao;
        private readonly IUserDao _userDao;

        public FormSetting(IFormSettingDao formSettingDao, IUserDao userDao)
        {
            _formSettingDao = formSettingDao;
            _userDao = userDao;
        }

        public FormSettingBO GetFormSettings(string formId, int currentOrgId = -1)
        {
            FormSettingBO result = _formSettingDao.GetFormSettings(formId, currentOrgId);

            return result;
        }

        public string SaveSettings(bool isDraftMode, FormSettingDTO formSettingDTO)
        {
            string message = "";
            FormSettingBO formSettingBO = new FormSettingBO();
            formSettingBO.AssignedUserList = formSettingDTO.AssignedUserList;
            formSettingBO.SelectedOrgList = formSettingDTO.SelectedOrgList;
            formSettingBO.DeleteDraftData = formSettingDTO.DeleteDraftData;
            try
            {
                List<UserBO> formCurrentUsersList = _userDao.GetUserByFormId(formSettingDTO.FormId);
                Dictionary<int, string> assignedOrgAdminList = _formSettingDao.GetOrgAdmins(formSettingDTO.SelectedOrgList);// about to share with
                List<UserBO> CurrentOrgAdminList = _formSettingDao.GetOrgAdminsByFormId(formSettingDTO.FormId);// shared with 
                _formSettingDao.UpdateSettingsList(formSettingBO, formSettingDTO.FormId);

                // Clear all Draft records
                if (formSettingDTO.DeleteDraftData)
                {
                    _formSettingDao.DeleteDraftRecords(formSettingDTO.FormId);
                }

                List<UserBO> AdminList = _userDao.GetAdminsBySelectedOrgs(formSettingBO, formSettingDTO.FormId);

                if (formSettingDTO.AssignedUserList.Count() > 0 && AppSettings.Key.SendEmailToAssignedUsers.GetBoolValue())
                {
                    SendEmail(formSettingDTO.AssignedUserList, formSettingDTO.FormId, formCurrentUsersList);
                }

                // Send Email to organization admin when a form is shared with that organization
                SendEmail(assignedOrgAdminList, formSettingDTO.FormId, CurrentOrgAdminList, true);

                message = "Success";
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
            FormSettingBO formSettingBO = new FormSettingBO();
            formSettingBO.ColumnNameList = formSettingDTO.ColumnNameList;
            FormInfoBO formInfoBO = new FormInfoBO();
            formInfoBO.FormId = formSettingDTO.FormId;
            formInfoBO.IsDraftMode = isDraftMode;
            formInfoBO.IsShareable = formSettingDTO.IsShareable;
            formInfoBO.DataAccesRuleId = formSettingDTO.SelectedDataAccessRule;
            _formSettingDao.UpdateColumnNames(formSettingBO, formSettingDTO.FormId);
            _formSettingDao.UpdateFormMode(formInfoBO, formSettingBO);
            if (formSettingDTO.IsDisabled)
            {
                _formSettingDao.SoftDeleteForm(formSettingDTO.FormId);
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
