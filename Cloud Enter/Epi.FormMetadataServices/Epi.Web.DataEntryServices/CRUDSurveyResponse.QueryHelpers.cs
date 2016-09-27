using System.Linq;

namespace Epi.Cloud.DataEntryServices
{
	public partial class CRUDSurveyResponse
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

		private static string Expression(string left, string relational_operator, object right)
		{
			string expression;

			if (right is int)
				expression = string.Format("?.{0} {1} {2}", left, relational_operator, right.ToString());
			else if (right != null)
				expression = string.Format("?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			else
				expression = string.Format("?.{0} {1} null", left, relational_operator);
			return expression;
		}

		private static string And_Expression(string left, string relational_operator, object right, bool skip = false)
		{
			var expression = skip ? string.Empty : string.Format("AND ?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			return expression;
		}

		private static string Or_Expression(string left, string relational_operator, object right, bool skip = false)
		{
			var expression = skip ? string.Empty : string.Format("OR ?.{0} {1} {2}", left, relational_operator, "'" + right.ToString() + "'");
			return expression;
		}
	}
}
