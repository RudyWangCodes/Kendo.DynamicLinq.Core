﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Kendo.DynamicLinq.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var people = new[]
            {
                new Person { Age = 40 },
                new Person { Age = 25 },
                new Person { Age = 4 },
                new Person { Age = 20 }
            };

            var sort = new List<Sort> { new Sort { Field = "age", Dir = "asc"} };

            var result = people.AsQueryable().ToDataSourceResult(10, 0, sort, null, new[]
            {
                new Aggregator
                {
                    Aggregate = "sum",
                    Field = "Age"
                }
            }, null);

            foreach (Person item in result.Data)
            {
                Console.WriteLine(item.Age);
            }
            //Console.Write(result.Aggregates);
            Console.ReadKey();
        }
    }

    [KnownType(typeof(Person))]
    public class Person
    {
        public int Age { get; set; }
    }
}
