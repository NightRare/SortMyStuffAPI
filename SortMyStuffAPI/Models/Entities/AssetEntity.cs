﻿using System;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AssetEntity
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string ContainerId { get; set; }

        public string Category { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        public DateTimeOffset ModifyTimestamp { get; set; }

    }
}