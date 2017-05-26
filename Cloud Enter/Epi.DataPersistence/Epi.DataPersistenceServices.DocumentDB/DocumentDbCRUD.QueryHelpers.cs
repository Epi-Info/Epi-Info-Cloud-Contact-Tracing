using System.Collections.Generic;
using System.Linq;
using Epi.FormMetadata.DataStructures;
using Epi.DataPersistence.Constants;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class DocumentDbCRUD
    {
        private const string LT = "<";
        private const string LE = "<=";
        private const string EQ = "=";
        private const string NE = "!=";
        private const string GT = ">";
        private const string GE = ">=";
        private const string SELECT = "SELECT ";
        private const string ORDERBY = " ORDER BY ";
        private const string DESC = " DESC ";
        private const string FROM = " FROM ";
        private const string WHERE = " WHERE ";
        private const string AND = " AND ";
        private const string OR = " OR ";
        private const string Alias = "`@`";

        private const string FRP_ = "FormResponseProperties.";
        private const string FRP_RecStatus = FRP_ + "RecStatus";
        private const string FRP_ResponseQA_ = FRP_ + "ResponseQA.";

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

        private string AssembleParentQASelect(string formName, List<string> columnNames)
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

        private string GetAllRecordByFormId(string collectionAlias, string formId, List<string> formPoperties, List<string> columnlist)
        {
            string SelectColumnList = string.Empty;

            //var SelectFormPoperties = AssembleParentSelect(collectionAlias, formPoperties.Select(g =>"."+FRP + g).ToArray());
            var SelectFormPoperties = AssembleSelect(collectionAlias, formPoperties.Select(g => FRP_ + g).ToArray());

            if (columnlist != null && columnlist.Count > 0)
            {
                // convert column list to this format {patientname1: Zika.FormResponseProperties.ResponseQA.patientname1} as ResponseQA
                SelectColumnList = AssembleParentQASelect(collectionAlias, columnlist);
            }

            var query = SELECT
                           + SelectFormPoperties + ","
                           + AssembleSelect(collectionAlias, "_ts,")
                           + SelectColumnList
                           + WHERE
                           + AssembleWhere(collectionAlias, Expression(FRP_ + "FormId", EQ, formId)
                           + And_Expression(FRP_RecStatus, NE, RecordStatus.Deleted))
                           + ORDERBY
                           + AssembleSelect(collectionAlias, "_ts")
                           + DESC;

            return query;
        }

        private string SearchByFiledNames(string collectionAlias, string formId, List<string> formPoperties, KeyValuePair<FieldDigest, string>[] columnlist)
        {
            string SelectColumnList = string.Empty;

            var SelectFormPoperties = AssembleSelect(collectionAlias, formPoperties.Select(g => FRP_ + g).ToArray());
            if (columnlist != null)
            {
                // convert column list to this format ex:{patientname1: Zika.FormResponseProperties.ResponseQA.patientname1} as ResponseQA
                SelectColumnList = AssembleParentQASelect(collectionAlias, columnlist.Select(x => x.Key.FieldName).ToList());
            }

            
            var fieldKeyList = columnlist.Select(x=> collectionAlias + "." + FRP_ResponseQA_ + x.Key.FieldName +EQ +'"'+x.Value+'"' + AND).ToArray();

            var expersion = collectionAlias +"."+ FRP_ + "FormId" + EQ + "\"" + formId + "\"" + AND + collectionAlias + "." + FRP_RecStatus + NE + RecordStatus.Deleted;
            var query = SELECT
                           + SelectFormPoperties + ","
                           + AssembleSelect(collectionAlias, "_ts,")
                           + SelectColumnList
                           + WHERE
                           + AssembleWhere(collectionAlias, fieldKeyList)
                           + expersion
                           + ORDERBY
                           + AssembleSelect(collectionAlias, "_ts")
                           + DESC;
            return query;
        }


    }
}
