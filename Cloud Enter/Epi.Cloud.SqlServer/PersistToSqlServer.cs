using System.Data;
using System.Linq;
using System.Data.SqlClient;
using Epi.Cloud.Common.Metadata;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.MetadataServices.Common.ProxyService;
using Epi.FormMetadata.Constants;

namespace Epi.Cloud.SqlServer
{
    public class PersistToSqlServer
    {
        PageDigest[][] PageDigests;
        // string connStr = ConfigurationHelper.GetEnvironmentResourceKey("EPIInfo7Entities");
        string connStr = "Server=tcp:eicsraqa.database.windows.net,1433;Database=EICDCQA;User ID=eicsraadmin;Password=+Z18]B/h-1F862v;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
        public void PersistToSQLServerDB(FormResponseDetail objFormResponseDetail, PageDigest[][] PageDigestsVal)
        {
            PageDigests = PageDigestsVal;
            //TODO: Insert data for formInfo
            string ColumnNames = "RECSTATUS,GlobalRecordId,FirstSaveLogonName,FirstSaveTime,LastSaveLogonName,LastSaveTime,FKEY";

            string ColumnValues = objFormResponseDetail.RecStatus + ",'" + objFormResponseDetail.ResponseId + "','" + objFormResponseDetail.FirstSaveLogonName + "','" + objFormResponseDetail.LastSaveTime + "','" +
                objFormResponseDetail.LastSaveLogonName + "','" + objFormResponseDetail.LastSaveTime + "'," + (objFormResponseDetail.ParentResponseId != null ? "'" + objFormResponseDetail.ParentResponseId + "'" : "NULL");

            objFormResponseDetail.FirstSaveLogonName = objFormResponseDetail.LastSaveLogonName = "SRANT\\Patelh";

            string UpdateColumnQuery = "RECSTATUS = '" + objFormResponseDetail.RecStatus +
                "', GlobalRecordId ='" + objFormResponseDetail.ResponseId +
                "', FirstSaveLogonName ='" + objFormResponseDetail.FirstSaveLogonName +
                "', FirstSaveTime ='" + objFormResponseDetail.LastSaveTime +
                "', LastSaveLogonName ='" + objFormResponseDetail.LastSaveLogonName +
                "', LastSaveTime ='" + objFormResponseDetail.LastSaveTime +
                 "', FKEY ='" + (objFormResponseDetail.ParentResponseId != null ? objFormResponseDetail.ParentResponseId : "NULL") + "'";

            PersistToSql(objFormResponseDetail.ResponseId, objFormResponseDetail.FormName, ColumnNames, ColumnValues, UpdateColumnQuery);


            // PageDigests = this.GetPageDigest(); 

            //TODO: Insert/Update data for All Child Forms
            if (objFormResponseDetail.PageResponseDetailList.Count > 0)
            {
                foreach (PageResponseDetail objPageResoponse in objFormResponseDetail.PageResponseDetailList)
                {
                    if (objPageResoponse.ResponseQA.Count != 0)
                    {
                        string childFormName = objPageResoponse.FormName + objPageResoponse.PageId;
                        string ChildcolumnNames = "GlobalRecordId";
                        string ChildColumnValues = "'" + objFormResponseDetail.ResponseId + "'";
                        string ChildColumnUpdateColumnQuery = string.Empty;

                        foreach (var responseQA in objPageResoponse.ResponseQA)
                        {
                            string ResponseKey = responseQA.Key;
                            string ResponseValue = responseQA.Value;


                            //if (!ResponseKey.Contains("groupbox") && !ResponseKey.StartsWith("grp") && !ResponseKey.EndsWith("grp"))
                            //{
                                MetadataAccessor getFieldDigest = new MetadataAccessor();
                                //var FieldDataType = getFieldDigest.GetFieldDigestByFieldName(objPageResoponse.FormId, ResponseKey);

                                var FieldDataType = GetFieldDataTypeByFieldName(objPageResoponse.FormId, objPageResoponse.PageId, ResponseKey);


                                //if (!(FieldDataType.ToString().Equals(MetaFieldType.CommandButton.ToString()))
                                //    && (!FieldDataType.ToString().Equals(MetaFieldType.LabelTitle.ToString()))
                                //    && (!FieldDataType.ToString().Equals(MetaFieldType.Relate.ToString()))
                                //    && (!FieldDataType.ToString().Equals(MetaFieldType.Group.ToString())))
                                if(!(FieldDataType.ToString()== "Unknown"))
                                {
                                    ChildcolumnNames += ChildcolumnNames != string.Empty ? "," + ResponseKey : ResponseKey;


                                    if ((!string.IsNullOrEmpty(ResponseValue)) && ResponseValue.Contains("'"))
                                    {
                                        ResponseValue = ResponseValue.Replace("'", "''");
                                    }

                                    //if (FieldDataType.ToString().Equals(MetaFieldType.Checkbox.ToString()) || FieldDataType.ToString().Equals(MetaFieldType.YesNo.ToString()))
                                    if (FieldDataType.ToString()== "Boolean" || FieldDataType.ToString() == "YesNo") 
                                    {
                                        ResponseValue = ResponseValue.ToLower().ToString() == "yes" ? "1" : "0";

                                        ChildColumnValues += ChildColumnValues != string.Empty ? "," + ResponseValue : "" + 0;
                                        ChildColumnUpdateColumnQuery += ChildcolumnNames != string.Empty ? ResponseKey + "=" + ResponseValue + "," : "NULL";

                                    }
                                    //else if (FieldDataType.ToString().Equals(MetaFieldType.Number.ToString()))
                                    else if (FieldDataType.ToString().Equals("Number"))
                                    {
                                        ChildColumnValues += ChildColumnValues != string.Empty ? "," + (ResponseValue != string.Empty ? ResponseValue : "NULL") : "NULL";
                                        ChildColumnUpdateColumnQuery += ChildcolumnNames != string.Empty ? ResponseKey + "=" + (ResponseValue != string.Empty ? ResponseValue : "NULL") + "," : "NULL";

                                    }
                                    else
                                    {
                                        ChildColumnValues += ChildColumnValues != string.Empty ? ",'" + (ResponseValue != string.Empty ? ResponseValue : "NULL") + "'" : ResponseValue != string.Empty ? ResponseValue : "NULL";
                                        ChildColumnUpdateColumnQuery += ChildcolumnNames != string.Empty ? ResponseKey + "='" + (ResponseValue != string.Empty ? ResponseValue : "NULL") + "'," : "NULL";

                                    }
                                }
                            //}
                        }


                        if (ChildColumnUpdateColumnQuery.EndsWith(","))
                        {
                            ChildColumnUpdateColumnQuery = ChildColumnUpdateColumnQuery.Substring(0, ChildColumnUpdateColumnQuery.Length - ",".Length);
                        }


                        PersistToSql(objPageResoponse.ResponseId, childFormName, ChildcolumnNames, ChildColumnValues, ChildColumnUpdateColumnQuery);
                    }
                }
            }

            if (objFormResponseDetail.ChildFormResponseDetailList.Count > 0)
            {
                foreach (FormResponseDetail childformResponse in objFormResponseDetail.ChildFormResponseDetailList)
                {
                    PersistToSQLServerDB(childformResponse, PageDigests);
                }
            }
        }


