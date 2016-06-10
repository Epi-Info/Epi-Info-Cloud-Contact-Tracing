using System.Collections.Generic;
using System.Linq;
using Epi.Data.EF;
using System.Data;
using Epi.Cloud.Common.Metadata;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Epi.Cloud.Common.Configuration;

namespace Epi.Cloud.SqlServer
{
    // EF follows a Code based Configuration model and will look for a class that
    // derives from DbConfiguration for executing any Connection Resiliency strategies
    public class DbConfigurationWithRetryStrategy : DbConfiguration
    {
        public DbConfigurationWithRetryStrategy()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }

    /// <summary>
    /// Concret SQL Server Database Class
    /// </summary>
    public class MetaData
    {

        public metaFieldType GetMetaDataType(int ProjectId)
        {
            using (EPIInfo7Entities metacontext = new EPIInfo7Entities(ConfigurationHelper.GetEnvironmentResourceKey("EPIInfo7Entities")))
            {
                var metadata = metacontext.metaFieldTypes.Where(s => s.FieldTypeId == ProjectId).FirstOrDefault<metaFieldType>();
                return metadata;
            }
        }

        public ProjectTemplateMetadata GetProjectTemplateMetadata(string projectId)
        {
            ProjectTemplateMetadata templateMetaInfo = new ProjectTemplateMetadata();

            using (EPIInfo7Entities metacontext = new EPIInfo7Entities(ConfigurationHelper.GetEnvironmentResourceKey("EPIInfo7Entities")))
            {
                try
                {

                    var dbInfoResult = from mdb in metacontext.metaDbInfoes
                                       select new
                                       {
                                           mdb.ProjectId,
                                           mdb.ProjectName,
                                           mdb.ProjectLocation,
                                           mdb.EpiVersion
                                       };

                    foreach (var projElement in dbInfoResult)
                    {
                        ProjectMetadata projMetaDataInfo = new ProjectMetadata();
                        projMetaDataInfo.Id = projElement.ProjectId.ToString();
                        projMetaDataInfo.Name = projElement.ProjectName;
                        projMetaDataInfo.Location = projElement.ProjectLocation;
                        projMetaDataInfo.EpiVersion = projElement.EpiVersion;

                        var result = from mf in metacontext.metaFields
                                     join mp in metacontext.metaPages on (mf.PageId) equals mp.PageId into lmp
                                     from mp in lmp.DefaultIfEmpty()
                                     join mv in metacontext.metaViews on (mp.ViewId) equals mv.ViewId into lmv
                                     from mv in lmv.DefaultIfEmpty()
                                     where mf.PageId != null
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
                                         mf.SourceFieldId,
                                         mv.ViewId,
                                         mp.BackgroundId,
                                         ViewName = mv.Name,
                                         mv.IsRelatedView,
                                         mv.CheckCode,
                                         mv.CheckCodeBefore,
                                         mv.CheckCodeAfter,
                                         mv.RecordCheckCodeBefore,
                                         mv.RecordCheckCodeAfter,
                                         mv.CheckCodeVariableDefinitions,
                                         mv.Width,
                                         mv.Height,
                                         mv.Orientation,
                                         mv.LabelAlign,
                                         mv.EIWSOrganizationKey,
                                         mv.EIWSFormId,
                                         mv.EWEOrganizationKey,
                                         mv.EWEFormId
                                     };


                        List<ViewMetadata> lstViewMetadata = new List<ViewMetadata>();

                        var lstGetViews = result.GroupBy(u => u.ViewId).Select(grp => grp.ToList()).ToList();

                        foreach (var element in lstGetViews)
                        {
                            foreach (var viewProp in element)
                            {
                                ViewMetadata viewMetaInfo = new ViewMetadata();
                                viewMetaInfo.ViewId = viewProp.ViewId;
                                viewMetaInfo.Name = viewProp.ViewName;
                                viewMetaInfo.IsRelatedView = viewProp.IsRelatedView;
                                viewMetaInfo.CheckCode = viewProp.CheckCode;
                                viewMetaInfo.CheckCodeBefore = viewProp.CheckCodeBefore;
                                viewMetaInfo.CheckCodeAfter = viewProp.CheckCodeAfter;
                                viewMetaInfo.RecordCheckCodeBefore = viewProp.RecordCheckCodeBefore;
                                viewMetaInfo.RecordCheckCodeAfter = viewProp.RecordCheckCodeAfter;
                                viewMetaInfo.CheckCodeVariableDefinitions = viewProp.CheckCodeVariableDefinitions;
                                viewMetaInfo.Width = viewProp.Width;
                                viewMetaInfo.Height = viewProp.Height;
                                viewMetaInfo.Orientation = viewProp.Orientation;
                                viewMetaInfo.LabelAlign = viewProp.LabelAlign;
                                viewMetaInfo.EIWSOrganizationKey = viewProp.EIWSOrganizationKey;
                                viewMetaInfo.EIWSFormId = viewProp.EIWSFormId;
                                viewMetaInfo.EWEOrganizationKey = viewProp.EWEOrganizationKey;
                                viewMetaInfo.EWEFormId = viewProp.EWEFormId;
                                lstViewMetadata.Add(viewMetaInfo);
                                break;
                            }
                        }

                        projMetaDataInfo.Views = lstViewMetadata.ToArray();
                        var lstGetPages = result.GroupBy(u => u.PageId).Select(grp => grp.ToList()).ToList();

                        var sourceTableNames = new List<Tuple<string, string>>();
                        List<PageMetadata> lstPageMetadata = new List<PageMetadata>();
                        foreach (var element in lstGetPages)
                        {
                            PageMetadata metapage = new PageMetadata();
                            foreach (var pageEle in element)
                            {
                                metapage.PageId = pageEle.PageId;
                                metapage.Name = pageEle.PageName;
                                metapage.Position = pageEle.Position;
                                metapage.ViewId = pageEle.ViewId;
                                metapage.BackgroundId = pageEle.BackgroundId;
                                break;
                            }

                            List<FieldMetdata> lstFieldMetadata = new List<FieldMetdata>();

                            foreach (var fieldElement in element)
                            {
                                //Populate Fields Data                          
                                FieldMetdata metaField = new FieldMetdata();
                                metaField.UniqueId = new System.Guid(fieldElement.UniqueId.ToString());
                                metaField.Name = fieldElement.Name;
                                metaField.PageId = fieldElement.PageId;
                                metaField.FieldId = fieldElement.FieldId;
                                metaField.FieldTypeId = fieldElement.FieldTypeId;
                                metaField.ControlAfterCheckCode = fieldElement.ControlAfterCheckCode;
                                metaField.ControlBeforeCheckCode = fieldElement.ControlBeforeCheckCode;
                                metaField.PageName = fieldElement.PageName;
                                metaField.PageBeforeCheckCode = fieldElement.PageBeforeCheckCode;
                                metaField.PageAfterCheckCode = fieldElement.PageAfterCheckCode;

                                metaField.ControlTopPositionPercentage = fieldElement.ControlTopPositionPercentage;
                                metaField.ControlLeftPositionPercentage = fieldElement.ControlLeftPositionPercentage;
                                metaField.ControlHeightPercentage = fieldElement.ControlHeightPercentage;
                                metaField.ControlWidthPercentage = fieldElement.ControlWidthPercentage;
                                metaField.ControlFontFamily = fieldElement.ControlFontFamily;
                                metaField.ControlFontSize = (double?)fieldElement.ControlFontSize;
                                metaField.ControlFontStyle = fieldElement.ControlFontStyle;
                                metaField.ControlScriptName = fieldElement.ControlScriptName;
                                metaField.PromptTopPositionPercentage = fieldElement.PromptTopPositionPercentage;
                                metaField.PromptLeftPositionPercentage = fieldElement.PromptLeftPositionPercentage;
                                metaField.PromptText = fieldElement.PromptText;
                                metaField.PromptFontFamily = fieldElement.PromptFontFamily;
                                metaField.PromptFontSize = (double?)fieldElement.PromptFontSize;
                                metaField.PromptFontStyle = fieldElement.PromptFontStyle;
                                metaField.PromptScriptName = fieldElement.PromptScriptName;
                                metaField.ShouldRepeatLast = fieldElement.ShouldRepeatLast;
                                metaField.IsRequired = fieldElement.IsRequired;
                                metaField.IsReadOnly = fieldElement.IsReadOnly;
                                metaField.ShouldRetainImageSize = fieldElement.ShouldRetainImageSize;
                                metaField.Pattern = fieldElement.Pattern;
                                metaField.MaxLength = fieldElement.MaxLength;
                                metaField.ShowTextOnRight = fieldElement.ShowTextOnRight;
                                metaField.Lower = fieldElement.Lower;
                                metaField.Upper = fieldElement.Upper;
                                metaField.RelateCondition = fieldElement.RelateCondition;
                                metaField.ShouldReturnToParent = fieldElement.ShouldReturnToParent;
                                metaField.RelatedViewId = fieldElement.RelatedViewId;
                                metaField.List = fieldElement.List;
                                metaField.SourceTableName = fieldElement.SourceTableName;
                                metaField.CodeColumnName = fieldElement.CodeColumnName;
                                metaField.TextColumnName = fieldElement.TextColumnName;
                                metaField.BackgroundColorSpecified = fieldElement.BackgroundColor;
                                metaField.Sort = fieldElement.Sort;
                                metaField.IsExclusiveTable = fieldElement.IsExclusiveTable;
                                metaField.TabIndex = (int)fieldElement.TabIndex;
                                metaField.HasTabStop = fieldElement.HasTabStop;
                                metaField.SourceFieldId = fieldElement.SourceFieldId;

                                lstFieldMetadata.Add(metaField);
                            }

                            metapage.Fields = lstFieldMetadata.ToArray();
                            lstPageMetadata.Add(metapage);
                            sourceTableNames.AddRange(metapage.Fields.Where(fields => !string.IsNullOrWhiteSpace(fields.SourceTableName))
                                .Select(field => new Tuple<string, string>(field.SourceTableName, field.TextColumnName)).ToArray());
                        }
                        projMetaDataInfo.Pages = lstPageMetadata.ToArray();

                        sourceTableNames = sourceTableNames.Distinct().ToList();
                        var sourceTables = new List<SourceTable>();
                        foreach (var sourceTableInfo in sourceTableNames)
                        {
                            var sourceTable = new SourceTable { TableName = sourceTableInfo.Item1, Items = PopulateCodeTables(sourceTableInfo.Item1, sourceTableInfo.Item2).ToArray() };
                            sourceTables.Add(sourceTable);
                        }
                        templateMetaInfo.SourceTables = sourceTables.ToArray();
                        templateMetaInfo.Project = projMetaDataInfo;
                    }
                }
                catch (System.Exception ex)
                {

                }

                return templateMetaInfo;
            }
        }

        public DataTable GetDropdownDB(string tableName, string ColumnName)
        {
            SqlDatabase sqldb = new SqlDatabase();
            return sqldb.GetTableData(tableName, ColumnName, string.Empty);
        }

        public static List<string> PopulateCodeTables(string TableName, string ColumnName)
        {
            if (string.IsNullOrEmpty(TableName) || string.IsNullOrEmpty(ColumnName)) return null;
            List<string> lstSourceTableVal = new List<string>();

            MetaData getDropdownVal = new MetaData();
            DataTable dtDropDownVal = getDropdownVal.GetDropdownDB(TableName, ColumnName);

            foreach (DataRow _SourceTableValue in dtDropDownVal.Rows)
            {
                lstSourceTableVal.Add(_SourceTableValue[ColumnName].ToString());

                //lstSourceTableVal.Append("&#;");
            }
            return lstSourceTableVal;
        }

    }
}
