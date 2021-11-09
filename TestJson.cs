using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

//using Newtonsoft.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TestJson {
	static class Tester {
		public static void Go(bool doBenchMarks_a = false)
		{
			var msg = "TestJson";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			var pet = new Pet {Name="Emrod", BirthDatetime=DateTime.Parse("2017-12-20 13:12")};
			var option = new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			};
			var petSerializedText = JsonSerializer.Serialize(pet,option);
			Console.WriteLine($"petSerializedText = \"{petSerializedText}\"\n\n");

			var jbm = new JsonbenchMarks();
			string sA = jbm.SerializeFromSystemTextJson();

			var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings();
			jsonSettings.DateFormatString = "yyyy-MM-dd";
			string sB = jbm.SerializeFromNewtonsoft(jsonSettings);
			if (sA != sB) Console.WriteLine($"{sA}\n\n Not equals to\n\n{sB}");
			else Console.WriteLine(sA);
			if (doBenchMarks_a) BenchmarkRunner.Run<JsonbenchMarks>();
		}
	}
	public class Pet {
		public string Name { get; set; }

		[DataType(DataType.Date)]
		[JsonConverter(typeof(JsonDateConverter))]
		public DateTime BirthDatetime { get; set; }

		public int Age => DateTime.Now.Year - this.BirthDatetime.Year;
	}
	public class Owner {
		public string Name { get; set; }
		public Pet[] Pets { get; set; }
	}

	[MemoryDiagnoser]
	public class JsonbenchMarks {
		public static readonly List<Owner> owners = new List<Owner> {
					new Owner { Name = "Alain Trépanier", Pets = new Pet[] { new Pet { Name="Miko", BirthDatetime=DateTime.Parse("2020-12-20") }, new Pet { Name="Betzie", BirthDatetime=DateTime.Parse("2021-12-20") },new Pet { Name="Émeraude", BirthDatetime=DateTime.Parse("2021-12-20") }}},
					new Owner { Name = "Évie Dutel", Pets = new Pet[] { new Pet { Name = "Snowball", BirthDatetime=DateTime.Parse("2020-12-20")}}},
					new Owner { Name = "Sam Bucca", Pets = new Pet[] { new Pet { Name = "Belle", BirthDatetime=DateTime.Parse("2013-12-20")} }},
					new Owner { Name = "Thomas Hawk", Pets = new Pet[] { new Pet { Name = "Sweetie", BirthDatetime=DateTime.Parse("2012-12-20")}, new Pet { Name = "Rover", BirthDatetime=DateTime.Parse("2010-12-20")}} }
				};
		public static readonly string serializedOwners = "[{\"Name\":\"Alain Trépanier\",\"Pets\":[{\"Name\":\"Miko\",\"BirthDatetime\":\"2020-12-20\",\"Age\":1},{\"Name\":\"Betzie\",\"BirthDatetime\":\"2021-12-20\",\"Age\":0},{\"Name\":\"Émeraude\",\"BirthDatetime\":\"2021-12-20\",\"Age\":0}]},{\"Name\":\"Évie Dutel\",\"Pets\":[{\"Name\":\"Snowball\",\"BirthDatetime\":\"2020-12-20\",\"Age\":1}]},{\"Name\":\"Sam Bucca\",\"Pets\":[{\"Name\":\"Belle\",\"BirthDatetime\":\"2013-12-20\",\"Age\":8}]},{\"Name\":\"Thomas Hawk\",\"Pets\":[{\"Name\":\"Sweetie\",\"BirthDatetime\":\"2012-12-20\",\"Age\":9},{\"Name\":\"Rover\",\"BirthDatetime\":\"2010-12-20\",\"Age\":11}]}]";

		[Benchmark]
		public string SerializeFromSystemTextJson()
		{
			JsonSerializerOptions jso = new JsonSerializerOptions();
			jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			return System.Text.Json.JsonSerializer.Serialize(owners, jso);
		}

		[Benchmark]
		public string SerializeFromNewtonsoft(Newtonsoft.Json.JsonSerializerSettings s) => Newtonsoft.Json.JsonConvert.SerializeObject(owners,s);

		[Benchmark]
		public List<Owner> DeserializeFromSystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<List<Owner>>(serializedOwners);

		[Benchmark]
		public List<Owner> DeserializeFromNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<List<Owner>>(serializedOwners);
	}
	 class JsonDateConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
       => DateTime.ParseExact(reader.GetString(),
                    "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
       => writer.WriteStringValue(value.ToString(
                    "yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}