using System.Collections.Generic;
using System.Linq;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.Constants;
using Epi.FormMetadata.DataStructures;

namespace Epi.DataPersistenceServices.CosmosDB
{
    public partial class CosmosDBCRUD
    {
        private const string LT = "<";
        private const string LE = "<=";
        private const string EQ = "=";
        private const string NE = "!=";
        private const string GT = ">";
        private const string GE = ">=";
        private const string SELECT = "SELECT ";
        private const string ORDER_BY = " ORDER BY ";
        private const string DESC = " DESC ";
        private const string FROM = " FROM ";
        private const string WHERE = " WHERE ";
        private const string AND = " AND ";
        private const string OR = " OR ";
        private const string Alias = "`@`";

        private const string FRP_ = "FormResponseProperties.";
        private const string FRP_RecStatus = FRP_ + "RecStatus";
        private const string FRP_ResponseQA_ = FRP_ + "ResponseQA.";
        private const string FRP_UserOrgId = FRP_ + "UserOrgId";
        private const string FRP_FirstSaveTime = FRP_ + "FirstSaveTime";

        private const string udf_wildCardCompare = "udf.WildCardCompare";
        private const string udf_sharingRules = "udf.SharingRules";

        private string AssembleSelect(string collectionName, params string[] columnNames)
        {
            string columnList;
            if (columnNames.Length == 1 && columnNames[0] == "*")
            {
                columnList = "*";
            }
            else
            {
                columnList = collectionName + '.' + string.Join(", " + collectionName + '.', columnNames);
            }
            return columnList;
        }

        private string AssembleParentSelect(string collectionName, params string[] columnNames)
        {
            string columnList;
            if (columnNames.Length == 1 && columnNames[0] == "*")
            {
                columnList = "*";
            }
            else
            {
                columnList = collectionName + string.Join("," + collectionName, columnNames);
            }
            return columnList;
        }

        private string AssembleParentQASelect(string formName, string[] columnNames)
        {
            string query = "{"
                            + AssembleParentSelect(null, columnNames.Select(g => g.ToLower() + ":" + formName + "." + FRP_ResponseQA_ + g.ToLower()).ToArray())
                            + "} AS ResponseQA "
                            + FROM + formName;
            return query;
        }

        private string AssembleWhere(string collectionName, params string[] expressions)
        {
            string where;
            where = string.Join(" ", expressions.Select(e => e.ToString().Replace(Alias, collectionName)));
            return where;
        }

        private static string Expression(string left, string relational_operator, object right, string and_or = null)
        {
            string expression;

            if (right == null)
            {
                expression = string.Format("{0}.{1} {2} null", Alias, left, relational_operator);
            }
            else
            {
                if (right is string == false)
                    expression = string.Format("{0}.{1} {2} {3}", Alias, left, relational_operator, right.ToString());
                else
                    expression = string.Format("{0}.{1} {2} {3}", Alias, left, relational_operator, "'" + right.ToString() + "'");
            }

            return and_or == null ? expression : and_or + expression;
        }

        private static string ExpressionWithFunction(string left, string relational_operator, object right, string function)
        {
            string expression = string.Format("{0}({1}.{2}) {3} {4}", function, Alias, left, relational_operator, "'" + right.ToString() + "'");
            return expression;
        }

        private static string And_Expression(string left, string relational_operator, object right, bool excludeExpression = false)
        {
            var expression = excludeExpression ? string.Empty : Expression(left, relational_operator, right, AND);
            return expression;
        }

        private static string And_Expression(string left, string relational_operator, object right, string function)
        {
            var expression = function == null
                ? Expression(left, relational_operator, right)
                : ExpressionWithFunction(left, relational_operator, right, function);
            return AND + expression;
        }

        private static string Or_Expression(string left, string relational_operator, object right, string function)
        {
            var expression = function == null
                ? Expression(left, relational_operator, right)
                : ExpressionWithFunction(left, relational_operator, right, function);
            return OR + expression;
        }

        private static string Or_Expression(string left, string relational_operator, object right, bool excludeExpression = false)
        {
            var expression = excludeExpression ? string.Empty : Expression(left, relational_operator, right, OR);
            return expression;
        }

