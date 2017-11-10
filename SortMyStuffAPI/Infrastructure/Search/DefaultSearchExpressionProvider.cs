using SortMyStuffAPI.Utils;
using System;
using System.Linq.Expressions;

namespace SortMyStuffAPI.Infrastructure
{
    public class DefaultSearchExpressionProvider : ISerachExpressionProvider
    {
        public virtual Expression GetComparison(MemberExpression left, string op, ConstantExpression right)
        {
            if (!op.Equals(ApiStrings.PARAMETER_OP_EQUAL, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid operator '{op}'.");

            return Expression.Equal(left, right);
        }


        public virtual ConstantExpression GetValue(string input)
            => Expression.Constant(input);
    }
}
