using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace SortMyStuffAPI.Models
{
    public class Asset : Resource
    {
        public string Name { get; set; }

        public string ContainerId { get; set; }

        public string Category { get; set; }

//        public IList<string> DetailsList { get; set; }

        public long CreateTimestamp { get; set; }

        public long ModifyTimestamp { get; set; }

        public string ThumbnailUrl { get; set; }

    }
}
