﻿/*
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
using System.Diagnostics;
using System.IO;
using System.Linq;
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
	public override string ToString() => $"Owner's name is {Name}, his id is {Id} and his start date is {StartDate.ToString("yyyy-MM-dd")}\n";
}

public unsafe class Program {
	public static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
	static async Task Main(string[] args)
	{
		if (args.Length > 0) {
			int start=0;
			if (args.Length > 1) int.TryParse(args[1], out start);

			if (int.TryParse(args[0], out int size)) {
				var matrixRandom = new int[size,size];
				Random rnd = new Random();
				for (int y=0; y<size ;y++) for (int x=0; x<size ;x++) matrixRandom[y,x] = rnd.Next(10); // random int betwwen 0 inclusively and 9 inclusively
				AdventOfCode2021_15A(matrixRandom,start);
			} else {
				var lines = System.IO.File.ReadAllText(args[0]).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				var matrix = new int[lines.Length,lines.Length];
				fixed (int* p = matrix) new Span<int>(p, matrix.Length).Fill(100); // requires to be inside unsafe methods
				for (int y=0; y<lines.Length ;y++) for (int x=0; x<lines.Length ;x++) if (lines[y][x]>='0' && lines[y][x]<='9') matrix[y,x] = lines[y][x] - '0';
				AdventOfCode2021_15A(matrix,start);
			}
			return;
		}
	
		InitDatabase();

		// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#getvalue
		var po = config.GetSection("ProductOwner").Get<ProductOwner>();
		//var po = config.GetValue<ProductOwner>("ProductOwner");
		if (!Validator.TryValidateObject(po,new ValidationContext(po),new List<ValidationResult>(),true)) throw new Exception("Unable to find all settings");
		Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor),config.GetValue<string>("ConsoleForegroundColor"),true);
		Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH\\hmm}: Config: Console color is {config.GetValue<string>("ConsoleForegroundColor")}\nProductOwner is {po}");

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

		{
			var msg = "Test Dictionary to list of System.ValueTuple";
			Console.WriteLine($"\n--- {msg} {new String('-',Math.Max(65 - msg.Length,3))}\n");
			List<(long a, int b)> tuples = (new Dictionary<long,int>() { { 1L,1 },{ 2L,2 } }).Select(x => (x.Key, x.Value)).ToList();
			foreach (var tuple in tuples) Console.WriteLine($"({tuple.a},{tuple.b})");
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

		// *** The other tests ***
		//TestRegex.Tester.Go();
		//TestAsync.Tester.Go(".");
		//TestDatabase.Tester.Go();
		//TestMisc.Tester.Go();
		//TestJson.Tester.Go(false);
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
		var api = new MinimalApi(); // https://localhost:5501/donut
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

	public static void DiceProbabilities()
	{
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
	}

	/*public static unsafe void FillMultiDimArray<T>(this Array array, T value)
	{
		fixed (int* a = Array.Fill<int>() {
			var span = new Span<T>(array, array.Length);
			span.Fill(value);
		}
	}*/

	static unsafe void AdventOfCode2021_15A(int[,] matrix, int start=0)
	{
		int wh = matrix.GetLength(0);
		if (wh != matrix.GetLength(1)) throw new ArgumentException("matrix must be square");
		if (!start.Between(-wh+1,wh-1)) throw new ArgumentException($"start must be between {-wh+1} inclusively and {wh-1} inclusively");

		var sw = Stopwatch.StartNew();
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
					var path = new List<(int y, int x)>();
					path.Add(Location(outterPerimeterLocation,outterPerimeterWidth,o));

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

			innerPerimeterIx ^= 1;
			outterPerimeterIx ^= 1;
		}

		var startIx = start<0 ? wh+start-1 : wh+start-1;
		for (int y=0; y<wh ;y++) {
			for (int x=0; x<wh ;x++) if (matrix[y,x]==100) Console.Write("￭"); else if (perimeters[innerPerimeterIx,startIx].path.Contains((y,x))) Console.Write("∙"); else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}
		Console.WriteLine($"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {wh} x {wh} is {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}; LowestRisk={perimeters[innerPerimeterIx,startIx].risk-perimeters[outterCopyIx,startIx].risk} ({perimeters[innerPerimeterIx,startIx].path.Count-1} moves); start={start}\n");

		// for (int i=0; i<wh ;i++) {
		// 	Console.Write();
		// }

		(int y,int x) Location((int y,int x) location, int width, int x) {
			if (x<width) return (location.y+(width-x-1),location.x);
			return (location.y,location.x+(x+1-width));
		}
	}

	static void AdventOfCode2021_15Ab(string file)
	{
		var lines = System.IO.File.ReadAllText(file).Split('\n');

		int height = lines.Length;
		int width = lines[0].Length;

		var matrix = new int[height,width];
		for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix[y,x] = lines[y][x]>='0' && lines[y][x]<='9' ? lines[y][x] - '0' : -1;

		for (int y=0; y<height ;y++) {
			for (int x=0; x<width ;x++) if (matrix[y,x]==-1) Console.Write("￭"); else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}

		var result = FindBestPath(matrix, (0,0), (height-1,width-1));
	}

	static (int risk, List<(int y,int x)> path) FindBestPath(int [,] matrix, (int y,int x) begin, (int y,int x) end)
	{
		int height = matrix.GetLength(0), width = matrix.GetLength(1);
		int lowestRisk = int.MaxValue, iterationCount = 0, pathCount = 0;
		var shortestPath = new List<(int y,int x)>();
		var sw = Stopwatch.StartNew();

		MoveTo(begin,0,new List<(int y,int x)>());

		Console.WriteLine($"\nDuration for {height} x {width} is {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}; LowestRisk={lowestRisk} ({shortestPath.Count}); iterationCount={iterationCount}; pathCount={pathCount}");
		for (int y=0; y<height ;y++) {
			for (int x=0; x<width ;x++) if (matrix[y,x]==-1) Console.Write("￭"); else if (shortestPath.Contains((y,x))) Console.Write("∙"); else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}

		return (lowestRisk,shortestPath);

		void MoveTo((int y, int x) location, int risk, List<(int y,int x)> path) {
			if (location.y<0 || location.y>=height || location.x<0 || location.x>=width || matrix[location.y,location.x]==-1 || path.Contains((location.y,location.x))) return;

			iterationCount++;
			risk += matrix[location.y,location.x];
			path.Add(location);

			/*Console.Write('\n');
			for (int y=0; y<height ;y++) {
				for (int x=0; x<width ;x++) if (matrix[y,x]==-1) Console.Write("￭"); else if (p.Contains((y,x))) Console.Write("∙"); else Console.Write($"{matrix[y,x],1}");
				Console.Write('\n');
			}
			Console.ReadLine();*/

			if (location == end) {
				pathCount++;
				if (risk <= lowestRisk) {
					if (risk < lowestRisk || path.Count < shortestPath.Count)  {
						shortestPath = path;
						Console.Write($"\n{DateTime.Now:HH\\hmm} ({sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}) {risk} ({path.Count}) "); //foreach (var v in p) Console.Write($"({v.y},{v.x})"); Console.Write('\n');
						Console.Beep();
					}
					lowestRisk = risk;
				}
				return;
			}

			if (risk + matrix[end.y,end.x] > lowestRisk) return; // no need to go further since we already have found better path than this path segment

			MoveTo((location.y,location.x+1), risk, new List<(int y,int x)>(path));
			MoveTo((location.y+1,location.x), risk, new List<(int y,int x)>(path));
			MoveTo((location.y,location.x-1), risk, new List<(int y,int x)>(path));
			MoveTo((location.y-1,location.x), risk, new List<(int y,int x)>(path));
		}
	}
}
