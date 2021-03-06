﻿using SortMyStuffAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class AssetEntity : IEntity
    {
        public string Id { get; set; }

        [Required]
        [Mutable]
        public string Name { get; set; }

        // TODO: make CategoryId changeable
        public string CategoryId { get; set; }

        [Mutable]
        [Required]
        public string ContainerId { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        [Mutable]
        public DateTimeOffset ModifyTimestamp { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

        public virtual ICollection<DetailEntity> Details { get; set; }
    }
}