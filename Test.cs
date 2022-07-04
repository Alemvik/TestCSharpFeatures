/*
https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run
https://www.youtube.com/watch?v=ifTF3ags0XI
https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code
https://docs.microsoft.com/en-us/dotnet/core/tools/
https://docs.microsoft.com/en-us/dotnet/core/install/macos
https://dotnet.microsoft.com/en-us/download/dotnet/6.0
https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net50
https://www.tutorialsteacher.com/core/net-core-command-line-interface
https://git-scm.com/
https://docs.github.com/en/github/importing-your-projects-to-github/importing-source-code-to-github/adding-an-existing-project-to-github-using-the-command-line
https://code.visualstudio.com/shortcuts/keyboard-shortcuts-macos.pdf
Formatting options for c# (omnisharp.json): https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.formatting.csharpformattingoptions?view=roslyn-dotnet
https://code.visualstudio.com/docs/editor/intellisense

This document is best viewed when keeping tab characters and to have them worth 2 spaces.
Use VsCode's terminal to create project, run, build, etc. examples: 
	% brew install node  ;optional usefull server tools
	% brew upgrade ; update brew itself and all it has installed
	% dotnet --version
	% dotnet nuget list source
	% dotnet new console -n TestAsync ; It will create a TestAsync folder (to delete it: %rm -rf TestAsync)
	% dotnet restore ; Useful to do after adding packages in the Test.csproj file
	% dotnet run -c debug ; or: dotnet run -c release
	% dotnet build -c Release
	% dotnet clean
	% dotnet run --project Test.csproj
	% dotnet run Test.dll 
	% bin/Debug/net6.0/Test
	% ASPNETCORE_ENVIRONMENT=Staging dotnet run

	% git config --global http.sslVerify false
	% git clone https://github.com/Alemvik/TestCHarpFeatures
   % git status
	% git add .
	% git commit -m "messages"
	% git push -f
	To reset main branch to origin/main use those 2 commands:
	(git fetch downloads the latest from remote without trying to merge or rebase anything.
   Then the git reset resets the master branch to what you just fetched. The --hard option changes
	all the files in your working tree to match the files in origin/main)
	% git fetch --all
	% git reset --hard origin/main
	Use git stash to save uncommitted changed (staged or not) and git stash pop to recover them or git stash drop to discard them
Usefull VsCode extensions: .NET Core Test Explorer, Auto Rename Tag, C#, Code Runner, Debugger for Chrome, HTML CSS Support, HTML Preview, .Net Core Tools, NuGet Package Manager, Subtitles Editor, Thunder Client, vscode-icons, XML Tools
To test this, just comment in/out the throw lines from the three task members.
*/
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Data;
global using System.Data.Common;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
using System.Text.Json;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Alemvik;
using static Alemvik.Convertion; // To get the DataTable ToCsv extension
//using cnv = Alemvik.Convertion;

namespace Test;

public static class IntExtension { // Extension methods must be defined in a static class.
	public static bool Between(this int value, int from, int to) {
		return value >= from && value <= to;
	}
}

public class ProductOwner {
	[Range(5,15)]
	public int Id { get; set; }
	[Required]
	public string Name { get; set; }
	public DateTimeOffset StartDate { get; set; }
	public override string ToString() => $"Owner's name is {Name}, his id is {Id} and his start date is {StartDate:yyyy-MM-dd}\n";
}

public static class Program {
	static readonly HttpClient httpClient = new(); // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.

