﻿using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SortMyStuffAPI.Infrastructure
{
    public static class FormMetadata
    {
        public static FormSpecification FromModel(RequestForm model, Link self)
        {
            var formFields = new List<FormField>();

            foreach (var prop in GetAllPropertyInfo(model.GetType()))
            {
                var value = prop.CanRead
                    ? prop.GetValue(model)
                    : null;

                var attributes = prop.GetCustomAttributes().ToArray();

                var name = attributes.OfType<DisplayAttribute>().SingleOrDefault()?.Name
                    ?? prop.Name.ToCamelCase();

                var description = attributes.OfType<DisplayAttribute>()
                    .SingleOrDefault()?.Description;

                var enumValues = attributes.OfType<OptionAttribute>().SingleOrDefault()?.Options;
                var options = new List<FormFieldOption>();
                if (enumValues != null)
                {
                    foreach(var v in enumValues)
                    {
                        options.Add(new FormFieldOption
                        {
                            // display the name of the enum values rather than the number
                            Value = v.ToString()
                        });
                    }
                }

                var secret = attributes.OfType<SecretAttribute>().Any();
                var required = attributes.OfType<RequiredAttribute>().Any();
                var immutable = attributes.OfType<ImmutableAttribute>().Any();

                string scopedUnique = null;
                var requireUnique = attributes.OfType<ScopedUniqueAttribute>().Any();
                if (requireUnique)
                {
                    scopedUnique = $"Require uniqueness for every " +
                        $"{prop.GetCustomAttribute<ScopedUniqueAttribute>().Scope.ToString()}";
                }

                var type = GetFriendlyType(prop, attributes);

                var stringLength = attributes.OfType<StringLengthAttribute>()
                    .SingleOrDefault()?.MaximumLength;

                var minLength = attributes.OfType<MinLengthAttribute>()
                    .SingleOrDefault()?.Length;
                var maxLength = attributes.OfType<MaxLengthAttribute>()
                    .SingleOrDefault()?.Length;

                formFields.Add(new FormField
                {
                    Name = name,
                    Required = required,
                    Secret = secret,
                    Immutable = immutable,
                    ScopedUnique = scopedUnique,
                    Options = options.Any() ? options.ToArray() : null,
                    Type = type,
                    StringLength = stringLength,
                    Value = value,
                    Description = description,
                    MinLength = minLength,
                    MaxLength = maxLength
                });
            }

            return new FormSpecification()
            {
                Self = self,
                Name = model.GetType().Name.ToCamelCase(),
                Value = formFields.ToArray()
            };
        }

        private static IEnumerable<PropertyInfo> GetAllPropertyInfo(Type type)
        {
            foreach (var prop in type.GetTypeInfo().DeclaredProperties)
            {
                yield return prop;
            }

            if (type != typeof(RequestForm))
            {
                foreach (var prop in GetAllPropertyInfo(type.BaseType))
                {
                    yield return prop;
                }
            }
        }

        private static string GetFriendlyType(PropertyInfo property, Attribute[] attributes)
        {
            var isEmail = attributes.OfType<EmailAddressAttribute>().Any();
            if (isEmail) return "email";

            var typeName = FormFieldTypeConverter.GetTypeName(property.PropertyType);
            if (!string.IsNullOrEmpty(typeName)) return typeName;

            return property.PropertyType.Name.ToCamelCase();
        }
    }
}
