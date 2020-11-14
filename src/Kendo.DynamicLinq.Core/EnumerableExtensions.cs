﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Kendo.DynamicLinq
{
    public static class EnumerableExtensions
    {
        public static dynamic GroupByMany<TElement>(this IEnumerable<TElement> elements, IEnumerable<Group> groupSelectors)
        {
            //create a new list of Kendo Group Selectors 
            var selectors = new List<GroupSelector<TElement>>(groupSelectors.Count());
            foreach (var selector in groupSelectors)
            {
                //compile the Dynamic Expression Lambda for each one
                var expression = DynamicExpressionParser.ParseLambda(false, typeof(TElement), typeof(object), selector.Field);
                //add it to the list
                selectors.Add(new GroupSelector<TElement>
                {
                    Selector = (Func<TElement, object>)expression.Compile(),
                    Field = selector.Field,
                    Aggregates = selector.Aggregates
                });
            }

            //call the actual group by method
            return elements.GroupByMany(selectors.ToArray());
        }

        public static dynamic GroupByMany<TElement>(this IEnumerable<TElement> elements, params GroupSelector<TElement>[] groupSelectors)
        {
            if (groupSelectors.Length > 0)
            {
                //get selector
                var selector = groupSelectors.First();
                var nextSelectors = groupSelectors.Skip(1).ToArray();   //reduce the list recursively until zero

                //group by and return                
                return  elements.GroupBy(selector.Selector).Select( 
                            g => new GroupResult
                            {
                                Value = g.Key,
                                Aggregates = selector.Aggregates,
                                HasSubgroups = groupSelectors.Length > 1,
                                Count = g.Count(),                                
                                Items = g.GroupByMany(nextSelectors),   //recursivly group the next selectors
                                SelectorField = selector.Field
                            });
            }

            //if there are not more group selectors return data
            return elements;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            foreach (var i in source)
            {
                yield return i;
            }

            yield return item;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;

            foreach (T i in source)
            {
                yield return i;
            }
        }
    }
}
