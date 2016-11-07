using System.Collections.Generic;
using System.Linq;
using Epi.Data.EF;
using System.Data;
using Epi.FormMetadata.DataStructures;
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

        public Epi.FormMetadata.DataStructures.Template GetProjectTemplateMetadata(string projectId)
        {
            var template = new Epi.FormMetadata.DataStructures.Template();

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
                        var projMetaDataInfo = new Epi.FormMetadata.DataStructures.Project();
                        projMetaDataInfo.Id = projElement.ProjectId.ToString();
                        projMetaDataInfo.Name = projElement.ProjectName;
                        projMetaDataInfo.Location = projElement.ProjectLocation;
                        projMetaDataInfo.EpiVersion = projElement.EpiVersion;

                        var result = from mf in metacontext.metaFields
                                     join mp in metacontext.metaPages on (mf.PageId) equals mp.PageId into lmp
                                     from mp in lmp.DefaultIfEmpty()
                                     join mv in metacontext.metaViews on (mp.ViewId) equals mv.ViewId into lmv
                                     from mv in lmv.DefaultIfEmpty()
                                     //join sm in metacontext.SurveyMetaDatas on new Guid(mv.EWEFormId) equals sm.SurveyId into lsm
                                     //from sm in lsm.DefaultIfEmpty()
                                     where mf.PageId != null
                                     select new
                                     {
                                         ViewName = mv.Name,
                                         mv.ViewId,
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
                                         mv.EWEFormId,

                                         DataAccessRuleId = 0,

                                         PageName = mp.Name,
                                         mp.PageId,
                                         mp.Position,
                                         mp.BackgroundId,
                                         PageBeforeCheckCode = mp.CheckCodeBefore,
                                         PageAfterCheckCode = mp.CheckCodeAfter,

                                         mf.UniqueId,
                                         mf.Name,
                                         mf.FieldId,
                                         mf.FieldTypeId,
                                         ControlAfterCheckCode = mf.CheckCodeAfter,
                                         ControlBeforeCheckCode = mf.CheckCodeBefore,
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

                                         //sm.DataAccessRuleId,
                                         //sm.IsDraftMode,
                                         //sm.IsShareable,
                                         //sm.IsSQLProject,
                                         //sm.OrganizationId,
                                         //sm.OrganizationName,
                                         //sm.OwnerId,
                                         //sm.ParentId,
                                         //sm.SurveyName
                                     };


                        var lstViewMetadata = new List<Epi.FormMetadata.DataStructures.View>();

                        var lstGetViews = result.GroupBy(u => u.ViewId).Select(grp => grp.ToList()).ToArray();

                        var viewProperties = lstGetViews.Select(v => v.First());
                        var eweFormIds = viewProperties.Select(vp => new Guid(vp.EWEFormId)).Distinct().ToArray();
                        var surveyMetadataProperties = metacontext.SurveyMetaDatas
                            .Where(x => eweFormIds.Contains(x.SurveyId))
                            .Select(x => new { x.SurveyId,
                                               x.DataAccessRuleId,
                                               x.IsDraftMode,
                                               x.IsShareable,
                                               x.IsSQLProject,
                                               x.OrganizationId,
                                               x.OrganizationName,
                                               x.OwnerId,
                                               x.ParentId,
                                               x.SurveyName}).ToArray();

                        foreach (var viewProp in viewProperties)
                        {
                            var view = new Epi.FormMetadata.DataStructures.View();
                            view.ViewId = viewProp.ViewId;
                            view.Name = viewProp.ViewName;
                            view.IsRelatedView = viewProp.IsRelatedView;
                            view.CheckCode = viewProp.CheckCode;
                            view.CheckCodeBefore = viewProp.CheckCodeBefore;
                            view.CheckCodeAfter = viewProp.CheckCodeAfter;
                            view.RecordCheckCodeBefore = viewProp.RecordCheckCodeBefore;
                            view.RecordCheckCodeAfter = viewProp.RecordCheckCodeAfter;
                            view.CheckCodeVariableDefinitions = viewProp.CheckCodeVariableDefinitions;
                            view.Width = viewProp.Width;
                            view.Height = viewProp.Height;
                            view.Orientation = viewProp.Orientation;
                            view.LabelAlign = viewProp.LabelAlign;
                            view.EIWSOrganizationKey = viewProp.EIWSOrganizationKey;
                            view.EIWSFormId = viewProp.EIWSFormId;
                            view.OrganizationKey = viewProp.EWEOrganizationKey;
                            view.FormId = viewProp.EWEFormId;
                            var eweViewProps = surveyMetadataProperties.Where(x => x.SurveyId == new Guid(viewProp.EWEFormId)).SingleOrDefault();
                            if (eweViewProps != null)
                            {
                                view.FormName = eweViewProps.SurveyName;
                                view.DataAccessRuleId = eweViewProps.DataAccessRuleId.HasValue ? eweViewProps.DataAccessRuleId.Value : 0;
                                view.IsDraftMode = eweViewProps.IsDraftMode;
                                view.IsShareable = eweViewProps.IsShareable.HasValue ? eweViewProps.IsShareable.Value : false;
                                view.OrganizationId = eweViewProps.OrganizationId;
                                view.OrganizationName = eweViewProps.OrganizationName;
                                view.OwnerUserId = eweViewProps.OwnerId;
                                view.ParentFormId = eweViewProps.ParentId.HasValue ? eweViewProps.ParentId.Value.ToString() : null;
                            }

                            lstViewMetadata.Add(view);
                        }

                        projMetaDataInfo.Views = lstViewMetadata.ToArray();
                        var lstGetPages = result.GroupBy(u => new { u.ViewId, u.PageId }).Select(group => group.ToList()).ToList();

                        var sourceTableNames = new List<SourceTableInfo>();
                        var lstPageMetadata = new List<Epi.FormMetadata.DataStructures.Page>();
                        foreach (var element in lstGetPages)
                        {
                            var metapage = new Epi.FormMetadata.DataStructures.Page();
                            foreach (var pageEle in element)
                            {
                                metapage.PageId = pageEle.PageId;
                                metapage.Name = pageEle.PageName;
                                metapage.Position = pageEle.Position;
                                metapage.ViewId = pageEle.ViewId;
                                metapage.BackgroundId = pageEle.BackgroundId;
                                break;
                            }

                            List<Field> lstFieldMetadata = new List<Field>();

                            foreach (var fieldElement in element)
                            {
                                //Populate Fields Data                          
                                Field metaField = new Field();
                                metaField.ViewId = fieldElement.ViewId;
                                metaField.PageId = fieldElement.PageId;
                                metaField.PageName = fieldElement.PageName;
                                metaField.PagePosition = fieldElement.Position;

                                metaField.UniqueId = new System.Guid(fieldElement.UniqueId.ToString());
                                metaField.Name = fieldElement.Name;
                                metaField.FieldId = fieldElement.FieldId;
                                metaField.FieldTypeId = fieldElement.FieldTypeId;
                                metaField.ControlAfterCheckCode = fieldElement.ControlAfterCheckCode;
                                metaField.ControlBeforeCheckCode = fieldElement.ControlBeforeCheckCode;
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
                                .Select(field => new SourceTableInfo(field.SourceTableName, field.TextColumnName)).ToArray());
                        }

                        foreach (var view in projMetaDataInfo.Views)
                        {
                            view.Pages = lstPageMetadata.Where(v => v.ViewId == view.ViewId).ToArray();
                        }

                        sourceTableNames = sourceTableNames.Distinct().ToList();
                        var sourceTables = new List<SourceTable>();
                        foreach (var sourceTableInfo in sourceTableNames)
                        {
                            var sourceTable = new SourceTable { TableName = sourceTableInfo.TableName, Values = PopulateCodeTables(sourceTableInfo.TableName, sourceTableInfo.ColumnName).ToArray() };
                            sourceTables.Add(sourceTable);
                        }
                        template.SourceTables = sourceTables.ToArray();
                        template.Project = projMetaDataInfo;
                    }
                }
                catch (System.Exception ex)
                {

                }

                return template;
            }
        }

        private class SourceTableInfo
        {
            public SourceTableInfo(string tableName, string columnName)
            {
                TableName = tableName;
                ColumnName = columnName;
            }
            public string TableName { get; set; }
            public string ColumnName { get; set; }
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
