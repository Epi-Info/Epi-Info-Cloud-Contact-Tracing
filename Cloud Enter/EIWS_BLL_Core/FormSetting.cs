using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Interfaces.DataInterface;

namespace Epi.Web.BLL
{
    public class FormSetting
    {


        private readonly IFormSettingDao _formSettingDao;
        private readonly IUserDao _userDao;
        private readonly IFormInfoDao _formInfoDao;

        public FormSetting(IFormSettingDao formSettingDao, IUserDao userDao, IFormInfoDao formInfoDao)
        {
            _formSettingDao = formSettingDao;
            _userDao = userDao;
            _formInfoDao = formInfoDao;
        }

        public FormSetting(IFormSettingDao pFormSettingDao )
        {
            _formSettingDao = pFormSettingDao;
            
        }

        public FormSettingBO GetFormSettings(string FormId, string Xml,int CurrentOrgId = -1)
        {
            FormSettingBO result = _formSettingDao.GetFormSettings(FormId, CurrentOrgId);
            if (!string.IsNullOrEmpty(Xml))
            {
                result.FormControlNameList = GetFormColumnNames(Xml, result.ColumnNameList);
            }
            else
            {
                result.FormControlNameList = new Dictionary<int, string>();
                var Columns = GetAllColumns(FormId);

                for (int i = 0; i < Columns.Count; i++)
                {
                    result.FormControlNameList.Add(i, Columns[i]);
                }
            }
            return result;
        }

        private List<string> GetAllColumns(string FormId)
        {
            return _formSettingDao.GetAllColumnNames(FormId);
        }

        public Dictionary<int, string> GetFormColumnNames(string Xml, Dictionary<int, string> Selected)
        {
            Dictionary<int, string> List = new Dictionary<int, string>();

            XDocument xdoc = XDocument.Parse(Xml);


            var _FieldsTypeIDs = from _FieldTypeID in
                                     xdoc.Descendants("Field")

                                 select _FieldTypeID;
            int Count = 0;
            foreach (var _FieldTypeID in _FieldsTypeIDs)
            {
                if (!Selected.ContainsValue(_FieldTypeID.Attribute("Name").Value.ToString()) && _FieldTypeID.Attribute("FieldTypeId").Value != "2" && _FieldTypeID.Attribute("FieldTypeId").Value != "21" && _FieldTypeID.Attribute("FieldTypeId").Value != "3"
                    && _FieldTypeID.Attribute("FieldTypeId").Value != "4" && _FieldTypeID.Attribute("FieldTypeId").Value != "13" && _FieldTypeID.Attribute("FieldTypeId").Value != "20")
                {
                    List.Add(Count, _FieldTypeID.Attribute("Name").Value.ToString());
                    Count++;
                }
            }
            return List;

        }


        public FormSettingBO SaveSettings(Dictionary<int, string> ColumnList, bool IsDraftMode)
        {
            throw new NotImplementedException();
        }





       // public string SaveSettings(bool IsDraftMode, Dictionary<int, string> ColumnNameList, Dictionary<int, string> AssignedUserList, string FormId, Dictionary<int, string> SelectedOrgList, bool IsShareable)
        public string SaveSettings(bool IsDraftMode, FormSettingDTO FormSettingDTO)
        {
            string Message = "";
            FormSettingBO FormSettingBO = new FormSettingBO { FormId = FormSettingDTO.FormId };
           // FormSettingBO.ColumnNameList = FormSettingDTO.ColumnNameList;
            FormSettingBO.AssignedUserList = FormSettingDTO.AssignedUserList;
            FormSettingBO.SelectedOrgList = FormSettingDTO.SelectedOrgList;
            FormSettingBO.DeleteDraftData = FormSettingDTO.DeleteDraftData;
            //FormInfoBO FormInfoBO = new FormInfoBO();
            //FormInfoBO.FormId = FormSettingDTO.FormId;
            //FormInfoBO.IsDraftMode =IsDraftMode;
            //FormInfoBO.IsShareable = FormSettingDTO.IsShareable;
            try
            {
                List<UserBO> FormCurrentUsersList = _userDao.GetUserByFormId(FormSettingDTO.FormId);
                //FormSettingDao.UpDateColumnNames(FormSettingBO, FormSettingDTO.FormId);
               // FormSettingDao.UpDateFormMode(FormInfoBO);
                Dictionary<int, string> AssignedOrgAdminList = _formSettingDao.GetOrgAdmins(FormSettingDTO.SelectedOrgList);// about to share with
                List<UserBO> CurrentOrgAdminList = _formSettingDao.GetOrgAdminsByFormId(FormSettingDTO.FormId);// shared with 
                _formSettingDao.UpDateSettingsList(FormSettingBO, FormSettingDTO.FormId);

                // Clear all Draft records
                if (FormSettingDTO.DeleteDraftData)
                { 
                _formSettingDao.DeleteDraftRecords(FormSettingDTO.FormId);
                }

               List<UserBO> AdminList =  _userDao.GetAdminsBySelectedOrgs(FormSettingBO, FormSettingDTO.FormId);

                if (ConfigurationManager.AppSettings["SEND_EMAIL_TO_ASSIGNED_USERS"].ToUpper() == "TRUE" && FormSettingDTO.AssignedUserList.Count() > 0)
                {
                    SendEmail(FormSettingDTO.AssignedUserList, FormSettingDTO.FormId, FormCurrentUsersList);

                }

                // Send Email to organization admin when a form is shared with that organization
             SendEmail(AssignedOrgAdminList, FormSettingDTO.FormId, CurrentOrgAdminList,true);


                Message = "Success";
            }
            catch (Exception Ex)
            {
                Message = "Error";
                throw Ex;

            }
            return Message;
        }

