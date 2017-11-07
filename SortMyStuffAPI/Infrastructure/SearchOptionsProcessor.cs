using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Infrastructure
{
    public class SearchOptionsProcessor<T, TEntity>
    {
        private readonly string[] _searchQuery;

        public SearchOptionsProcessor(string[] searchQuery)
        {
            _searchQuery = searchQuery;
        }

        public IEnumerable<SearchTerm> GetAllTerms()
        {
            if (_searchQuery == null) yield break;

            // Each search expression looks like:
            // "fieldName operator value..."
            foreach (var expression in _searchQuery)
            {
                if (string.IsNullOrEmpty(expression)) continue;

                var tokens = expression.Split(' ');

                if (tokens.Length == 0)
                {
                    yield return new SearchTerm
                    {
                        ValidSyntax = false,
                        Name = expression
                    };
                    continue;
                }

                if (tokens.Length < 3)
                {
                    yield return new SearchTerm
                    {
                        ValidSyntax = false,
                        Name = tokens[0]
                    };
                    continue;
                }

                // valid syntax expression
                yield return new SearchTerm
                {
                    ValidSyntax = true,
                    Name = tokens[0],
                    Operator = tokens[1],
                    Value = string.Join(' ', tokens.Skip(2))
                };
            }
        }

        public IEnumerable<SearchTerm> GetValidTerms()
        {
            var allTerms = GetAllTerms().Where(t => t.ValidSyntax).ToArray();
            if (!allTerms.Any()) yield break;

            var sortableTerms = GetSearchableTermsFromModel();

            foreach (var term in allTerms)
            {
                var searchableTerm = sortableTerms
                    .SingleOrDefault(x => x.Name.Equals(term.Name, StringComparison.OrdinalIgnoreCase));
                if (searchableTerm == null) continue;

                yield return new SearchTerm
                {
                    ValidSyntax = term.ValidSyntax,
                    Name = searchableTerm.Name,
                    Operator = term.Operator,
                    Value = term.Value,
                    ExpressionProvider = searchableTerm.ExpressionProvider
                };
            }
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            var terms = GetValidTerms().ToArray();
            if (!terms.Any()) return query;

            var modifiedQuery = query;

            foreach (var term in terms)
            {
                var propertyInfo = ExpressionHelper
                    .GetPropertyInfo<TEntity>(term.Name);
                var obj = ExpressionHelper.Parameter<TEntity>();

                // Build the LINQ expression backwards:
                // query = query.Where(x => x.Property == "Value");

                // x.Property
                var left = ExpressionHelper.GetPropertyExpression(obj, propertyInfo);
                // "Value"
                var right = term.ExpressionProvider.GetValue(term.Value);

                // x.Property == "Value"
                var comparisonExpression = term.ExpressionProvider.GetComparison(left, term.Operator, right);

                // x => x.Property == "Value"
                var lambdaExpression = ExpressionHelper
                    .GetLambda<TEntity, bool>(obj, comparisonExpression);

                // query = query.Where...
                modifiedQuery = ExpressionHelper.CallWhere(modifiedQuery, lambdaExpression);
            }

            return modifiedQuery;
        }

        private static IEnumerable<SearchTerm> GetSearchableTermsFromModel()
            => typeof(T)
            .GetTypeInfo()
            .DeclaredProperties
            .Where(p => p.GetCustomAttributes<SearchableAttribute>().Any())
            .Select(p => new SearchTerm
            {
                Name = p.Name,
                ExpressionProvider = p.GetCustomAttribute<SearchableAttribute>().ExpressionProvider
            });
    }
}