        private static string And_SearchExpressions(KeyValuePair<FieldDigest, string>[] searchQualifiers)
        {
            string searchExpression = string.Empty;
            if (searchQualifiers == null || searchQualifiers.Length == 0) return string.Empty;
            foreach (var searchQualifier in searchQualifiers)
            {
                if (searchQualifier.Value.Contains('*') || searchQualifier.Value.Contains('?'))
                {
                    var expression = And_Expression(FRP_ResponseQA_ + searchQualifier.Key.FieldName, EQ, searchQualifier.Value.ToLowerInvariant(), "LOWER");
                    searchExpression += expression;
                }
            }
            return searchExpression;
        }

        private static string LOWER(string argument)
        {
            return string.Format("LOWER({0})", argument);
        }

        private string GenerateResponseGridQuery(string collectionAlias, string formId, List<string> formPoperties, 
            string[] columnlist, KeyValuePair<FieldDigest, string>[] searchQualifiers,
            ResponseAccessRuleContext responseAccessRuleContext, string querySetToken)
        {
            string SelectColumnList = string.Empty;

            var SelectFormPoperties = AssembleSelect(collectionAlias, formPoperties.Select(g => FRP_ + g).ToArray());

            if (columnlist != null && columnlist.Length > 0)
            {
                // convert column list to this format {patientname1: Zika.FormResponseProperties.ResponseQA.patientname1} as ResponseQA
                SelectColumnList = AssembleParentQASelect(collectionAlias, columnlist);
            }

            if (searchQualifiers != null && searchQualifiers.Length > 0)
            {
                var searchQualifierList = searchQualifiers.Select(x => AssembleSearchQuailifier(collectionAlias, x.Key.FieldName, x.Value) + AND).ToArray();

                var expression = collectionAlias + "." + FRP_ + "FormId" + EQ + "\"" + formId + "\"" + AND + collectionAlias + "." + FRP_RecStatus + NE + RecordStatus.Deleted;

                if (!string.IsNullOrWhiteSpace(querySetToken)) expression += AND + collectionAlias + "." + FRP_FirstSaveTime + LE + querySetToken;

                var query = SELECT
                               + SelectFormPoperties + ","
                               + AssembleSelect(collectionAlias, "_ts,")
                               + SelectColumnList
                               + WHERE
                               + AssembleAcessRuleQualifier(collectionAlias, responseAccessRuleContext)
                               + AssembleWhere(collectionAlias, searchQualifierList)
                               + expression;
                               //+ ORDER_BY
                               //+ AssembleSelect(collectionAlias, "_ts")
                               //+ DESC;
                return query;
            }
            else
            {
                var query = SELECT
                               + SelectFormPoperties + ","
                               + AssembleSelect(collectionAlias, "_ts,")
                               + SelectColumnList
                               + WHERE
                               + AssembleAcessRuleQualifier(collectionAlias, responseAccessRuleContext)
                               + AssembleWhere(collectionAlias, Expression(FRP_ + "FormId", EQ, formId)
                               + And_Expression(FRP_RecStatus, NE, RecordStatus.Deleted)
                               + (!string.IsNullOrWhiteSpace(querySetToken) ? And_Expression(FRP_FirstSaveTime, LE, querySetToken) : string.Empty));
                               //+ ORDER_BY
                               //+ AssembleSelect(collectionAlias, "_ts")
                               //+ DESC;

                return query;
            }
        }

        private string AssembleSearchQuailifier(string collectionAlias, string fieldName, string fieldValue)
        {
            string qualifier;
            if (fieldValue.Contains('*') || fieldValue.Contains('?'))
            {
                qualifier = udf_wildCardCompare + "(" + collectionAlias + "." + FRP_ResponseQA_ + fieldName + "," + "\"" + fieldValue + "\"" + ")";
            }
            else
            {
                qualifier = collectionAlias + "." + FRP_ResponseQA_ + fieldName + EQ + '"' + fieldValue + '"';
            }
            return qualifier;
        }

        private string AssembleAcessRuleQualifier(string collectionAlias, ResponseAccessRuleContext ruleContext)
        {
            string qualifier = string.Empty;
            if (ruleContext != null && ruleContext.IsSharable)
            {
                var parameters = new string[]
                {
                ruleContext.RuleId.ToString(),
                ruleContext.IsHostOrganizationUser.ToString().ToLower(),
                ruleContext.UserOrganizationId.ToString(),
                collectionAlias + "." + FRP_UserOrgId
                };

                qualifier = udf_sharingRules + "(" + string.Join(",", parameters) + ")" + AND;
            }

            return qualifier;
        }
    }
}
