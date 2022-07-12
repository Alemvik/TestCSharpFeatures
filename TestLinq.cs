using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestLinq;

static class Tester {
	public static void Go()
	{
		var msg = "TestLinq";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

		var users = new string[] {"Alain Trepanier", "Émie Dutel", "Évie Dutel"};
		var firstDutel = users.FirstOrDefault(x => x.EndsWith("dutel", StringComparison.CurrentCultureIgnoreCase), "N/A"); // use First when you want the first item from zero or more
		var panier = users.SingleOrDefault(x => x.EndsWith("panier", StringComparison.CurrentCultureIgnoreCase), "N/A"); // use Single when you are expecting zero or one item
		var topTwo = users.Where(x => x.Contains("ie",StringComparison.CurrentCultureIgnoreCase))
			.OrderByDescending(x => x)
         .Take(2);
		Console.WriteLine($"firstDutel={firstDutel}; panier={panier}; topTwo={string.Join(',',topTwo)}");

		var owners = new List<Owner> {
			new Owner { Name = "Alain Trépanier", Pets = new Pet[] { new Pet { Name = "Miko", Age = 5 }, new Pet { Name = "Betzie", Age = 2 }, new Pet { Name = "Émeraude", Age = 6 } } },
			new Owner { Name = "Évie Dutel", Pets = new Pet[] { new Pet { Name = "Snowball", Age = 1 } } },
			new Owner { Name = "Sam Bucca", Pets = new Pet[] { new Pet { Name = "Belle", Age = 8 } } },
			new Owner { Name = "Thomas Hawk", Pets = new Pet[] { new Pet { Name = "Sweetie", Age = 6 }, new Pet { Name = "Rover", Age = 13 } } }};

		// LINQ comes in two syntactical flavors: The Query syntax and the Method syntax. They can do almost the same, but while 
		// the query syntax is almost a new language within C#, the method syntax looks just like regular C# method calls.

		// Query syntax aka fluent syntax
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

		// See https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/ranges-indexes
		var skipOneTakeThree = owners.Take(1..4).ToArray(); // from 1 to 4-1=3
		Console.WriteLine($"skipOneTakeThree = {skipOneTakeThree[0]} and {skipOneTakeThree[1]} and {skipOneTakeThree[2]}");

		var lastTwoOwners = owners.Take(^2..).ToArray(); // from -2 and all the ones after ("^2.." is same as "^2..^0")
		Console.WriteLine($"lastTwoOwners = {lastTwoOwners[0]} and {lastTwoOwners[1]}");

		var allButLastTwoOwners = owners.Take(..^3).ToArray();
		Console.WriteLine($"allButLastTwoOwners (has {allButLastTwoOwners.Length} items) = {allButLastTwoOwners[0]}");
	}

	class Pet {
		public string Name { get; set; }
		public int Age { get; set; }

		public override string ToString()
		{
			return $"{this.Name} ({this.Age})";
		}
	}

	class Owner {
		public string Name { get; set; }
		public Pet[] Pets { get; set; }

		public override string ToString()
		{
			var pets = Pets.OrderBy(x => x.Name).Select(x => x.ToString());
			return $"{this.Name}: {String.Join(",", pets.ToArray())}";
		}
	}
}