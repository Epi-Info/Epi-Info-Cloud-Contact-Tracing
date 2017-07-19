using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using Epi.Cloud.Common.Constants;
using Epi.Data;
using Epi.Data.EF;
using Epi.FormMetadata.DataStructures;

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
    public class Metadata
    {
        public Epi.FormMetadata.DataStructures.Template GetProjectTemplateMetadata(string projectId)
        {
            var template = new Epi.FormMetadata.DataStructures.Template();
            using (EPIInfo7Entities metacontext = new EPIInfo7Entities("EPIInfo7Entities"))
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

                        var results = result.ToArray();

                        var lstGetViews = results.GroupBy(u => u.ViewId).Select(grp => grp.ToList()).ToArray();


                        var viewProperties = lstGetViews.Select(v => v.First());
                        var eweFormIds = viewProperties.Where(vp => vp.EWEFormId != null).Select(vp => new Guid(vp.EWEFormId)).Distinct().ToArray();
                        var surveyMetadataProperties = metacontext.SurveyMetaDatas
                            .Where(x => eweFormIds.Contains(x.SurveyId))
                            .Select(x => new
                            {
                                x.SurveyId,
                                x.DataAccessRuleId,
                                x.IsDraftMode,
                                x.IsShareable,
                                x.IsSQLProject,
                                x.OrganizationId,
                                x.OrganizationName,
                                x.OwnerId,
                                x.ParentId,
                                x.SurveyName
                            }).ToArray();

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
                            view.EIWSFormId = viewProp.EIWSFormId != null ? viewProp.EIWSFormId.ToLower() : viewProp.EIWSFormId;
                            view.OrganizationKey = viewProp.EWEOrganizationKey;
                            view.FormId = viewProp.EWEFormId != null ? viewProp.EWEFormId.ToLower() : Guid.Empty.ToString("N");
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
                        var lstGetPages = results.GroupBy(u => new { u.ViewId, u.PageId }).Select(group => group.ToList()).ToList();

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
            return GetTableData(tableName, ColumnName, string.Empty);
        }

        public static List<string> PopulateCodeTables(string TableName, string ColumnName)
        {
            if (string.IsNullOrEmpty(TableName) || string.IsNullOrEmpty(ColumnName)) return null;
            List<string> lstSourceTableVal = new List<string>();

            Metadata getDropdownVal = new Metadata();
            DataTable dtDropDownVal = getDropdownVal.GetDropdownDB(TableName, ColumnName);

            foreach (DataRow _SourceTableValue in dtDropDownVal.Rows)
            {
                lstSourceTableVal.Add(_SourceTableValue[ColumnName].ToString());

                //lstSourceTableVal.Append("&#;");
            }
            return lstSourceTableVal;
        }


        /// <summary>
        ///  Returns contents of a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="sortCriteria">Comma delimited string of column names and asc/DESC order</param>
        /// <returns></returns>
        public DataTable GetTableData(string tableName, string columnNames, string sortCriteria)
        {
            try
            {
                if (string.IsNullOrEmpty(columnNames))
                {
                    columnNames = Epi.StringLiterals.STAR;
                }

                string queryString = "select " + columnNames + " from " + FormatTableName(tableName);

                if (!string.IsNullOrEmpty(sortCriteria))
                {
                    queryString += " order by " + sortCriteria;
                }
                Query query = this.CreateQuery(queryString);
                return Select(query);
            }
            finally
            {
            }
        }


        /// <summary>
        /// Formats a table name to comply with SQL syntax
        /// </summary>
        /// <param name="tableName">The table name to format</param>
        /// <returns>string representing the formatted table name</returns>
        private string FormatTableName(string tableName)
        {
            string formattedTableName = string.Empty;

            if (tableName.Contains("."))
            {
                string[] parts = tableName.Split('.');

                foreach (string part in parts)
                {
                    formattedTableName = formattedTableName + InsertInEscape(part) + ".";
                }

                formattedTableName = formattedTableName.TrimEnd('.');
            }
            else
            {
                formattedTableName = InsertInEscape(tableName);
            }
            return formattedTableName;
        }

        /// <summary>
        /// Inserts the string in escape characters. [] for SQL server and `` for MySQL etc.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string InsertInEscape(string str)
        {
            string newString = string.Empty;
            if (!str.StartsWith(StringLiterals.LEFT_SQUARE_BRACKET))
            {
                newString = StringLiterals.LEFT_SQUARE_BRACKET;
            }
            newString += str;
            if (!str.EndsWith(StringLiterals.RIGHT_SQUARE_BRACKET))
            {
                newString += StringLiterals.RIGHT_SQUARE_BRACKET;
            }
            return newString;
        }

        /// <summary>
        /// Create SQL query object
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns></returns>
        public Query CreateQuery(string sqlStatement)
        {
            return new SqlQuery(sqlStatement);
        }

        /// <summary>
        /// Creates a new connection and executes a select query 
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <returns>Result set</returns>
        public DataTable Select(Query selectQuery)
        {
            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("selectQuery");
            }
            #endregion

            DataTable table = new DataTable();
            return Select(selectQuery, table);
        }

        /// <summary>
        /// Executes a SELECT statement against the database and returns a disconnected data table. NOTE: Use this overload to work with Typed DataSets.
        /// </summary>
        /// <param name="selectQuery">The query to be executed against the database</param>
        /// <param name="dataTable">Table that will contain the result</param>
        /// <returns>A data table object</returns>
        public DataTable Select(Query selectQuery, DataTable dataTable)
        {
            #region Input Validation
            if (selectQuery == null)
            {
                throw new ArgumentNullException("selectQuery");
            }
            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }
            #endregion Input Validation


            IDbConnection connection = GetConnection();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = (SqlCommand)GetCommand(selectQuery.SqlStatement, connection, selectQuery.Parameters);
            adapter.SelectCommand.CommandTimeout = 1500;

            try
            {

                adapter.Fill(dataTable);
                try
                {
                    adapter.FillSchema(dataTable, SchemaType.Source);
                }
                catch (ArgumentException ex)
                {
                    // do nothing
                }
                return dataTable;
            }
            //SqlException being caught to handle denied permissions for SELECT, but other 
            //  exceptions may occur and will need to be handled.  -den4  11/23/2010
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new System.ApplicationException("Error executing select query against the database.", ex);
                /*Epi.Windows.MsgBox.Show("You may not have permission to access the database. \n\n" +
                    ex.Message,
                    "Permission Denied",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return dataTable;*/
            }
            catch (Exception ex)
            {
                throw new System.ApplicationException("Error executing select query against the database.", ex);
            }
        }
        /// <summary>
        /// Gets an connection instance based on current ConnectionString value
        /// </summary>
        /// <returns>Connection instance</returns>
        public IDbConnection GetConnection()
        {
            var connectionString = ConnectionStrings.GetConnectionString(ConnectionStrings.Key.DBConnection);
            return GetNativeConnection(connectionString);
        }

        /// <summary>
        /// Gets a native connection instance from supplied connection string
        /// </summary>
        /// <returns>Native connection object</returns>
        protected virtual SqlConnection GetNativeConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        ///  Gets a new command using an existing connection
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="connection"></param>
        /// <param name="parameters">Parameters for the query to be executed</param>
        /// <returns></returns>
        protected virtual IDbCommand GetCommand(string sqlStatement, IDbConnection connection, List<QueryParameter> parameters)
        {

            #region Input Validation
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentNullException("sqlStatement");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            #endregion

            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (QueryParameter parameter in parameters)
            {
                command.Parameters.Add(this.ConvertToNativeParameter(parameter));
            }

            return command;
        }


        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>A native equivalent of a DbParameter</returns>
        protected virtual SqlParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            if (parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            return new SqlParameter(parameter.ParameterName, CovertToNativeDbType(parameter.DbType), parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value);
        }

        /// <summary>
        /// Gets the SQL version of a generic DbType
        /// </summary>
        /// <returns>SQL version of the generic DbType</returns>
        private SqlDbType CovertToNativeDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return SqlDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return SqlDbType.Char;
                case DbType.Binary:
                    return SqlDbType.Binary;
                case DbType.Boolean:
                    return SqlDbType.Bit;
                case DbType.Byte:
                    return SqlDbType.TinyInt;
                case DbType.Currency:
                    return SqlDbType.Money;
                case DbType.Date:
                    return SqlDbType.DateTime;
                case DbType.DateTime:
                    return SqlDbType.DateTime;
                case DbType.DateTime2:
                    return SqlDbType.DateTime2;
                case DbType.DateTimeOffset:
                    return SqlDbType.DateTimeOffset;
                case DbType.Decimal:
                    return SqlDbType.Decimal;
                case DbType.Double:
                    return SqlDbType.Float;
                case DbType.Guid:
                    return SqlDbType.UniqueIdentifier;
                case DbType.Int16:
                    return SqlDbType.SmallInt;
                case DbType.Int32:
                    return SqlDbType.Int;
                case DbType.Int64:
                    return SqlDbType.BigInt;
                case DbType.Object:
                    return SqlDbType.Binary;
                case DbType.SByte:
                    return SqlDbType.TinyInt;
                case DbType.Single:
                    return SqlDbType.Real;
                case DbType.String:
                    return SqlDbType.NVarChar;
                case DbType.StringFixedLength:
                    return SqlDbType.NChar;
                case DbType.Time:
                    return SqlDbType.DateTime;
                case DbType.UInt16:
                    return SqlDbType.SmallInt;
                case DbType.UInt32:
                    return SqlDbType.Int;
                case DbType.UInt64:
                    return SqlDbType.BigInt;
                case DbType.VarNumeric:
                    return SqlDbType.Decimal;
                default:
                    return SqlDbType.VarChar;
            }
        }
    }
}
