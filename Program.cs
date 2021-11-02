/*
https://www.youtube.com/watch?v=ifTF3ags0XI
https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code
https://docs.microsoft.com/en-us/dotnet/core/tools/
https://docs.microsoft.com/en-us/dotnet/core/install/macos
https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net50
https://www.tutorialsteacher.com/core/net-core-command-line-interface
https://git-scm.com/
https://docs.github.com/en/github/importing-your-projects-to-github/importing-source-code-to-github/adding-an-existing-project-to-github-using-the-command-line
https://code.visualstudio.com/shortcuts/keyboard-shortcuts-macos.pdf
Formatting options for c# (omnisharp.json): https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.formatting.csharpformattingoptions?view=roslyn-dotnet

This document is best viewed when keeping tab characters and to have them worth 2 spaces.
Use VsCode's terminal to create project, run, build, etc. examples: 
	% brew install node  ;optional usefull server tools
	% brew upgrade ; update brew itself and all it has installed
	% dotnet --version
	% dotnet nuget list source
	% dotnet new console -n TestAsync ; It will create a TestAsync folder (to delete it: %rm -rf TestAsync)
	% dotnet restore ; Useful to do after adding packages in the Test.csproj file
	% dotnet run -c debug ; or: dotnet run -c release
	% dotnet build
	% dotnet run TestAsync.dll 

   % git status
	% git add .
	% git commit -m "messages"
	% git push -f
Usefull VsCode extensions: .NET Core Test Explorer, Auto Rename Tag, C#, Code Runner, Debugger for Chrome, HTML CSS Support, HTML Preview, .Net Core Tools, NuGet Package Manager, Subtitles Editor, Thunder Client, vscode-icons, XML Tools
To test this, just comment in/out the throw lines from the three task members.
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Alemvik;
using static Alemvik.Convertion; // To get the DataTable ToCsv extension
//using cnv = Alemvik.Convertion;
namespace Test {
	public class ProductOwner {
		[Range(5,15)]
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
		public DateTimeOffset StartDate { get; set; }
		public override string ToString() => $"Owner's name is {Name}, his id is {Id} and his start date is {StartDate.ToString("yyyy-MM-dd")}\n";
	}
	public class Program {
		public static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
		static async Task Main(string[] args_a)
		{
			InitDatabase();

			// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#getvalue
			var po = config.GetSection("ProductOwner").Get<ProductOwner>();
			//var po = config.GetValue<ProductOwner>("ProductOwner");
			if (!Validator.TryValidateObject(po,new ValidationContext(po),new List<ValidationResult>(),true)) throw new Exception("Unable to find all settings");
			Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),config.GetValue<string>("ConsoleForegroundColor"),true);
			Console.WriteLine($"Config: Console color is {config.GetValue<string>("ConsoleForegroundColor")}\nProductOwner is {po}");

			{
				var msg = "Test Dictionary";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
				var da = new Dictionary<string,int>(); // it has hashtable
				da["Oranges"] = 67;
				var db = new ConcurrentDictionary<string,int>(); // this one is thread safe and has more features
				db["Oranges"] = 77;
				Console.WriteLine($"Oranges count: {da["Oranges"]} and {db["Oranges"]}");
			}

			{
				var msg = "Test Read to \"string[]\" from appsettings.json";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
				var animals = config.GetSection("Animals").Get<string[]>();
				Console.WriteLine($"Animals: {string.Join(',',animals)}");
			}

			{
				var msg = "Test Read to \"(string,string)[]\" from appsettings.json";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

				(string Category, string Produce)[] items = config.GetSection("Items")
				   .Get<Dictionary<string,string>[]>()
			   	.SelectMany(d => d) // Flatten from Dictionary[] to KeyValuePair[]
				   .Select(kvp => (kvp.Key, kvp.Value))
					.ToArray();
				foreach (var item in items) Console.WriteLine($"({item.Category},{item.Produce})");
			}

			{
				var msg = "Test Read to \"List<(string,string[])>\" from appsettings.json";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

				List<(string Category, string[] Produces)> products = config.GetSection("Products")
				   .Get<Dictionary<string,string[]>[]>()
				   .SelectMany(i => i) // Flatten from Dictionary[] to KeyValuePair[]
					.Select(i => (i.Key, i.Value)) // .Select(i => new ValueTuple<string, string[]>(i.Key, i.Value))
					.ToList();

				foreach (var product in products) {
					Array.Sort(product.Produces);
					Console.WriteLine($"({product.Category},[{String.Join(',',product.Produces)}])");
				}
			}

			{
				var msg = "Test Read to \"List<(string,string,string)>\" from appsettings.json";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

				List<(string nme, string ado, string con)> dataSources = config.GetSection("DataSources")
					.Get<Dictionary<string,Dictionary<string,string>>[]>()
					.SelectMany(i => i)
					.Select(i => ValueTuple.Create(i.Key,i.Value.First().Key,i.Value.First().Value))
					.ToList();
				Db.Init(dataSources);
				foreach (var ds in dataSources) Console.WriteLine($"{ds}");
			}

			// Dictionary to list of System.ValueTuple
			{
				var msg = "Test Dictionary to System.ValueTuple";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
				List<(long a, int b)> tuples = (new Dictionary<long,int>() { { 1L,1 },{ 2L,2 } }).Select(x => (x.Key, x.Value)).ToList();
				foreach (var tuple in tuples) Console.WriteLine($"({tuple.a},{tuple.b})");
			}

			// Test System.ValueTuple
			{
				var msg = "Test System.ValueTuple";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
				(string nme, string ado, string con)[] dataSources = { ("One", "SqlServer", "Source=..."),("Two", "MySql", "Source=...") };
				foreach (var item in dataSources) Console.WriteLine($"{item}");
			}

			// Test System.ValueTuple
			{
				var msg = "Test linq's SelectMany";
				Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
				var names = new List<string>() { "Alain","Trépanier" };
				var namesFlat = names.SelectMany(x => x);
				foreach (char c in namesFlat) Console.Write(c + " ");
				Console.Write("\n");
			}

			const int dices = 1;
			var result = new List<int[]>();
			result.Add(new int[dices]); // add first combination
			Array.Fill(result[0],1);
			for (; ; ) {
				var lastResult = result[result.Count - 1];
				var newResult = new int[dices];
				lastResult.CopyTo(newResult,0);
				result.Add(newResult);

				for (int i = 0; i < dices; i++) {
					if (newResult[i] < 6) { newResult[i]++; break; }
					if (i == dices - 1 && newResult[dices - 1] == 6) goto A;
					newResult[i] = 1;
				}
			}
		A:
			var total = new int[2 * dices + 1];
			Array.Fill(total,0);
			for (int i = 0; i < Math.Pow(6,dices); i++) {
				int lineTotal = 0;
				Console.Write($"\n{i + 1,4}: ");
				for (int j = 0; j < dices; j++) {
					Console.Write($"{result[i][j],-3}");
					if (result[i][j] == 6) lineTotal += 2;
					else if (result[i][j] > 3) lineTotal++;
				}
				total[lineTotal]++;
			}
			Console.Write("\n----\n");
			for (int i = 0; i < total.Length; i++) Console.WriteLine($"{i,-4}: {total[i],6}{total[i] / Math.Pow(6,dices) * 100,10:0.00}");

			// *** The other tests ***
			//TestAsync.Tester.Go(".");
			//TestDatabase.Tester.Go();
			//TestMisc.Tester.Go();
			//TestJson.Tester.Go(false);
			// TestLinq.Tester.Go();
			//TestExtension.Tester.Go();
			// TestSpan.Tester.Go();
			// TestStream.Tester.Go();
			//await TestDynamicType.Tester. Go("Alemvik" /*"ElfoCrash"*/); // if you don't use this test, better 
			//TestCsv();
			//TestXquery.Tester.Go();
			//Console.WriteLine(DateTime.Now.Date); // How to havie it is OS format ?
			var api = new MinimalApi(); // https://localhost:5001/donut
		}

		static void InitDatabase()
		{
			DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

			//Db.Init(new Emvie.DataSource[] { new("MySqlSrvA", "MySql.Data.MySqlClient", "DataSource=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq;program_name=test") });

			List<(string nme, string ado, string con)> dataSources = config.GetSection("DataSources")
				.Get<Dictionary<string, Dictionary<string, string>>[]>() // make sure you included Microsoft.Extensions.Configuration.Binder nuget package
				.SelectMany(i => i)
				.Select(i => ValueTuple.Create(i.Key, i.Value.First().Key, i.Value.First().Value))
				.ToList();
			Db.Init(dataSources);
		}
		static void TestCsv()
		{
			Console.WriteLine($"\n--- TestCsv {new String('-',50)}\n");
			DataTable tbl;
			using (var sr = new StreamReader("Test.csv")) tbl = ConvertCsvToDataTable(sr,new string[] { "first","last","birth date" });
			// var ms = new MemoryStream();
			// using (FileStream fs = new FileStream("Test.csv", FileMode.Open, FileAccess.Read)) fs.CopyTo(ms);
			// ms.Seek(0, SeekOrigin.Begin);
			// using (var sr = new StreamReader(ms)) tbl = ConvertCsvToDataTable(sr, new string[] { "first", "last", "birth date" });
			foreach (DataColumn c in tbl.Columns) Console.Write($"{c.ColumnName,-20}"); Console.Write($"\n{new String('-',20 * tbl.Columns.Count)}\n");
			foreach (DataRow r in tbl.Rows) {
				foreach (DataColumn c in tbl.Columns) Console.Write($"{r[c.Ordinal],-20}");
				Console.Write("\n");
			}
			Console.WriteLine($"tbl.Columns.Count = {tbl.Columns.Count}, tbl.Rows.Count = {tbl.Rows.Count}");
			string csv = tbl.ToCsv();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write($"\n---\n{csv}\n---\n");
			Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),config.GetValue<string>("ConsoleForegroundColor"),true);
		}
	}
}