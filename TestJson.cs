
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

//using Newtonsoft.Json;

namespace TestJson;

static class Tester {
	record Person(string Name, DateOnly BirthDay);
	public static void Go()
	{
		var msg = "TestJson";
		Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

		var pet = new Pet { Name = "Emrod",BirthDay = DateOnly.Parse("2017-12-20") };
		var person = new Person("Alain",DateOnly.Parse("1967-03-15"));
		var option = new JsonSerializerOptions (JsonSerializerDefaults.General) {
			//PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		option.Converters.Add(new DateOnlyJsonConverter());
		//options.Converters.Add(new TimeOnlyConverter());
		string personSerializedText = System.Text.Json.JsonSerializer.Serialize(person,option);
		Console.WriteLine($"personSerializedText = \"{personSerializedText}");
		string petSerializedText = System.Text.Json.JsonSerializer.Serialize(pet,option);
		Console.WriteLine($"petSerializedText = \"{petSerializedText}\"\n\n");

		string sA = SerializeFromSystemTextJson();
		string sB = SerializeFromNewtonsoft();
		if (sA != sB) Console.WriteLine($"SerializeFromSystemTextJson:\n{sA}\n\nNot equals to\n\nSerializeFromNewtonsoft:\n{sB}");
		else Console.WriteLine(sA);
	}
	
public class Pet {
	public string Name { get; set; }

	[DataType(DataType.Date)]
	[JsonConverter(typeof(DateOnlyJsonConverter))] // or use option.Converters.Add(new DateOnlyJsonConverter());
	public DateOnly BirthDay { get; set; }

	public int Age => DateTime.Now.Year - this.BirthDay.Year;
}

public class Owner {
	public string Name { get; set; }
	public Pet[] Pets { get; set; }
}


	public static readonly List<Owner> owners = new() {
				new Owner { Name = "Alain Trépanier", Pets = new Pet[] { new Pet { Name="Miko", BirthDay=DateOnly.Parse("2020-12-20") }, new Pet { Name="Betzie", BirthDay=DateOnly.Parse("2021-12-20") },new Pet { Name="Émeraude", BirthDay=DateOnly.Parse("2021-12-20") }}},
				new Owner { Name = "Évie Dutel", Pets = new Pet[] { new Pet { Name = "Snowball", BirthDay=DateOnly.Parse("2020-12-20")}}},
				new Owner { Name = "Sam Bucca", Pets = new Pet[] { new Pet { Name = "Belle", BirthDay=DateOnly.Parse("2013-12-20")} }},
				new Owner { Name = "Thomas Hawk", Pets = new Pet[] { new Pet { Name = "Sweetie", BirthDay=DateOnly.Parse("2012-12-20")}, new Pet { Name = "Rover", BirthDay=DateOnly.Parse("2010-12-20")}} }
			};
	public static readonly string serializedOwners = "[{\"Name\":\"Alain Trépanier\",\"Pets\":[{\"Name\":\"Miko\",\"BirthDatetime\":\"2020-12-20\",\"Age\":1},{\"Name\":\"Betzie\",\"BirthDatetime\":\"2021-12-20\",\"Age\":0},{\"Name\":\"Émeraude\",\"BirthDatetime\":\"2021-12-20\",\"Age\":0}]},{\"Name\":\"Évie Dutel\",\"Pets\":[{\"Name\":\"Snowball\",\"BirthDatetime\":\"2020-12-20\",\"Age\":1}]},{\"Name\":\"Sam Bucca\",\"Pets\":[{\"Name\":\"Belle\",\"BirthDatetime\":\"2013-12-20\",\"Age\":8}]},{\"Name\":\"Thomas Hawk\",\"Pets\":[{\"Name\":\"Sweetie\",\"BirthDatetime\":\"2012-12-20\",\"Age\":9},{\"Name\":\"Rover\",\"BirthDatetime\":\"2010-12-20\",\"Age\":11}]}]";

	public static string SerializeFromSystemTextJson()
	{
		JsonSerializerOptions jso = new() {
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		return System.Text.Json.JsonSerializer.Serialize(owners,jso);
	}

	public static string SerializeFromNewtonsoft()
	{
		Newtonsoft.Json.JsonSerializerSettings nsJsonSettings = new() {
			DateFormatString = "yyyy-MM-dd"
		};

		return Newtonsoft.Json.JsonConvert.SerializeObject(owners, nsJsonSettings);
	}

	public static List<Owner> DeserializeFromSystemTextJson() => System.Text.Json.JsonSerializer.Deserialize<List<Owner>>(serializedOwners);

	public static List<Owner> DeserializeFromNewtonsoft() => Newtonsoft.Json.JsonConvert.DeserializeObject<List<Owner>>(serializedOwners);

}

class DateOnlyJsonConverter : JsonConverter<DateOnly> {
	public override DateOnly Read(ref Utf8JsonReader reader,Type typeToConvert,JsonSerializerOptions options) => 
		DateOnly.ParseExact(reader.GetString(), "yyyy-MM-dd",CultureInfo.InvariantCulture);

	// public override DateOnly ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	// {
	// 	return DateOnly.ParseExact(reader.GetString(), "yyyy-MM-dd",CultureInfo.InvariantCulture);
	// }

	public override void Write(Utf8JsonWriter writer,DateOnly value,JsonSerializerOptions options) => 
		writer.WriteStringValue(value.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture));
}
