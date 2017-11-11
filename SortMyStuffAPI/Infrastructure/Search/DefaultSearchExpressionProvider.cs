using SortMyStuffAPI.Utils;
using System;
using System.Linq.Expressions;
using SortMyStuffAPI.Exceptions;

namespace SortMyStuffAPI.Infrastructure
{
    public class DefaultSearchExpressionProvider : ISerachExpressionProvider
    {
        public virtual Expression GetComparison(MemberExpression left, string op, ConstantExpression right)
        {
            if (!op.Equals(ApiStrings.ParameterOpEqual, StringComparison.OrdinalIgnoreCase))
                throw new InvalidSearchOperationException($"Invalid operator '{op}'.");

            return Expression.Equal(left, right);
        }


        public virtual ConstantExpression GetValue(string input)
            => Expression.Constant(input);
    }
}
