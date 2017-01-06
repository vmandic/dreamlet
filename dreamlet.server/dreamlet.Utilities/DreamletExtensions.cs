using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.Utilities
{
	public static class DreamletExtensions
	{
	}

	public static class ExpressionExtensions
	{
		public static string GetPropertyName(this LambdaExpression expression)
		{
			var body = (MemberExpression)expression.Body;
			return body.Member.Name;
		}

		public static string GetPropertyName<T>(this Expression<Func<T, object>> expression)
		{
			string propertyName;
			Expression bodyExpression = expression.Body;

			if (bodyExpression.NodeType == ExpressionType.Convert && bodyExpression is UnaryExpression)
			{
				Expression operand = ((UnaryExpression)expression.Body).Operand;
				propertyName = ((MemberExpression)operand).Member.Name;
			}
			else
			{
				var body = (MemberExpression)expression.Body;
				propertyName = body.Member.Name;
			}

			return propertyName;
		}
	}
}
