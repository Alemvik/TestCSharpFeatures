/*
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
	% dotnet run Test.dll 
	% bin/Debug/net6.0/Test
	% ASPNETCORE_ENVIRONMENT=Staging dotnet run

   % git status
	% git add .
	% git commit -m "messages"
	% git push -f
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
global using System.IO;
global using System.Linq;
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

public class Program {
	private static readonly IConfiguration cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json",optional:false,reloadOnChange:true).Build();
	public static readonly string environment = cfg.GetSection("Environment").Get<string>();
	public static readonly IConfiguration config = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json",optional:false,reloadOnChange:true)
		.AddJsonFile($"appsettings_{environment}.json",optional:true,reloadOnChange:true) // Last loaded key wins! https://devblogs.microsoft.com/premier-developer/order-of-precedence-when-configuring-asp-net-core/
		.Build();

	static async Task Main(string[] args)
	{
		if (args.Length > 0) {
			int start=0;
			int width = 0;
			//if (args.Length > 1) int.TryParse(args[1], out start);

			int[,] matrix;

A:			if (int.TryParse(args[0], out int height)) {
				width = height;
				if (args.Length > 1) int.TryParse(args[1], out width);
				matrix = new int[width,height];
				Random rnd = new();
				for (int y=0; y<width ;y++) for (int x=0; x<height ;x++) matrix[y,x] = rnd.Next(10); // random int betwwen 0 inclusively and 9 inclusively
			} else {
				var lines = System.IO.File.ReadAllText(args[0]).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				height = lines.Length;
				width = lines[0].Length;
				matrix = new int[height,width];
				unsafe { fixed (int* p = matrix) new Span<int>(p, matrix.Length).Fill(int.MaxValue);} // requires to be inside unsafe methods or unsabe block
				for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) if (lines[y][x]>='0' && lines[y][x]<='9') matrix[y,x] = lines[y][x] - '0';

				var matrix5 = new int[height*5,width*5];
				for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[y,x] = matrix[y,x];
				for (int i=0; i<4 ;i++) for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[(i+1)*height+y,x] = matrix5[i*height+y,x] + 1  == 10 ? 1 : matrix5[i*height+y,x] + 1;
				for (int i=0; i<5 ;i++) for (int j=0; j<4 ;j++) for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[i*height+y,(j+1)*width+x] = matrix5[i*height+y,j*width+x] + 1 == 10 ? 1 : matrix5[i*height+y,j*width+x] + 1;
				//matrix = matrix5;
				/*for (int y=0; y<height*5 ;y++) {
					for (int x=0; x<width*5 ;x++) Console.Write(matrix5[y,x]);
					Console.Write('\n');
				}*/
			}

			/*var sw2 = Stopwatch.StartNew();
			var (risk2, path2) = FindBestPath2(matrix,start);
			sw2.Stop();
			Console.Write('\n');PrintMatrix(matrix,risk2,path2,sw2);*/

			(int y,int x) srcLoc = (0,0);
			(int y,int x) dstLoc = (matrix.GetLength(0)-1,matrix.GetLength(1)-1);
			var sw1 = Stopwatch.StartNew();
			var (risk, path) = FindBestPath(matrix, srcLoc, dstLoc);

			//if (int.TryParse(args[0], out int ghr) && (risk2 - risk < 3 /*|| path.Count == path2.Count*/)) goto A;

			Console.Write('\n'); PrintMatrix(matrix,risk,path,sw1);

			return;
		}

		// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#getvalue
		var po = config.GetSection("ProductOwner").Get<ProductOwner>();
		//var po = config.GetValue<ProductOwner>("ProductOwner");
		if (!Validator.TryValidateObject(po,new ValidationContext(po),new List<ValidationResult>(),true)) throw new Exception("Unable to find all settings");
		Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),config.GetValue<string>("ConsoleForegroundColor"),true);

		InitDatabase();

		Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH\\hmm}: App version is {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}\nConfig: Console color is {config.GetValue<string>("ConsoleForegroundColor")}\nProductOwner is {po}");

		Console.Write($"Environment is {environment}; Environment is {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")} ({Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")})\n"); // The ASPNETCORE_ENVIRONMENT value overrides DOTNET_ENVIRONMENT

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
			foreach (var (Category, Produce) in items) Console.WriteLine($"({Category},{Produce})");
		}

		{
			var msg = "Test Read to \"List<(string,string[])>\" from appsettings.json";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");

			List<(string Category, string[] Produces)> products = config.GetSection("Products")
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

			List<(string nme, string ado, string con)> dataSources = config.GetSection("DataSources")
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

		// *** The other tests ***
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
		//Console.WriteLine(DateTime.Now.Date); // How to have it is OS default format ?
		//var api = new MinimalApi();
		//var api = new MinimalApiUsingCarter(); // https://localhost:5501/donut
	}

	static void InitDatabase()
	{
		DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

		//Db.Init(new Emvie.DataSource[] { new("MySqlSrvA", "MySql.Data.MySqlClient", "DataSource=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq;program_name=test") });

		string configSection = "DataSources_" + environment;
		if (!config.GetChildren().Any(item => item.Key == configSection)) configSection = configSection[0..configSection.LastIndexOf('_')];
		//Console.Write($"configSection={configSection}");

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

	/*public static unsafe void FillMultiDimArray<T>(this Array array, T value)
	{
		fixed (int* a = Array.Fill<int>() {
			var span = new Span<T>(array, array.Length);
			span.Fill(value);
		}
	}*/

	static (int risk, List<(int y,int x)> path) FindBestPath2(int[,] matrix, int start=0)
	{
		int wh = matrix.GetLength(0);
		if (wh != matrix.GetLength(1)) throw new ArgumentException("matrix must be square");
		if (!start.Between(-wh+1,wh-1)) throw new ArgumentException($"start must be between {-wh+1} inclusively and {wh-1} inclusively");

		//var sw = Stopwatch.StartNew();
		var perimeters = new (List<(int y,int x)> path, int risk)[3,2*wh-1]; //Array.Fill(horA,(null,0));
		var innerPerimeterIx = 0;
		var outterPerimeterIx = 1;
		var outterCopyIx = 2;
		perimeters[innerPerimeterIx,0] = (new List<(int y,int x)>() {(wh-1,wh-1)},matrix[wh-1,wh-1]);
		for (int x=0; x<wh-1 ;x++) {// with wh=5: 0123
			int innerPerimeterLength = 2 * x + 1; // with wh=5: 1,3,5,7
			int outterPerimeterLength = innerPerimeterLength + 2;
			int innerPerimeterWidth = (innerPerimeterLength + 1) / 2;
			int outterPerimeterWidth = (outterPerimeterLength + 1) / 2;
			(int y, int x) innerPerimeterLocation = (wh-innerPerimeterWidth,wh-innerPerimeterWidth);
			(int y, int x) outterPerimeterLocation = (innerPerimeterLocation.y-1,innerPerimeterLocation.x-1);

			for (int i=0; i<outterPerimeterWidth ;i++) perimeters[outterCopyIx,i].risk = matrix[wh-1-i,wh-x-2];
			for (int i=0; i<x+1 ;i++) perimeters[outterCopyIx,outterPerimeterWidth+i].risk = matrix[wh-outterPerimeterWidth,wh-1-x+i];

			var io = new int[outterPerimeterLength];
			io[outterPerimeterWidth] = io[outterPerimeterWidth-1] = io[outterPerimeterWidth-2] = innerPerimeterWidth-1;
			for (int i=outterPerimeterWidth+1; i<outterPerimeterLength ;i++) io[i] = io[i-1]+1;
			for (int i=outterPerimeterWidth-3; i>=0 ;i--) io[i] = io[i+1]-1;

			for (int o=0; o<outterPerimeterLength ;o++) { // botomleft to upperright
				perimeters[outterPerimeterIx,o].risk = int.MaxValue;
				int ii=0;
				for (int i=0; i<innerPerimeterLength ;i++) {
					int r = perimeters[outterCopyIx,o].risk + perimeters[innerPerimeterIx,i].risk;
					var path = new List<(int y, int x)> {
						Location(outterPerimeterLocation, outterPerimeterWidth, o)
					};

					// Reached the two lefttop corners
					if (i==innerPerimeterWidth-1 && o==outterPerimeterWidth-1) {
						if (perimeters[outterCopyIx,o-1].risk <= perimeters[outterCopyIx,o+1].risk) {
							r += perimeters[outterCopyIx,o-1].risk;
							path.Add((outterPerimeterLocation.y+1,outterPerimeterLocation.x));
						} else {
							r += perimeters[outterCopyIx,o+1].risk;
							path.Add((outterPerimeterLocation.y,outterPerimeterLocation.x+1));
						}
					} else {
						int l = (o>=outterPerimeterWidth ? Array.LastIndexOf(io,i) : Array.IndexOf(io,i)) - o;
						if (l>0) for (int k=0; k<l ;k++) {r += perimeters[outterCopyIx,o+k+1].risk; path.Add(Location(outterPerimeterLocation,outterPerimeterWidth,o+k+1));}
						else for (int k=0; k<-l ;k++) {r += perimeters[outterCopyIx,o-k-1].risk; path.Add(Location(outterPerimeterLocation,outterPerimeterWidth,o-k-1));}
					}

					if (r < perimeters[outterPerimeterIx,o].risk || (r == perimeters[outterPerimeterIx,o].risk && path.Count < perimeters[outterPerimeterIx,o].path.Count)) {
						perimeters[outterPerimeterIx,o].risk = r;
						perimeters[outterPerimeterIx,o].path = path;
						ii=i;
					}
				}

				perimeters[outterPerimeterIx,o].path.AddRange(perimeters[innerPerimeterIx,ii].path);
			}

// for (int k=0; k<innerPerimeterLength ;k++) Console.Write($"{perimeters[innerPerimeterIx,k].risk} ");Console.Write(" ==> ");
// for (int k=0; k<outterPerimeterLength ;k++) Console.Write($"{perimeters[outterCopyIx,k].risk} ");Console.Write(" ==> ");
// for (int k=0; k<outterPerimeterLength ;k++) Console.Write($"{perimeters[outterPerimeterIx,k].risk} ");Console.Write('\n');

			innerPerimeterIx ^= 1;
			outterPerimeterIx ^= 1;
		}

		var startIx = start<0 ? wh+start-1 : wh+start-1;
/*		for (int y=0; y<wh ;y++) {
			for (int x=0; x<wh ;x++) if (matrix[y,x]==int.MaxValue) Console.Write("￭"); else if (perimeters[innerPerimeterIx,startIx].path.Contains((y,x))) {if (Console.IsOutputRedirected) Console.Write('●'); else {Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"{matrix[y,x],1}"); Console.ForegroundColor = ConsoleColor.White;}} else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}
		Console.WriteLine($"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {wh} x {wh} is {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}; LowestRisk={perimeters[innerPerimeterIx,startIx].risk-perimeters[outterCopyIx,startIx].risk} ({perimeters[innerPerimeterIx,startIx].path.Count-1} moves); start={start}\n");
*/
		return (perimeters[innerPerimeterIx,startIx].risk-perimeters[outterCopyIx,startIx].risk,perimeters[innerPerimeterIx,startIx].path);

		static (int y,int x) Location((int y,int x) location, int width, int x) {
			if (x<width) return (location.y+(width-x-1),location.x);
			return (location.y,location.x+(x+1-width));
		}
	}

	static (int risk, List<(int y,int x)> path) FindBestPath(int [,] matrix, (int y,int x) src, (int y,int x) dst, uint maxBackSteps=uint.MaxValue)
	{
		int height = matrix.GetLength(0), width = matrix.GetLength(1);
		int lowestRiskSoFar = int.MaxValue, iterationCount = 0, pathCount = 0;
		var shortestPath = new List<(int y,int x)>();

		if (matrix[src.y,src.x] == int.MaxValue) throw new ArgumentException("Invalid source location - a wall !");

		int minimalRislLocation = matrix.Cast<int>().Min();

		//var archives = new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path, (int y, int x) from)>();
		var archives = new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>[4] {
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // right
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // down
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // left
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>()  // up
		};
		return MoveTo(src,-matrix[src.y,src.x],(new List<(int y,int x)>(),(0,0))); // Do not consider the risk of the starting location unless you move there

		(int risk, List<(int y,int x)> path) MoveTo((int y, int x) location, int risk, (List<(int y,int x)> path, (int y,int x) max) path, int direction=0) {
			if (location.y<0 || location.y>=height || location.x<0 || location.x>=width || matrix[location.y,location.x]==int.MaxValue || 
				path.path.Contains((location.y,location.x)) || 
				risk + matrix[dst.y,dst.x] + minimalRislLocation * (Math.Abs(location.y-dst.y)+Math.Abs(location.x-dst.x)-1) > lowestRiskSoFar) return (int.MaxValue,null);

			iterationCount++;
			risk += matrix[location.y,location.x];
			path.path.Add(location);
			path.max.y = Math.Max(path.max.y,location.y);
			path.max.x = Math.Max(path.max.x,location.x);

			var loc = location;
			(int risk, List<(int y,int x)> path) archiveValue = (int.MaxValue,null);
			if (direction>0 && archives[direction-1].TryGetValue(location, out archiveValue)) {
				//if (archiveValue.risk == int.MaxValue) return (int.MaxValue,null);
				if (risk+archiveValue.risk > lowestRiskSoFar) return (int.MaxValue,null);
				risk += archiveValue.risk;
				path.path.AddRange(archiveValue.path);
				//Console.WriteLine($"{location}: {archiveValue.risk}");
				location = dst;
			}

			if (location == dst) {
				pathCount++;
				if (risk <= lowestRiskSoFar) {
					if (risk < lowestRiskSoFar || path.path.Count < shortestPath.Count)  {
						shortestPath = path.path;
						if (archiveValue.risk != int.MaxValue) {
							Console.Beep(); Console.Write($"\n{DateTime.Now:HH\\hmm} {loc} {risk} ({shortestPath.Count}): "); for (int i=0; i<Math.Min(20,path.path.Count) ;i++) Console.Write($"({path.path[i].y},{path.path[i].x}) "); Console.Write('\n');
						}
					}
					lowestRiskSoFar = risk;
					//return (risk,path.path);
				}

				return (risk,path.path);
			}

			//if (risk + matrix[dst.y,dst.x] > lowestRiskSoFar) return (int.MaxValue,null); // no need to go further since we already have found better path than this path segment

			var right = MoveTo((location.y,location.x+1), risk, (new List<(int y,int x)>(path.path),path.max),1);
			if (right.risk != int.MaxValue && right.path[^1]==dst) {
				var segment = right.path.SkipWhile(l => l!=location).Skip(1).ToList();
				int segmentRisk = segment.Sum(l=>matrix[l.y,l.x]);

				if (archives[0].TryGetValue(location, out archiveValue) && (archiveValue.risk != segmentRisk || archiveValue.path.Count != segment.Count)) {
					Console.Write($"{location} ({segmentRisk} was {archiveValue.risk}): "); for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]); Console.Write('\n');
				} else {
					Console.Write($"{location} ({segmentRisk}): "); for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]); Console.Write('\n');
				}

				archives[0][location] = (segmentRisk,segment);
			} //else if (archives[0].TryGetValue(location, out archiveValue)) Console.Write($"{location} (void was {archiveValue.risk})\n"); //archives[0].Remove(location); //] = (int.MaxValue,null);

			var down = MoveTo((location.y+1,location.x), risk, (new List<(int y,int x)>(path.path),path.max),2);

			//if (Math.Max(0,path.max.y-path.path[^1].y) + Math.Max(0,path.max.x-path.path[^1].x) > maxBackSteps) return (int.MaxValue,null);
			var left = MoveTo((location.y,location.x-1), risk, (new List<(int y,int x)>(path.path),path.max),3);
			var up = MoveTo((location.y-1,location.x), risk, (new List<(int y,int x)>(path.path),path.max),4);

			//var min = new (int risk, List<(int y,int x)> path)[] {right,down,left,up}.OrderBy(m => m.risk).First();//.Min();
			var min = new (int risk, List<(int y,int x)> path)[] {right,down,left,up}.MinBy(x => x.risk);

			/*if (min.risk != int.MaxValue) {
				var segment = min.path.SkipWhile(l => l!=location).Skip(1).ToList();
				var fromLoc = min.path[^(segment.Count+1)];
				archives[location] = (min.risk-risk,segment,fromLoc);
				if (location == (3,11) || location == (666,4)) {
					Console.Write($"\nMinimum risk from {location} arriving from ({fromLoc}) ({min.risk-risk}/{lowestRiskSoFar}): ");
					for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]);Console.Write(";");
					for (int i=0; i<min.path.Count ;i++) Console.Write(min.path[i]);
					Console.Write("\n");
				}
			} else {
				if (location == (3,11) && archives.ContainsKey(location) && archives[location].risk!=int.MaxValue) {
					Console.Write($"\nMinimum risk from {location} ({archives[location].risk}/{lowestRiskSoFar}): Voided: ");
					for (int i=0; i<archives[location].path.Count ;i++) Console.Write(archives[location].path[i]);
					Console.Write("\n");
				}
				archives[location] = (int.MaxValue,null,(0,0));
			}*/

			return min;
		}
	}

	static void PrintMatrix(int[,] matrix, int risk, List<(int y,int x)> path, Stopwatch sw)
	{
		for (int y=0; y<matrix.GetLength(0) ;y++) {
			for (int x=0; x<matrix.GetLength(1) ;x++) {
				char v = matrix[y,x]==int.MaxValue ? '#' : (char)(matrix[y,x] + '0');
				if (path.Contains((y,x))) if (Console.IsOutputRedirected) Console.Write('*');
				else {
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write(v);
					Console.ForegroundColor = ConsoleColor.White;
				} else Console.Write(v);
			}
			Console.Write('\n');
		}
		//for (int i=0; i<path.Count ;i++) Console.Write(path[i]);
		string value = $"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {matrix.GetLength(0)} rows per {matrix.GetLength(1)} columns is {sw.Elapsed:hh\\:mm\\:ss\\.fff}; Risk={risk} ({path.Count - 1} moves); srcLoc=({path[0].y},{path[0].x}); dstLoc=({path[^1].y},{path[^1].x})\n";
		Console.WriteLine(value);
	}

	// string str = CsvToInsert("Test2.csv", "dbo.TableA", ',', (x=>x.Replace("\\n","'+CHAR(10)+'")));
	// Example output: "insert into dbo.Mytable(Id,Name,StartDate) select Id,Name,StartDate from (values (1,'Alain','1967-03-13'),(2,'Emie','2008-12-20')) sub (Id,Name,StartDate);"
	/*public static string CsvToInsert(string csvFile, string tableName, char separator=',', Func<string,string> actionOnStringValues=null)
	{
		if (actionOnStringValues is null) actionOnStringValues = (x=>x);

		string[] lines = System.Text.RegularExpressions.Regex.Replace(System.IO.File.ReadAllText(csvFile), @"\r\n|\n\r|\r", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		//lines = lines.Where(r => r[0] != '#').ToArray();
		//if (lines.Length < 2) return "";

		var fields = new List<string>();
		foreach(string fieldName in lines[0].Split(separator)) fields.Add(fieldName.Trim());
		string fieldsStr = string.Join(',',fields.ToArray());

		var data = new List<string>();
		for(int r=1; r<lines.Length ;r++) {
			if (lines[r][0]=='#') continue; // ignore lines starting with #
			string[] values = lines[r].Split(separator,StringSplitOptions.TrimEntries);
			if (values.Length < fields.Count) throw new Exception($"CsvToInsert(\"{csvFile}\"): Invalid input file at line {r+1}"); // Checking for less than since trailing may add extra empty fields
		
			var subValues = new List<string>();
			foreach(string value in values) {
				if (value.Length==0 || value.ToUpper()=="DEFAULT") subValues.Add("DEFAULT");
				else if (value.ToUpper()=="NULL") subValues.Add("NULL");
				else if (value[0] == '\'' && value[^1] == '\'') subValues.Add('\''+actionOnStringValues(value.Substring(1,value.Length-2))+'\'');
				else if (value[0] == '\'' || value[^1] == '\'') throw new Exception($"CsvToInsert(\"{csvFile}\"): Invalid input file at line {r+1}");
				else if (int.TryParse(value,out int ii)) subValues.Add(value);
				else if (double.TryParse(value,out double dd)) subValues.Add(value);
				else subValues.Add('\''+actionOnStringValues(value)+'\'');
			}

			data.Add("\n  ("+string.Join(',',subValues.ToArray())+')');
		}
		string dataStr = string.Join(',',data.ToArray());

		var dataRows = new List<string>();
		string statement = $"insert into {tableName} ({fieldsStr}) select {fieldsStr} from (values {dataStr}\n) sub ({fieldsStr});";

		return statement;
	}*/
}
