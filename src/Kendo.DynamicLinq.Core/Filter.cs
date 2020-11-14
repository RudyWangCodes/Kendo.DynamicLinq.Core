﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Kendo.DynamicLinq
{
    /// <summary>
    /// Represents a filter expression of Kendo DataSource.
    /// </summary>
    [DataContract]
    public class Filter
    {
        /// <summary>
        /// Gets or sets the name of the sorted field (property). Set to <c>null</c> if the <c>Filters</c> property is set.
        /// </summary>
        [DataMember(Name = "field")]
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the filtering operator. Set to <c>null</c> if the <c>Filters</c> property is set.
        /// </summary>
        [DataMember(Name = "operator")]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the filtering value. Set to <c>null</c> if the <c>Filters</c> property is set.
        /// </summary>
        [DataMember(Name = "value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the filtering logic. Can be set to "or" or "and". Set to <c>null</c> unless <c>Filters</c> is set.
        /// </summary>
        [DataMember(Name = "logic")]
        public string Logic { get; set; }

        /// <summary>
        /// Gets or sets the child filter expressions. Set to <c>null</c> if there are no child expressions.
        /// </summary>
        [DataMember(Name = "filters")]
        public IEnumerable<Filter> Filters { get; set; }

        /// <summary>
        /// Mapping of Kendo DataSource filtering operators to Dynamic Linq
        /// </summary>
        private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
        {
            {"eq", "="},
            {"neq", "!="},
            {"lt", "<"},
            {"lte", "<="},
            {"gt", ">"},
            {"gte", ">="},
            {"startswith", "StartsWith"},
            {"endswith", "EndsWith"},
            {"contains", "Contains"},
            {"doesnotcontain", "Contains"},
            {"isempty", ""},
            {"isnotempty", "!" },
            {"isnull", "="},
            {"isnotnull", "!="},
            {"isnullorempty", ""},
            {"isnotnullorempty", "!"}
        };

        /// <summary>
        /// Get a flattened list of all child filter expressions.
        /// </summary>
        public IList<Filter> All()
        {
            var filters = new List<Filter>();
            Collect(filters);
            return filters;
        }

        private void Collect(IList<Filter> filters)
        {
            if (Filters != null && Filters.Any())
            {
                foreach (var filter in Filters)
                {
                    filters.Add(filter);
                    filter.Collect(filters);
                }
            }
            else
            {
                filters.Add(this);
            }
        }

        /// <summary>
        /// Converts the filter expression to a predicate suitable for Dynamic Linq e.g. "Field1 = @1 and Field2.Contains(@2)"
        /// </summary>
        /// <param name="filters">A list of flattened filters.</param>
        public string ToExpression(Type type, IList<Filter> filters)
        {
            if (Filters != null && Filters.Any())
            {
                return "(" + string.Join(" " + Logic + " ", Filters.Select(filter => filter.ToExpression(type, filters)).ToArray()) + ")";
            }

            int index = filters.IndexOf(this);
            var comparison = Operators[Operator];

            var typeProperties = type.GetRuntimeProperties();
            var currentPropertyType = typeProperties.FirstOrDefault(f => f.Name.Equals(Field, StringComparison.OrdinalIgnoreCase))?.PropertyType;
            
            switch (Operator)
            {
                case "doesnotcontain" when currentPropertyType == typeof(string):
                    return $"!{Field}.ToLower().{comparison}(@{index})";
                case "doesnotcontain":
                    return $"({Field} != null && !{Field}.ToString().ToLower().{comparison}(@{index}))";
                case "isnull":
                case "isnotnull":
                    throw new NotSupportedException($"Operator {Operator} not support non-string type");
            }

            if (comparison == "StartsWith" || comparison == "EndsWith" || comparison == "Contains")
            {
                return currentPropertyType == typeof(string)
                    ? $"{Field}.ToLower().{comparison}(@{index})"
                    : $"({Field} != null && {Field}.ToString().ToLower().{comparison}(@{index}))";
            }

            return $"{Field} {comparison} @{index}";
        }
    }
}
