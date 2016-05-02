
using Epi.Cloud.MetadataServices.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Epi.Cloud.SqlServer;

namespace Epi.Cloud.DBAccessService.Repository
{
    public class GetmetadataDB
    {
        string connStr = WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;


        //Call the Cloud EF and get the meta data
        public async Task<List<MetadataFieldAttributes>> MetaDataAsync(int? pageid)
        {
            List<MetadataFieldAttributes> pfileds = new List<MetadataFieldAttributes>();
            DataTable dt = new DataTable();

            MetaData metaDt = new MetaData();
            List<MetadataDbFieldAttribute> dataPfieleds = new List<MetadataDbFieldAttribute>();

            //Get the meta data using entity framework passing pageid
            dataPfieleds = metaDt.GetFieldsByPageAsData(pageid);


            foreach (var data in dataPfieleds)
            {
                MetadataFieldAttributes pfiled = new MetadataFieldAttributes();

                pfiled.Name = data.Name;
                pfiled.PageId = data.PageId;
                pfiled.FieldId = data.FieldId;
                pfiled.UniqueId = data.UniqueId;
                pfiled.FieldTypeId = data.FieldTypeId;
                pfiled.ControlAfterCheckCode = data.ControlAfterCheckCode;
                pfiled.ControlBeforeCheckCode = data.ControlBeforeCheckCode;
                pfiled.PageName = data.PageName;
                pfiled.PageBeforeCheckCode = data.PageBeforeCheckCode;
                pfiled.PageAfterCheckCode = data.PageAfterCheckCode;
                pfiled.Position = data.Position;
                pfiled.ControlTopPositionPercentage = data.ControlTopPositionPercentage;
                pfiled.ControlLeftPositionPercentage = data.ControlLeftPositionPercentage;
                pfiled.ControlHeightPercentage = data.ControlHeightPercentage;
                pfiled.ControlWidthPercentage = data.ControlWidthPercentage;
                pfiled.ControlFontFamily = data.ControlFontFamily;
                pfiled.ControlFontSize = data.ControlFontSize;
                pfiled.ControlFontStyle = data.ControlFontStyle;
                pfiled.ControlScriptName = data.ControlScriptName;
                pfiled.PromptTopPositionPercentage = data.PromptTopPositionPercentage;
                pfiled.PromptLeftPositionPercentage = data.PromptLeftPositionPercentage;
                pfiled.PromptText = data.PromptText;
                pfiled.PromptFontSize = data.PromptFontSize;
                pfiled.PromptFontStyle = data.PromptFontStyle;
                pfiled.PromptFontFamily = data.PromptFontFamily;
                pfiled.PromptScriptName = data.PromptScriptName;
                pfiled.ShouldRepeatLast = data.ShouldRepeatLast;
                pfiled.IsRequired = data.IsRequired;
                pfiled.IsReadOnly = data.IsReadOnly;
                pfiled.ShouldRetainImageSize = data.ShouldRetainImageSize;
                pfiled.Pattern = data.Pattern;
                pfiled.MaxLength = data.MaxLength;
                pfiled.ShowTextOnRight = data.ShowTextOnRight;
                pfiled.Lower = data.Lower;
                pfiled.Upper = data.Upper;
                pfiled.RelateCondition = data.RelateCondition;
                pfiled.ShouldReturnToParent = data.ShouldReturnToParent;
                pfiled.RelatedViewId = data.RelatedViewId;
                pfiled.SourceTableName = data.SourceTableName;
                pfiled.CodeColumnName = data.CodeColumnName;
                pfiled.TextColumnName = data.TextColumnName;
                pfiled.BackgroundColor = data.BackgroundColor;
                pfiled.IsExclusiveTable = data.IsExclusiveTable;
                pfiled.TabIndex = data.TabIndex;
                pfiled.HasTabStop = data.HasTabStop;
                pfiled.SourceFieldId = data.SourceFieldId;
                pfiled.RequiredMessage = "This field is required";
                pfileds.Add(pfiled);
            }

            //return pfileds;

            return await Task.FromResult(pfileds);
        }


    }
}