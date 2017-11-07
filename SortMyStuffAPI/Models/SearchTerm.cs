using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class SearchTerm
    {
        public string Name { get; set; }

        public string Operator { get; set; }

        public string Value { get; set; }

        public bool ValidSyntax { get; set; }

        public ISerachExpressionProvider ExpressionProvider { get; set; }
    }
}
