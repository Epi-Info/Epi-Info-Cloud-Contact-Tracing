using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.Form.MetadataServices
{
    public class MetadataProvider
    {
        public List<CDTFieldAttributes> GetMeta(int pageid)
        {
            GetmetadataDB _getmeta = new GetmetadataDB();
            List<CDTProject> projectfields = new List<CDTProject>();
            projectfields = _getmeta.MetaDataAsync(pageid);
            List<CDTFieldAttributes> fieldattributes = new List<CDTFieldAttributes>();
            fieldattributes = _getmeta.GetFieldAttributes(projectfields);
            return fieldattributes;
        }
    }
    public class CDTProject
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public string PageId { get; set; }
        public string FieldId { get; set; }

        public string UniqueId { get; set; }
        public string FieldTypeId { get; set; }
        public string ControlAfterCheckCode { get; set; }
        public string ControlBeforeCheckCode { get; set; }
        public string PageName { get; set; }
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

        public string SourceFieldId { get; set; }
    }


    public class CDTFieldAttributes
    {
        public string RequiredMessage { get; set; }
        public String Name { get; set; }
        public String PromptText { get; set; }
        public int TabIndex { get; set; }
        public double PromptTopPositionPercentage { get; set; }
        public double PromptLeftPositionPercentage { get; set; }
        public double ControlTopPositionPercentage { get; set; }
        public double ControlLeftPositionPercentage { get; set; }

        public double ControlWidthPercentage { get; set; }

        public double ControlHeightPercentage { get; set; }

        public double PromptFontSize { get; set; }
        public String PromptFontStyle { get; set; }
        public String PromptFontFamily { get; set; }

        public double ControlFontSize { get; set; }
        public String ControlFontStyle { get; set; }
        public String ControlFontFamily { get; set; }

        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }

        public bool IsRequired { get; set; }
        public bool Required { get; set; }

        public bool ReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }
    }



    public class GetmetadataDB
    {
        // string connStr = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        //string connStr = "Server=tcp:epiinfocloudserver.database.windows.net,1433;Database=EpiinfoCloudDB;User ID=epiinfoadmin@epiinfocloudserver;Password=Strong5050;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
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
                    pfiled.Name = dr["Name"].ToString();
                    pfiled.PageId = dr["PageId"].ToString();
                    pfiled.FieldId = dr["FieldId"].ToString();
                    pfiled.UniqueId = dr["UniqueId"].ToString();
                    pfiled.FieldTypeId = dr["FieldTypeId"].ToString();
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
                    pfiled.PromptTopPositionPercentage = dr["PromptLeftPositionPercentage"] == DBNull.Value ? (double?)null : Convert.ToDouble(dr["PromptLeftPositionPercentage"]);
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

        public List<CDTFieldAttributes> GetFieldAttributes(List<CDTProject> projectinfo)
        {
            CDTFieldAttributes _field = new CDTFieldAttributes();
            List<CDTFieldAttributes> fieldattributes = new List<CDTFieldAttributes>();
            foreach (var field in projectinfo)
            {
                _field.Name = field.Name;
                _field.RequiredMessage = field.Name;
                _field.PromptText = field.TextColumnName;
                _field.TabIndex = Convert.ToInt32(field.TabIndex);
                _field.PromptTopPositionPercentage = Convert.ToDouble(field.PromptTopPositionPercentage);
                _field.PromptLeftPositionPercentage = Convert.ToDouble(field.PromptLeftPositionPercentage);
                _field.ControlTopPositionPercentage = Convert.ToDouble(field.ControlTopPositionPercentage);
                _field.ControlLeftPositionPercentage = Convert.ToDouble(field.ControlLeftPositionPercentage);
                _field.ControlWidthPercentage = Convert.ToDouble(field.ControlWidthPercentage);
                _field.ControlHeightPercentage = Convert.ToDouble(field.ControlHeightPercentage);
                _field.PromptFontSize = Convert.ToDouble(field.PromptFontSize);
                _field.PromptFontStyle = field.PromptFontStyle;
                _field.PromptFontFamily = field.PromptFontFamily;
                _field.ControlFontSize = Convert.ToDouble(field.ControlFontSize);
                _field.ControlFontStyle = field.ControlFontStyle;
                _field.ControlFontFamily = field.ControlFontFamily;
                _field.MaxLength = Convert.ToInt32(field.MaxLength);
                _field.Pattern = field.Pattern;
                _field.Lower = field.Lower;
                _field.Upper = field.Upper;
                _field.IsRequired = Convert.ToBoolean(field.IsRequired);
                //_field.Required=Convert.ToBoolean(field.r)
                _field.ReadOnly = Convert.ToBoolean(field.IsReadOnly);
                // _field.IsHidden=Convert.ToBoolean(field.)
                //_field.IsHighlighted = Convert.ToBoolean(field.IsHighlighted);
                //_field.IsDisabled=Convert.ToBoolean(field.)
                fieldattributes.Add(_field);
            }
            return fieldattributes;
        }
    }
}

