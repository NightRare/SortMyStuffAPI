using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SortMyStuffAPI.Infrastructure
{
    public class SortOptionsProcessor<T, TEntity>
    {
        private readonly string[] _orderBy;

        public SortOptionsProcessor(string[] orderBy)
        {
            _orderBy = orderBy;
        }

        public IEnumerable<SortTerm> GetAllTerms()
        {
            if (_orderBy == null) yield break;

            foreach (var term in _orderBy)
            {
                if (string.IsNullOrEmpty(term)) continue;

                var tokens = term.Split(' ');

                if(tokens.Length == 0)
                {
                    yield return new SortTerm {
                        Name = term,
                        Descending = false
                    };
                    continue;
                }

                // e.g. orderBy=XXX desc
                var descending = tokens.Length > 1 && 
                    tokens[1].Equals(ApiStrings.ParameterDesc, StringComparison.OrdinalIgnoreCase);

                yield return new SortTerm
                {
                    Name = tokens[0],
                    Descending = descending
                };
            }
        }

        public IEnumerable<SortTerm> GetValidTerms()
        {
            var allTerms = GetAllTerms().ToArray();
            if (!allTerms.Any()) yield break;

            var sortableTerms = GetSortableTermsFromModel();

            foreach (var term in allTerms)
            {
                var sortableTerm = sortableTerms
                    .SingleOrDefault(x => x.Name.Equals(term.Name, StringComparison.OrdinalIgnoreCase));
                if (sortableTerm == null) continue;

                yield return new SortTerm
                {
                    Name = sortableTerm.Name,
                    Descending = term.Descending,
                    Default = sortableTerm.Default
                };
            }
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            var terms = GetValidTerms().ToArray();
            if (!terms.Any())
            {
                terms = GetSortableTermsFromModel().Where(t => t.Default).ToArray();
            }

            if (!terms.Any()) return query;

            var modifiedQuery = query;
            var useThenBy = false;

            foreach (var term in terms)
            {
                var propertyInfo = ExpressionHelper
                    .GetPropertyInfo<TEntity>(term.Name);
                var obj = ExpressionHelper.Parameter<TEntity>();

                // Build the LINQ expression backwards:
                // query = query.OrderBy(x => x.Property);

                // x => x.Property
                var key = ExpressionHelper
                    .GetPropertyExpression(obj, propertyInfo);
                var keySelector = ExpressionHelper
                    .GetLambda(typeof(TEntity), propertyInfo.PropertyType, obj, key);

                // query.OrderBy/ThenBy[Descending](x => x.Property)
                modifiedQuery = ExpressionHelper
                    .CallOrderByOrThenBy(
                        modifiedQuery, useThenBy, term.Descending, propertyInfo.PropertyType, keySelector);

                useThenBy = true;
            }

            return modifiedQuery;
        }

        private static IEnumerable<SortTerm> GetSortableTermsFromModel()
            => typeof(T)
            .GetTypeInfo()
            .DeclaredProperties
            .Where(p => p.GetCustomAttributes<SortableAttribute>().Any())
            .Select(p => new SortTerm {
                Name = p.Name,
                Default = p.GetCustomAttribute<SortableAttribute>().Default
            });
    }
}
