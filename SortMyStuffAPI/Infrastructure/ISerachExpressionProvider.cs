using System.Linq.Expressions;

namespace SortMyStuffAPI.Infrastructure
{
    public interface ISerachExpressionProvider
    {
        ConstantExpression GetValue(string input);

        Expression GetComparison(MemberExpression left, string op, ConstantExpression right);
    }
}
