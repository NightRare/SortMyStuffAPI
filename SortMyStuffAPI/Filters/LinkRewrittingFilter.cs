﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Filters
{
    public class LinkRewrittingFilter : IAsyncResultFilter
    {
        private readonly IUrlHelperFactory _factory;

        public LinkRewrittingFilter(IUrlHelperFactory factory)
        {
            _factory = factory;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var objectResult = context.Result as ObjectResult;
            bool shouldSkip = objectResult?.Value == null || ShouldSkipStatusCodes(objectResult?.StatusCode);
            if (shouldSkip)
            {
                await next();
                return;
            }

            var helper = new LinkHelper(_factory.GetUrlHelper(context));
            RewriteAllLinks(objectResult.Value, helper);

            await next();
        }

        private static void RewriteAllLinks(object model, LinkHelper helper)
        {
            if (model == null) return;
            if (model.GetType() == typeof(Link))
            {
                ((Link) model).Href = helper.ToAbsolute((Link) model).Href;
                return;
            }

            var allProperties = model
                .GetType().GetTypeInfo()
                .GetAllProperties()
                .Where(p => p.CanRead)
                .ToArray();

            var linkProperties = allProperties
                .Where(p => p.CanWrite && p.PropertyType == typeof(Link));

            foreach (var linkProperty in linkProperties)
            {
                var link = linkProperty.GetValue(model);
                var rewritten = helper.ToAbsolute(link as Link);
                if (rewritten == null) continue;

                linkProperty.SetValue(model, rewritten);

                // Transfer the property values of the hidden Self property to Href, Method and Relations properties
                if (linkProperty.Name == nameof(Resource.Self))
                {
                    allProperties.SingleOrDefault(p => p.Name == nameof(Resource.Href))
                        ?.SetValue(model, rewritten.Href);
                    allProperties.SingleOrDefault(p => p.Name == nameof(Resource.Method))
                        ?.SetValue(model, rewritten.Method);
                    allProperties.SingleOrDefault(p => p.Name == nameof(Resource.Relations))
                        ?.SetValue(model, rewritten.Relations);
                }
            }

            var arrayProperties = allProperties.Where(p => p.PropertyType.IsArray);
            RewriteLinksInArrays(arrayProperties, model, helper);

            var objectProperties = allProperties.Except(linkProperties).Except(arrayProperties);
            RewriteLinksInNestedObjects(objectProperties, model, helper);
        }

        private static void RewriteLinksInNestedObjects(
            IEnumerable<PropertyInfo> objectProperties,
            object obj,
            LinkHelper helper)
        {
            // if the obj itself is not a Resource than skip it
            if(!(obj is Resource)) return;

            foreach (var objectProperty in objectProperties)
            {
                var propType = objectProperty.PropertyType;
                if (propType.Equals(typeof(string)))
                {
                    continue;
                }

                var typeInfo = objectProperty.PropertyType.GetTypeInfo();
                if (typeInfo.IsClass)
                {
                    RewriteAllLinks(objectProperty.GetValue(obj), helper);
                }
            }
        }

        private static void RewriteLinksInArrays(
            IEnumerable<PropertyInfo> arrayProperties,
            object obj,
            LinkHelper helper)
        {
            foreach (var arrayProperty in arrayProperties)
            {
                var array = arrayProperty.GetValue(obj) as Array ?? new Array[0];

                foreach (var element in array)
                {
                    RewriteAllLinks(element, helper);
                }
            }
        }

        private static bool ShouldSkipStatusCodes(int? statusCode)
        {
            var codesShouldNotBeSkipped = new int[]
            {
                (int)HttpStatusCode.OK,
                (int)HttpStatusCode.Created
            };
            return !statusCode.HasValue || 
                !codesShouldNotBeSkipped.Contains(statusCode.Value);
        }
    }
}
