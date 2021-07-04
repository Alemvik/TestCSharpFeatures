using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestLinq {
	static class Tester {
		public static void Go() {
			var owners = new List<Owner> {
			   new Owner { Name = "Alain Trépanier", Pets = new Pet[] { new Pet { Name="Miko", Age=5 }, new Pet { Name="Betzie", Age=2 },new Pet { Name="Émeraude", Age=6 }}},
			   new Owner { Name = "Évie Dutel", Pets = new Pet[] { new Pet { Name = "Snowball", Age = 1}}},
			   new Owner { Name = "Sam Bucca", Pets = new Pet[] { new Pet { Name = "Belle", Age = 8} }},
			   new Owner { Name = "Thomas Hawk", Pets = new Pet[] { new Pet { Name = "Sweetie", Age = 6}, new Pet { Name = "Rover", Age = 13}} }};

			// LINQ comes in two syntactical flavors: The Query syntax and the Method syntax. They can do almost the same, but while 
			// the query syntax is almost a new language within C#, the method syntax looks just like regular C# method calls.

			// Query syntax
			IEnumerable<string> namesA = from person in owners
				where person.Pets.All(pet => pet.Age > 5)
				orderby person.Name descending
				select person.Name;

			// Methos syntax
			IEnumerable<string> namesB = owners
			  .Where(x => x.Pets.All(pet => pet.Age > 5))
			  .OrderByDescending(p => p.Name)
			  .Select(p => p.Name);

			Debug.Assert(Enumerable.SequenceEqual(namesA, namesB)); // only has effect when compiling with debug

			Console.WriteLine("All owners that only have pets that are older then 5 years: {0}", String.Join(", ", namesA.ToArray()));

			IEnumerable<Owner> ownersA = owners.OrderByDescending(x => x.Name).TakeWhile(x => x.Name.Length <= 12); // TakeWhile is not available in Query syntax
			Console.WriteLine("Sort owners descending then take one until one's name has more then 12 letters: {0}", String.Join("; ", ownersA.Select(x => x.ToString())));
		}
		class Pet {
			public string Name { get; set; }
			public int Age { get; set; }

			public override string ToString() {
				return $"{this.Name} ({this.Age})";
			}
		}
		class Owner {
			public string Name { get; set; }
			public Pet[] Pets { get; set; }

			public override string ToString() {
				var pets = Pets.OrderBy(x => x.Name).Select(x => x.ToString());
				return $"{this.Name}: {String.Join(",", pets.ToArray())}";
			}
		}
	}
}