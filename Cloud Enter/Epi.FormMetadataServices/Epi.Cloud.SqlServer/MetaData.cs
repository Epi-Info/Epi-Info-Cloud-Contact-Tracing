using System.Collections.Generic;
using System.Linq;
using Epi.Data.EF;
using System.Reflection;
using System;
using System.Data;

namespace Epi.Cloud.SqlServer
{
    /// <summary>
    /// Concret SQL Server Database Class
    /// </summary>
    public class MetaData
    {

        public metaFieldType GetMetaDataType(int ProjectId)
        {
            using (EPIInfo7Entities metacontext = new EPIInfo7Entities())
            {
                var metadata = metacontext.metaFieldTypes.Where(s => s.FieldTypeId == ProjectId).FirstOrDefault<metaFieldType>();
                return metadata;
            }
        }

        public List<MetadataDbFieldAttribute> GetFieldsByPageAsData(int? pageId)
        {
            List<MetadataDbFieldAttribute> lstGetFieldsData = new List<MetadataDbFieldAttribute>();

            using (EPIInfo7Entities metacontext = new EPIInfo7Entities())
            {
                try
                {
                    var result = from mf in metacontext.metaFields
                                 join mp in metacontext.metaPages on (mf.PageId) equals mp.PageId into lmp
                                 from mp in lmp.DefaultIfEmpty()
                                 join mv in metacontext.metaViews on (mp.ViewId) equals mv.ViewId into lmv
                                 from mv in lmv.DefaultIfEmpty()                               
                                 where (mf.PageId.HasValue && mf.PageId == pageId )|| (!mf.PageId.HasValue && mf.PageId == pageId)
                                 select new
                                 {
                                     mf.UniqueId,
                                     mf.Name,
                                     mf.PageId,
                                     mf.FieldId,                                    
                                     mf.FieldTypeId,
                                     ControlAfterCheckCode = mf.CheckCodeAfter,
                                     ControlBeforeCheckCode = mf.CheckCodeBefore,
                                     PageName = mp.Name,
                                     PageBeforeCheckCode = mp.CheckCodeBefore,
                                     PageAfterCheckCode = mp.CheckCodeAfter,
                                     mp.Position,
                                     mf.ControlTopPositionPercentage,
                                     mf.ControlLeftPositionPercentage,
                                     mf.ControlHeightPercentage,
                                     mf.ControlWidthPercentage,
                                     mf.ControlFontFamily,
                                     mf.ControlFontSize,
                                     mf.ControlFontStyle,
                                     mf.ControlScriptName,
                                     mf.PromptTopPositionPercentage,
                                     mf.PromptLeftPositionPercentage,
                                     mf.PromptText,
                                     mf.PromptFontFamily,
                                     mf.PromptFontSize,
                                     mf.PromptFontStyle,
                                     mf.PromptScriptName,
                                     mf.ShouldRepeatLast,
                                     mf.IsRequired,
                                     mf.IsReadOnly,
                                     mf.ShouldRetainImageSize,
                                     mf.Pattern,
                                     mf.MaxLength,
                                     mf.ShowTextOnRight,
                                     mf.Lower,
                                     mf.Upper,
                                     mf.RelateCondition,
                                     mf.ShouldReturnToParent,
                                     mf.RelatedViewId,
                                     mf.List,
                                     mf.SourceTableName,
                                     mf.CodeColumnName,
                                     mf.TextColumnName,
                                     mf.BackgroundColor,
                                     mf.Sort,
                                     mf.IsExclusiveTable,
                                     mf.TabIndex,
                                     mf.HasTabStop,
                                     mf.SourceFieldId
                                 };


                    foreach (var element in result)
                    {
                        MetadataDbFieldAttribute GetFieldsData = new MetadataDbFieldAttribute();

                        GetFieldsData.UniqueId = element.UniqueId.HasValue ? element.UniqueId.Value.ToString("D") : string.Empty;
                        GetFieldsData.Name = element.Name;
                        GetFieldsData.PageId = element.PageId;
                        GetFieldsData.FieldId = element.FieldId;
                        GetFieldsData.FieldTypeId = element.FieldTypeId;
                        GetFieldsData.ControlAfterCheckCode = element.ControlAfterCheckCode;
                        GetFieldsData.ControlBeforeCheckCode = element.ControlBeforeCheckCode;
                        GetFieldsData.PageName = element.PageName;
                        GetFieldsData.PageBeforeCheckCode = element.PageBeforeCheckCode;
                        GetFieldsData.PageAfterCheckCode = element.PageAfterCheckCode;
                        GetFieldsData.Position = element.Position;
                        GetFieldsData.ControlTopPositionPercentage = element.ControlTopPositionPercentage;
                        GetFieldsData.ControlLeftPositionPercentage = element.ControlLeftPositionPercentage;
                        GetFieldsData.ControlHeightPercentage = element.ControlHeightPercentage;
                        GetFieldsData.ControlWidthPercentage = element.ControlWidthPercentage;
                        GetFieldsData.ControlFontFamily = element.ControlFontFamily;
                        GetFieldsData.ControlFontSize = element.ControlFontSize;
                        GetFieldsData.ControlFontStyle = element.ControlFontStyle;
                        GetFieldsData.ControlScriptName = element.ControlScriptName;
                        GetFieldsData.PromptTopPositionPercentage = element.PromptTopPositionPercentage;
                        GetFieldsData.PromptLeftPositionPercentage = element.PromptLeftPositionPercentage;
                        GetFieldsData.PromptText = element.PromptText;
                        GetFieldsData.PromptFontFamily = element.PromptFontFamily;
                        GetFieldsData.PromptFontSize = element.PromptFontSize;
                        GetFieldsData.PromptFontStyle = element.PromptFontStyle;
                        GetFieldsData.PromptScriptName = element.PromptScriptName;
                        GetFieldsData.ShouldRepeatLast = element.ShouldRepeatLast;
                        GetFieldsData.IsRequired = element.IsRequired;
                        GetFieldsData.IsReadOnly = element.IsReadOnly;
                        GetFieldsData.ShouldRetainImageSize = element.ShouldRetainImageSize;
                        GetFieldsData.Pattern = element.Pattern;
                        GetFieldsData.MaxLength = element.MaxLength;
                        GetFieldsData.ShowTextOnRight = element.ShowTextOnRight;
                        GetFieldsData.Lower = element.Lower;
                        GetFieldsData.Upper = element.Upper;
                        GetFieldsData.RelateCondition = element.RelateCondition;
                        GetFieldsData.ShouldReturnToParent = element.ShouldReturnToParent;
                        GetFieldsData.RelatedViewId = element.RelatedViewId;
                        GetFieldsData.List = element.List;
                        GetFieldsData.SourceTableName = element.SourceTableName;
                        GetFieldsData.CodeColumnName = element.CodeColumnName;
                        GetFieldsData.TextColumnName = element.TextColumnName;
                        GetFieldsData.BackgroundColor = element.BackgroundColor;
                        GetFieldsData.Sort = element.Sort;
                        GetFieldsData.IsExclusiveTable = element.IsExclusiveTable;
                        GetFieldsData.TabIndex = element.TabIndex;
                        GetFieldsData.HasTabStop = element.HasTabStop;
                        GetFieldsData.SourceFieldId = element.SourceFieldId;
                        lstGetFieldsData.Add(GetFieldsData);
                    }
                }
                catch (System.Exception ex)
                {

                }

                return lstGetFieldsData;


            }
        }


        public DataTable GetDropdownDB(string tableName,string ColumnName)
        {
            SqlDatabase sqldb = new SqlDatabase();
            return sqldb.GetTableData(tableName, ColumnName, string.Empty);            
        }

     

    }
}
