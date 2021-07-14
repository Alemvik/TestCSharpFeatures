using System;
using System.Collections.Generic;
using System.Text.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TestJson {
	static class Tester {
		public static void Go(bool doBenchMarks_a = false)
		{
			Console.WriteLine($"\n--- TestJson {new String('-', 50)}\n");

			var jbm = new JsonbenchMarks();
			string sA = jbm.SerializeFromSystemTextJson();
			string sB = jbm.SerializeFromNewtonsoft();
			if (sA != sB) Console.WriteLine($"{sA}\n Not equals to\n{sB}");
			Console.WriteLine(sA);
			if (doBenchMarks_a) BenchmarkRunner.Run<JsonbenchMarks>();
		}
	}
	public class Pet {
		public string Name { get; set; }
		public int Age { get; set; }
	}
	public class Owner {
		public string Name { get; set; }
		public Pet[] Pets { get; set; }
	}

	[MemoryDiagnoser]
	public class JsonbenchMarks {
		public static readonly List<Owner> owners = new List<Owner> {
					new Owner { Name = "Alain Trépanier", Pets = new Pet[] { new Pet { Name="Miko", Age=5 }, new Pet { Name="Betzie", Age=2 },new Pet { Name="Émeraude", Age=6 }}},
					new Owner { Name = "Évie Dutel", Pets = new Pet[] { new Pet { Name = "Snowball", Age = 1}}},
					new Owner { Name = "Sam Bucca", Pets = new Pet[] { new Pet { Name = "Belle", Age = 8} }},
					new Owner { Name = "Thomas Hawk", Pets = new Pet[] { new Pet { Name = "Sweetie", Age = 6}, new Pet { Name = "Rover", Age = 13}} }
				};
		public static readonly string serializedOwners = "[{\"Name\":\"Alain Trépanier\",\"Pets\":[{\"Name\":\"Miko\",\"Age\":5},{\"Name\":\"Betzie\",\"Age\":2},{\"Name\":\"Émeraude\",\"Age\":6}]},{\"Name\":\"Évie Dutel\",\"Pets\":[{\"Name\":\"Snowball\",\"Age\":1}]},{\"Name\":\"Sam Bucca\",\"Pets\":[{\"Name\":\"Belle\",\"Age\":8}]},{\"Name\":\"Thomas Hawk\",\"Pets\":[{\"Name\":\"Sweetie\",\"Age\":2},{\"Name\":\"Rover\",\"Age\":13}]}]";

		[Benchmark]
		public string SerializeFromSystemTextJson()
		{
			JsonSerializerOptions jso = new JsonSerializerOptions();
			jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			return System.Text.Json.JsonSerializer.Serialize(owners, jso);
		}

		[Benchmark]
		public string SerializeFromNewtonsoft() => Newtonsoft.Json.JsonConvert.SerializeObject(owners);

		[Benchmark]
		public List<Owner> DeserializeFromSystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<List<Owner>>(serializedOwners);

		[Benchmark]
		public List<Owner> DeserializeFromNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<List<Owner>>(serializedOwners);
	}
}