	static async Task Main(string[] args)
	{
		// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#getvalue
		var po = Cfg.Get<ProductOwner>("ProductOwner",null);
		//var po = config.GetValue<ProductOwner>("ProductOwner");
		if (!Validator.TryValidateObject(po,new ValidationContext(po),new List<ValidationResult>(),true)) throw new Exception("Unable to find all settings");
		Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),Cfg.Get<string>("ConsoleForegroundColor",""),true);

		InitDatabase();

		Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH\\hmm}: App version is {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}\nConfig: Console color is {Cfg.Get<string>("ConsoleForegroundColor","White")}\nProductOwner is {po}");

		Console.Write($"Environment regarding the appsettings.json file is {Cfg.environment}; Environment is {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")} ({Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")})\n"); // The ASPNETCORE_ENVIRONMENT value overrides DOTNET_ENVIRONMENT

		{
			var msg = "Test Dictionary";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			var da = new Dictionary<string, int> {
				["Oranges"] = 67
			}; // it has hashtable
			var db = new ConcurrentDictionary<string,int>(); // this one is thread safe and has more features
			db["Oranges"] = 77;
			Console.WriteLine($"Oranges count: {da["Oranges"]} and {db["Oranges"]}");
		}

		{
			var msg = "Test Read to \"string[]\" from appsettings.json";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			var animals = Cfg.Get<string[]>("Animals",null);
			Console.WriteLine($"Animals: {string.Join(',',animals)}");
		}

		{
			var msg = "Test Read to \"(string,string)[]\" from appsettings.json";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

			(string Category, string Produce)[] items = Cfg.Get("Items")
				.Get<Dictionary<string,string>[]>()
				.SelectMany(d => d) // Flatten from Dictionary[] to KeyValuePair[]
				.Select(kvp => (kvp.Key, kvp.Value))
				.ToArray();
			foreach (var (Category, Produce) in items) Console.WriteLine($"({Category},{Produce})");
		}

		{
			var msg = "Test Read to \"List<(string,string[])>\" from appsettings.json";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

			List<(string Category, string[] Produces)> products = Cfg.Get("Products")
				.Get<Dictionary<string,string[]>[]>()
				.SelectMany(i => i) // Flatten from Dictionary[] to KeyValuePair[]
				.Select(i => (i.Key, i.Value)) // .Select(i => new ValueTuple<string, string[]>(i.Key, i.Value))
				.ToList();

			foreach (var (Category, Produces) in products) {
				Array.Sort(Produces);
				Console.WriteLine($"({Category},[{String.Join(',',Produces)}])");
			}
		}

		{
			var msg = "Test Read to \"List<(string,string,string)>\" from appsettings.json";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

			List<(string nme, string ado, string con)> dataSources = Cfg.Get("DataSources")
				.Get<Dictionary<string,Dictionary<string,string>>[]>()
				.SelectMany(i => i)
				.Select(i => ValueTuple.Create(i.Key,i.Value.First().Key,i.Value.First().Value))
				.ToList();
			Db.Init(dataSources);
			foreach (var ds in dataSources) Console.WriteLine($"{ds}");
		}

		{
			var msg = "Test Dictionary to list of System.ValueTuple";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			List<(long a, int b)> tuples = (new Dictionary<long,int>() { { 1L,1 },{ 2L,2 } }).Select(x => (x.Key, x.Value)).ToList();
			foreach (var (a, b) in tuples) Console.WriteLine($"({a},{b})");
		}

		{
			var msg = "Test System.ValueTuple";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			(string nme, string ado, string con)[] dataSources = { ("One", "SqlServer", "Source=..."),("Two", "MySql", "Source=...") };
			foreach (var item in dataSources) Console.WriteLine($"{item}");
		}

		{
			var msg = "Test linq's SelectMany";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			var names = new List<string>() { "Alain","Trépanier" };
			var namesFlat = names.SelectMany(x => x);
			foreach (char c in namesFlat) Console.Write(c + " ");
			Console.Write("\n");
		}

		{
			var msg = "Test CsvToInsert statement";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			string str = CsvToInsert("Test2.csv", "dbo.TableA", ',', (x=>x.Replace("\\n","'+CHAR(10)+'")));
			Console.WriteLine('\n'+str+'\n');
		}

		{
			var msg = "Test CsvToUpdates statement";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			var updates = CsvToUpdates("Test2.csv", "dbo.TableA", ',', (x=>x.Replace("\\n","'+CHAR(10)+'")));
			Console.WriteLine('\n'+string.Join('\n',updates)+'\n');
		}

		{ // The null-coalescing operator ?? returns the value of its left-hand operand if it isn't null; otherwise, it evaluates the right-hand operand and returns its result. The ?? operator doesn't evaluate its right-hand operand if the left-hand operand evaluates to non-null.
			var msg = "Test the null-coalescing operator ??";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			int? i1=default, i2=null,i3=3;
			Console.WriteLine($"First not null among int? i1, i2=null,i3=3;: {i1??i2??i3}");
		}

		{ // The null-coalescing assignment operator ??= assigns the value of its right-hand operand to its left-hand operand only if the left-hand operand evaluates to null.
			var msg = "Test the null-coalescing assignment operator ??=";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			int? i1=default, i2=null,i3=3; // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/default-values
			i1 ??= i3; // equivalent to: if (i1 is null) i1 = i3;
			Console.WriteLine($"i1 after int? i1=default, i2=null,i3=3;i1 ??= i3; = {i1}");
			i1 ??= 4; // equivalent to: if (i1 is null) i1 = 4;
			Console.WriteLine($"i1 after int? i1=3, i2=null,i3=3;i1 ??= 4; = {i1}");
		}

		{ // https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0
			var msg = "Test sending a https web get request";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

			string uri = "https://api.agify.io?name=Alain"; // https://mixedanalytics.com/blog/list-actually-free-open-no-auth-needed-apis/
			try {
				HttpResponseMessage response = await httpClient.GetAsync(uri);
				response.EnsureSuccessStatusCode();
				string responseBody = await response.Content.ReadAsStringAsync();
				// Above three lines can be replaced with this new helper method: string responseBody = await httpClient.GetStringAsync(uri);

				Console.WriteLine("\r\nThe following headers were received in the response:");
				// Displays each header and it's key associated with the response.
				// RFC 2616:
				//   Content header fields here https://www.rfc-editor.org/rfc/rfc2616#section-7.1
				//   Request header fields here https://www.rfc-editor.org/rfc/rfc2616#section-5.3
				//   Response header fields here https://www.rfc-editor.org/rfc/rfc2616#section-6.2
				foreach(var header in response.Content.Headers)
					Console.Write($"  {header.Key} : {((string[])header.Value)[0]}\n");

				if (response.Content.Headers.TryGetValues("Content-Type", out var value)) {
					string contentType = value.ToArray()[0].Split(';')[0].Trim();
					// == "application/json"
					Console.Write($"contentType={contentType}\n");
				}

				if (uri == "http://worldclockapi.com/api/json/utc/now") {
					Console.WriteLine(responseBody);
					var jsonObj = JsonSerializer.Deserialize<Dictionary<string,object>>(responseBody);
					Console.WriteLine($"currentDateTime = {DateTime.Parse(jsonObj["currentDateTime"].ToString()):yyyy-mm-dd HH:mm:ss}\n");
				} else if (uri.StartsWith("https://api.agify.io?name=")) {
					Console.WriteLine(responseBody);
					var jsonObj = JsonSerializer.Deserialize<Dictionary<string,object>>(responseBody);
					Console.WriteLine($"Age = {jsonObj["age"]??0}\n");
				}
				//await Task.Delay(rnd.Next(1, 5 * 60 + 1) * 1000); // wait between 1 to 5 minutes
			} catch(HttpRequestException e) {
				Console.WriteLine("\nException Caught!");	
				Console.WriteLine("Message :{0} ",e.Message);
			}
		}
		Console.Write('\n');

		// *** The other tests (just uncoment the ones you want to explore) ***
		//TestRegex.Tester.Go();
		//TestAsync.Tester.Go(".");
		//TestDatabase.Tester.Go();
		//TestMisc.Tester.Go();
		//TestJson.Tester.Go();
		//TestLinq.Tester.Go();
		//TestExtension.Tester.Go();
		//TestSpan.Tester.Go();
		//TestStream.Tester.Go();
		//await TestDynamicType.Tester.Go("Alemvik" /*"ElfoCrash"*/);
		//TestCsv();
		//TestXquery.Tester.Go();
		//TestComposition.Tester.Go();
		//TestDeconstruction.Tester.Go();
		await TestEvents.Tester.Go();
		//Console.WriteLine(DateTime.Now.Date); // How to have it is OS default format ?
		//var api = new MinimalApi();
		//var api = new MinimalApiUsingCarter(); // https://localhost:5501/donut
		//TestBestPath.Tester.Go();
	}

	static void InitDatabase()
	{
		DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

		//Db.Init(new Emvie.DataSource[] { new("MySqlSrvA", "MySql.Data.MySqlClient", "DataSource=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq;program_name=test") });

		List<(string nme, string ado, string con)> dataSources = Cfg.Get("DataSources")
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
		Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),Cfg.Get<string>("ConsoleForegroundColor","White"),true);
	}

	public static void DiceProbabilities()
	{
		const int dices = 1;
		var result = new List<int[]> {
			new int[dices] // add first combination
		};
		Array.Fill(result[0],1);
		for (; ; ) {
			var lastResult = result[^1];
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
	}
}