        public void UpDateColumnNames(bool IsDraftMode, FormSettingDTO FormSettingDTO) 
        {
            FormSettingBO formSettingBO = new FormSettingBO();
            formSettingBO.ColumnNameList = FormSettingDTO.ColumnNameList;
            FormInfoBO formInfoBO = new FormInfoBO();
            formInfoBO.FormId = FormSettingDTO.FormId;
            formInfoBO.IsDraftMode = IsDraftMode;
            formInfoBO.IsShareable = FormSettingDTO.IsShareable;
            formInfoBO.DataAccesRuleId = FormSettingDTO.SelectedDataAccessRule;
            _formSettingDao.UpDateColumnNames(formSettingBO, FormSettingDTO.FormId);
            _formSettingDao.UpDateFormMode(formInfoBO);
            if (FormSettingDTO.IsDisabled)
            {

                _formSettingDao.SoftDeleteForm(FormSettingDTO.FormId);
            
            }

        }

        private void SendEmail(Dictionary<int, String> AssignedUserList, string FormId, List<UserBO> FormCurrentUsersList,bool ShareForm = false)
        {

            try
            {

                FormInfoBO FormInfoBO = _formInfoDao.GetFormByFormId(FormId);
                if (!string.IsNullOrEmpty(FormInfoBO.ParentId))
                {
                    return;
                }
                UserBO UserBO = _userDao.GetCurrentUser(FormInfoBO.UserId);
                List<string> UsersEmail = new List<string>();
                List<string> CurrentUsersEmail = new List<string>();

                foreach (UserBO User in FormCurrentUsersList)
                {
                    CurrentUsersEmail.Add(User.EmailAddress);
                }


                if (CurrentUsersEmail.Count() > 0)
                {

                    foreach (var User in AssignedUserList)
                    {
                        if (!CurrentUsersEmail.Contains(User.Value))
                        {

                            UsersEmail.Add(User.Value);
                        }

                    }
                }
                else
                {
                    foreach (var User in AssignedUserList)
                    {

                        UsersEmail.Add(User.Value);

                    }
                }
               
                if (UsersEmail.Count() > 0)
                {
                    Epi.Web.Enter.Common.Email.Email Email = new Web.Enter.Common.Email.Email();
                     if(!ShareForm ){
                   
                    Email.Body = UserBO.FirstName + " " + UserBO.LastName + " has assigned the following form to you in Epi Info™ Cloud Data Capture.\n\nTitle: " + FormInfoBO.FormName + " \n \n \nPlease click the link below to launch Epi Info™ Cloud Data Capture.";
                    Email.Body = Email.Body.ToString() + " \n \n" + ConfigurationManager.AppSettings["BaseURL"];
                    Email.From = UserBO.EmailAddress;
                    Email.To = UsersEmail;
                    Email.Subject = "An Epi Info Cloud Data Capture Form - " + FormInfoBO.FormName + " has been assigned to you";
                   
                     }
                     else
                     {
                          
                    Email.Body = UserBO.FirstName + " " + UserBO.LastName + " has shared the following form  with your organization in Epi Info™ Cloud Data Capture.\n\nTitle: " + FormInfoBO.FormName + " \n \n \nPlease click the link below to launch Epi Info™ Cloud Data Capture.";
                    Email.Body = Email.Body.ToString() + " \n \n" + ConfigurationManager.AppSettings["BaseURL"];
                    Email.From = UserBO.EmailAddress;
                    Email.To = UsersEmail;
                    Email.Subject = "An Epi Info Cloud Data Capture Form - " + FormInfoBO.FormName + " has been shared with your organization.";
                          
                     
                     }
                     Epi.Web.Enter.Common.Email.EmailHandler.SendMessage(Email);

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FormSettingBO GetFormSettings()
        {
            FormSettingBO result = _formSettingDao.GetFormSettings();
            return result;
        }
    }
}
