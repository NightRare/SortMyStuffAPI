using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace SortMyStuffAPI.Models.Resources
{
    public class Asset : Resource
    {
        public string Name { get; set; }

        public Link Container { get; set; }

        public string Category { get; set; }

        public string CreateTimestamp { get; set; }

        public string ModifyTimestamp { get; set; }

        public Link ThumbnailUrl { get; set; }

//        public IList<string> DetailsList { get; set; }

    }
}
