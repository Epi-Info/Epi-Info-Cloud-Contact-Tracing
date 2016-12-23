using System.Linq;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD
    {
        private const string LT = "<";
        private const string LE = "<=";
        private const string EQ = "=";
        private const string NE = "!=";
        private const string GT = ">";
        private const string GE = ">=";
        private const string SELECT = "SELECT ";
        private const string FROM = " FROM ";
        private const string WHERE = " WHERE ";
        private const string AND = " AND ";
        private const string OR = " OR ";

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

        private string AssembleWhere(string collectionName, params string[] expressions)
        {
            string where;
            where = string.Join(" ", expressions.Select(e => e.ToString().Replace("?", collectionName)));
            return where;
        }

        private static string Expression(string left, string relational_operator, object right, string and_or = null)
        {
            string expression;

            if (right == null)
            {
                expression = string.Format("?.{0} {1} null", left, relational_operator);
            }
            else
            {
                if (right is string == false)
                    expression = string.Format("?.{0} {1} {2}", left, relational_operator, right.ToString());
                else
                    expression = string.Format("?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
            }

			return and_or == null ? expression : and_or + expression;
		}

		private static string And_Expression(string left, string relational_operator, object right, bool excludeExpression = false)
		{
            var expression = excludeExpression ? string.Empty : Expression(left, relational_operator, right, AND);
			return expression;
		}

		private static string Or_Expression(string left, string relational_operator, object right, bool excludeExpression = false)
		{
			var expression = excludeExpression ? string.Empty : Expression(left, relational_operator, right, OR);
            return expression;
		}
	}
}
