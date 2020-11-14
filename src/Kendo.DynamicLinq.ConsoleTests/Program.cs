using System;
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
                new Person { Name = "John Doe", Age = 40 },
                new Person { Name = "Mary Jane", Age = 25 },
                new Person { Name = "Bruce Lee", Age = 4 },
                new Person { Name = "Mickey Winsley", Age = 20 }
            };

            var sort = new List<Sort> { new Sort { Field = "age", Dir = "asc"} };
            var filter = new Filter
            {
                Logic = "and",
                Filters = new List<Filter> { new Filter { Field = "name", Operator = "contains", Value = "LEE" } }
            };

            //var result = people.AsQueryable().ToDataSourceResult(10, 0, sort, null, new[]
            //{
            //    new Aggregator
            //    {
            //        Aggregate = "sum",
            //        Field = "Age"
            //    }
            //}, filter);

            var result = people.AsQueryable().ToDataSourceResult(10, 0, sort, filter);

            foreach (Person item in result.Data)
            {
                Console.WriteLine(item.Name + ", " + item.Age);
            }
            //Console.Write(result.Aggregates);
            Console.ReadKey();
        }
    }

    [KnownType(typeof(Person))]
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}
