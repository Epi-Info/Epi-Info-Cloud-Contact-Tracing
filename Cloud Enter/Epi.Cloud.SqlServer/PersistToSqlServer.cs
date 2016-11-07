using System.Data;
using System.Data.SqlClient;
using Epi.Cloud.Common.Metadata;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.SqlServer
{
    public class PersistToSqlServer
    {
        // string connStr = ConfigurationHelper.GetEnvironmentResourceKey("EPIInfo7Entities");
        string connStr = "Server=tcp:eicsradev.database.windows.net,1433;Database=EICDCDev;User ID=eicsraadmin;Password=hK=JDZNjW@S8!pv;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
        public void PersistToSQLServerDB(FormResponseDetail objFormResponseDetail)
        {
            //TODO: Insert data for formInfo
            string ColumnNames = "RECSTATUS,GlobalRecordId,FirstSaveLogonName,FirstSaveTime,LastSaveLogonName,LastSaveTime,FKEY";

            string ColumnValues = objFormResponseDetail.RecStatus + ",'" + objFormResponseDetail.GlobalRecordID + "','" + objFormResponseDetail.FirstSaveLogonName + "','" + objFormResponseDetail.LastSaveTime + "','" +
                objFormResponseDetail.LastSaveLogonName + "','" + objFormResponseDetail.LastSaveTime + "'," + (objFormResponseDetail.RelateParentResponseId != null ? "'" + objFormResponseDetail.RelateParentResponseId + "'" : "NULL");

            objFormResponseDetail.FirstSaveLogonName = objFormResponseDetail.LastSaveLogonName = "SRANT\\Patelh";

            string UpdateColumnQuery = "RECSTATUS = '" + objFormResponseDetail.RecStatus +
                "', GlobalRecordId ='" + objFormResponseDetail.GlobalRecordID +
                "', FirstSaveLogonName ='" + objFormResponseDetail.FirstSaveLogonName +
                "', FirstSaveTime ='" + objFormResponseDetail.LastSaveTime +
                "', LastSaveLogonName ='" + objFormResponseDetail.LastSaveLogonName +
                "', LastSaveTime ='" + objFormResponseDetail.LastSaveTime +
                 "', FKEY ='" + (objFormResponseDetail.RelateParentResponseId != null ? objFormResponseDetail.RelateParentResponseId : "NULL") + "'";

            PersistToSql(objFormResponseDetail.GlobalRecordID, objFormResponseDetail.FormName, ColumnNames, ColumnValues, UpdateColumnQuery);

            //TODO: Insert/Update data for All Child Forms
            if (objFormResponseDetail.PageResponseDetailList.Count > 0)
            {
                foreach (PageResponseDetail objPageResoponse in objFormResponseDetail.PageResponseDetailList)
                {
                    if (objPageResoponse.ResponseQA.Count != 0)
                    {
                        string childFormName = objPageResoponse.FormName + objPageResoponse.PageId;
                        string ChildcolumnNames = "GlobalRecordId";
                        string ChildColumnValues = "'" + objFormResponseDetail.GlobalRecordID + "'";
                        string ChildColumnUpdateColumnQuery = string.Empty;

                        foreach (var responseQA in objPageResoponse.ResponseQA)
                        {
                            string ResponseKey = responseQA.Key;
                            string ResponseValue = responseQA.Value;


                            //if (!ResponseKey.Contains("groupbox") && !ResponseKey.StartsWith("grp") && !ResponseKey.EndsWith("grp"))
                            //{
                            MetadataAccessor getFieldDigest = new MetadataAccessor();
                            var FieldDataType = getFieldDigest.GetFieldDigestByFieldName(objPageResoponse.FormId, ResponseKey);

                            if (!(FieldDataType.FieldType.ToString().Equals(MetaFieldType.CommandButton.ToString()))
                                && (!FieldDataType.FieldType.ToString().Equals(MetaFieldType.LabelTitle.ToString()))
                                && (!FieldDataType.FieldType.ToString().Equals(MetaFieldType.Relate.ToString()))
                                && (!FieldDataType.FieldType.ToString().Equals(MetaFieldType.Group.ToString())))
                            {
                                ChildcolumnNames += ChildcolumnNames != string.Empty ? "," + ResponseKey : ResponseKey;


                                if ((!string.IsNullOrEmpty(ResponseValue)) && ResponseValue.Contains("'"))
                                {
                                    ResponseValue = ResponseValue.Replace("'", "''");
                                }

                                if (FieldDataType.FieldType.ToString().Equals(MetaFieldType.Checkbox.ToString()) || FieldDataType.FieldType.ToString().Equals(MetaFieldType.YesNo.ToString()))
                                {
                                    ResponseValue = ResponseValue.ToLower().ToString() == "yes" ? "1" : "0";

                                    ChildColumnValues += ChildColumnValues != string.Empty ? "," + ResponseValue : "" + 0;
                                    ChildColumnUpdateColumnQuery += ChildcolumnNames != string.Empty ? ResponseKey + "=" + ResponseValue + "," : "NULL";

                                }
                                else if (FieldDataType.FieldType.ToString().Equals(MetaFieldType.Number.ToString()))
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


                        PersistToSql(objPageResoponse.GlobalRecordID, childFormName, ChildcolumnNames, ChildColumnValues, ChildColumnUpdateColumnQuery);
                    }
                }
            }

            if (objFormResponseDetail.ChildFormResponseDetailList.Count > 0)
            {
                foreach (FormResponseDetail childformResponse in objFormResponseDetail.ChildFormResponseDetailList)
                {
                    PersistToSQLServerDB(childformResponse);
                }
            }
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
