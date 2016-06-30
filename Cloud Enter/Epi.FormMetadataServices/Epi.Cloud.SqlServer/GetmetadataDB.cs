﻿// This class is not used. It is here if we need to retrieve the raw metadata from SqlServer.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Epi.Data.SqlServer
{
    public class GetmetadataDB
    {
        // string connStr = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        //string connStr = "Server=tcp:gelwebenterss.database.windows.net,1433;Database=EpiinfoCloudDB;User ID=saglenz@gelwebenterss;Password=Password123!@#;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
        string connStr = "Server=tcp:gelwebenterss.database.windows.net,1433;Database=EPIInfo7;User ID=saglenz@gelwebenterss;Password=Password123!@#;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";

        public List<CDTProject> MetaDataAsync(int pageid)
        {
            List<CDTProject> pfileds = new List<CDTProject>();
            DataTable dt = new DataTable();
            string query = GetFieldsOnPageAsDataTable(pageid);
            SqlConnection connection = new SqlConnection(connStr);
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                }
                foreach (DataRow dr in dt.Rows)
                {
                    CDTProject pfiled = new CDTProject();
                    //pfileds = new List<pfiled>();
                    pfiled.PageName = dr["Name"].ToString();
                    pfiled.PageId = dr["PageId"].ToString();
                    pfiled.FieldId = dr["FieldId"].ToString();
                    pfiled.UniqueId = dr["UniqueId"].ToString();
                    pfiled.FieldTypeId = (dr["FieldTypeId"]) == DBNull.Value ? (int?)null : Convert.ToInt32(dr["FieldTypeId"]);
                    pfiled.ControlAfterCheckCode = dr["ControlAfterCheckCode"].ToString();
                    pfiled.ControlBeforeCheckCode = dr["ControlBeforeCheckCode"].ToString();
                    pfiled.PageName = dr["PageName"].ToString();
                    pfiled.PageBeforeCheckCode = dr["PageBeforeCheckCode"].ToString();
                    pfiled.PageAfterCheckCode = dr["PageAfterCheckCode"].ToString();
                    pfiled.Position = dr["Position"].ToString();
                    pfiled.ControlTopPositionPercentage = Convert.ToDouble(dr["ControlTopPositionPercentage"]);
                    pfiled.ControlLeftPositionPercentage = Convert.ToDouble(dr["ControlLeftPositionPercentage"]);
                    pfiled.ControlHeightPercentage = Convert.ToDouble(dr["ControlHeightPercentage"]);
                    pfiled.ControlWidthPercentage = Convert.ToDouble(dr["ControlWidthPercentage"]);
                    pfiled.ControlFontFamily = dr["ControlFontFamily"].ToString();
                    pfiled.ControlFontSize = Convert.ToDouble(dr["ControlFontSize"]);
                    pfiled.ControlFontStyle = dr["ControlFontStyle"].ToString();
                    pfiled.ControlScriptName = dr["ControlScriptName"].ToString();
                    pfiled.PromptTopPositionPercentage = dr["PromptTopPositionPercentage"] == DBNull.Value ? (double?)null : Convert.ToDouble(dr["PromptTopPositionPercentage"]);
                    pfiled.PromptLeftPositionPercentage = (dr["PromptLeftPositionPercentage"]) == DBNull.Value ? (double?)null : Convert.ToDouble(dr["PromptLeftPositionPercentage"]);
                    pfiled.PromptText = dr["PromptText"].ToString();
                    pfiled.PromptFontSize = (dr["PromptFontSize"]) == DBNull.Value ? (double?)null : Convert.ToDouble(dr["PromptFontSize"]);
                    pfiled.PromptFontStyle = dr["PromptFontStyle"].ToString();
                    pfiled.PromptFontFamily = dr["PromptFontFamily"].ToString();
                    //publi string ControlFontFamily{get;set;}
                    //  ControlFontSize  
                    pfiled.PromptScriptName = dr["PromptScriptName"].ToString();
                    pfiled.ShouldRepeatLast = dr["ShouldRepeatLast"].ToString();
                    pfiled.IsRequired = (dr["IsRequired"]) == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["IsRequired"]);
                    pfiled.IsReadOnly = (dr["IsReadOnly"]) == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["IsReadOnly"]);
                    pfiled.ShouldRetainImageSize = dr["ShouldRetainImageSize"].ToString();
                    pfiled.Pattern = dr["Pattern"].ToString();
                    pfiled.MaxLength = (dr["MaxLength"]) == DBNull.Value ? (int?)null : Convert.ToInt32(dr["MaxLength"]);
                    pfiled.ShowTextOnRight = dr["ShowTextOnRight"].ToString();
                    pfiled.Lower = dr["Lower"].ToString();
                    pfiled.Upper = dr["Upper"].ToString();
                    pfiled.RelateCondition = dr["RelateCondition"].ToString();
                    pfiled.ShouldReturnToParent = dr["ShouldReturnToParent"].ToString();
                    pfiled.RelatedViewId = dr["RelatedViewId"].ToString();
                    pfiled.SourceTableName = dr["SourceTableName"].ToString();
                    pfiled.CodeColumnName = dr["CodeColumnName"].ToString();
                    pfiled.TextColumnName = dr["TextColumnName"].ToString();
                    pfiled.BackgroundColor = dr["BackgroundColor"].ToString();
                    pfiled.IsExclusiveTable = dr["IsExclusiveTable"].ToString();
                    pfiled.TabIndex = Convert.ToInt32(dr["TabIndex"]);
                    pfiled.HasTabStop = dr["HasTabStop"].ToString();
                    pfiled.SourceFieldId = dr["SourceFieldId"].ToString();
                    pfiled.ChoicesList = dr["List"].ToString();
                    pfiled.BackgroundColor = dr["BackgroundColor"].ToString();
                    pfileds.Add(pfiled);
                }

                connection.Close();
            }
            return pfileds;

            //return await Task.FromResult(pfileds);
        }

        public static string GetFieldsOnPageAsDataTable(int pageId)
        {
            string query = "select F.[Name], F.[PageId], F.[FieldId], F.[UniqueId], F.[FieldTypeId], F.[CheckCodeAfter] As ControlAfterCheckCode, F.[CheckCodeBefore] As ControlBeforeCheckCode,  " +
                "P.[Name] AS PageName, P.[CheckCodeBefore] As PageBeforeCheckCode, P.[CheckCodeAfter] As PageAfterCheckCode, P.[Position], " +
                "F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage], F.[ControlHeightPercentage], F.[ControlWidthPercentage] , F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[ControlScriptName], " +
                "F.[PromptTopPositionPercentage], F.[PromptLeftPositionPercentage], F.[PromptText], F.[PromptFontFamily], F.[PromptFontSize], F.[PromptFontStyle], F.[ControlFontFamily], F.[ControlFontSize], F.[ControlFontStyle], F.[PromptScriptName], " +
                "F.[ShouldRepeatLast], F.[IsRequired], F.[IsReadOnly],  " +
                "F.[ShouldRetainImageSize], F.[Pattern], F.[MaxLength], F.[ShowTextOnRight], " +
                "F.[Lower], F.[Upper], F.[RelateCondition], F.[ShouldReturnToParent], F.[RelatedViewId], F.[List]," +
                "F.[SourceTableName], F.[CodeColumnName], F.[TextColumnName],  F.[BackgroundColor], " +
                "F.[Sort], F.[IsExclusiveTable], F.[TabIndex], F.[HasTabStop],F.[SourceFieldId] " +
                "from ((metaFields F " +
                "LEFT JOIN metaPages P on P.[PageId] = F.[PageId]) " +
                "LEFT JOIN metaViews V on V.[ViewId] = P.[ViewId]) " +
                "where F.[PageId] =" + pageId + " order by F.[ControlTopPositionPercentage], F.[ControlLeftPositionPercentage]";

            return query;
        }
    }

    public class CDTProject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PageName { get; set; }
        public string PageId { get; set; }
        public string FieldId { get; set; }
        public string UniqueId { get; set; }
        public int? FieldTypeId { get; set; }
        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string Name { get; set; }
        public string PageBeforeCheckCode { get; set; }
        public string PageAfterCheckCode { get; set; }
        public string Position { get; set; }
        public double? ControlTopPositionPercentage { get; set; }
        public double? ControlLeftPositionPercentage { get; set; }
        public double? ControlHeightPercentage { get; set; }
        public double? ControlWidthPercentage { get; set; }
        public string ControlFontFamily { get; set; }
        public double? ControlFontSize { get; set; }
        public string ControlFontStyle { get; set; }
        public string ControlScriptName { get; set; }
        public double? PromptTopPositionPercentage { get; set; }
        public double? PromptLeftPositionPercentage { get; set; }
        public string PromptText { get; set; }
        public double? PromptFontSize { get; set; }
        public string PromptFontFamily { get; set; }
        public string PromptFontStyle { get; set; }
        //publi string ControlFontFamily{get;set;}
        // public string ControlFontSize { get; set; } 
        public string PromptScriptName { get; set; }
        public string ShouldRepeatLast { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsReadOnly { get; set; }
        public string ShouldRetainImageSize { get; set; }
        public string Pattern { get; set; }
        public int? MaxLength { get; set; }
        public string ShowTextOnRight { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }
        public string RelateCondition { get; set; }
        public string ShouldReturnToParent { get; set; }
        public string RelatedViewId { get; set; }
        public string SourceTableName { get; set; }
        public string CodeColumnName { get; set; }
        public string TextColumnName { get; set; }
        public string BackgroundColor { get; set; }
        public string IsExclusiveTable { get; set; }
        public int? TabIndex { get; set; }
        public string HasTabStop { get; set; }
        public string ChoicesList { get; set; }
        public string SourceFieldId { get; set; }
        public List<string> SourceTableValues { get; set; }
    }

}
