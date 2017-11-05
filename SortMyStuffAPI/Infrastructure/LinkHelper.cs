using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Infrastructure
{
    public class LinkHelper
    {
        private readonly IUrlHelper _urlHelper;

        public LinkHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public Link ToAbsolute(Link original)
        {
            if (original == null) return null;

            return new Link
            {
                Href = _urlHelper.Link(original.RouteName, original.RouteValues),
                Method = original.Method,
                Relations = original.Relations
            };
        }
    }
}