        public PageDigest[][] GetPageDigest()
        {

            ProjectMetadataServiceProxy ProjectMetadataServiceProxy = new ProjectMetadataServiceProxy();
            Epi.FormMetadata.DataStructures.PageDigest[][] pageDigest = ProjectMetadataServiceProxy.GetPageDigestMetadataAsync().Result;
            return pageDigest;
        }

        public PageDigest GetPageDigestByPageId(string formId, int pageId)
        {
            var pageDigests = PageDigests.Single(d => d[0].FormId == formId);
            var pageDigest = pageId > 0 ? pageDigests.Single(d => d.PageId == pageId) : pageDigests.FirstOrDefault();
            return pageDigest;
        }

        public AbridgedFieldInfo GetFieldInfoByFieldName(string formId, int pageId, string fieldName)
        {
            fieldName = fieldName.ToLower();
            var fieldDigest = GetPageDigestByPageId(formId, pageId);
            var fieldInfo = fieldDigest.Fields.Where(f => f.FieldName == fieldName).SingleOrDefault();
            return fieldInfo;
        }

        public FieldDataType GetFieldDataTypeByFieldName(string formId, int pageId, string fieldName)
        {
            var fieldInfo = GetFieldInfoByFieldName(formId, pageId, fieldName);
            return fieldInfo != null ? fieldInfo.DataType : FieldDataType.Undefined;
        }

        public void PersistToSql(string GlobalRecordId, string TableName, string ColumnNames, string ColumnValues, string UpdateColumnQuery)
        {
            ColumnValues = ColumnValues.Replace("'NULL'", "NULL");
            UpdateColumnQuery = UpdateColumnQuery.Replace("'NULL'", "NULL");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("usp_SyncToDocumentDb", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@GlobalRecordId", SqlDbType.VarChar).Value = GlobalRecordId;
                    cmd.Parameters.Add("@TableName", SqlDbType.VarChar).Value = TableName;
                    cmd.Parameters.Add("@ColumnNames", SqlDbType.VarChar).Value = ColumnNames;
                    cmd.Parameters.Add("@ColumnValues", SqlDbType.VarChar).Value = ColumnValues;
                    cmd.Parameters.Add("@UpdateColumnQuery", SqlDbType.VarChar).Value = UpdateColumnQuery;

                    con.Open();
                    int recordsAffected = cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
