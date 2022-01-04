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

public class Program {
	public static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
	static unsafe async Task Main(string[] args)
	{
		if (args.Length > 0) {
			int start=0;
			if (args.Length > 1) int.TryParse(args[1], out start);

			int[,] matrix;

A:			if (int.TryParse(args[0], out int size)) {
				matrix = new int[size,size];
				Random rnd = new Random();
				for (int y=0; y<size ;y++) for (int x=0; x<size ;x++) matrix[y,x] = rnd.Next(10); // random int betwwen 0 inclusively and 9 inclusively
			} else {
				var lines = System.IO.File.ReadAllText(args[0]).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				matrix = new int[lines.Length,lines.Length];
				fixed (int* p = matrix) new Span<int>(p, matrix.Length).Fill(int.MaxValue); // requires to be inside unsafe methods
				for (int y=0; y<lines.Length ;y++) for (int x=0; x<lines.Length ;x++) if (lines[y][x]>='0' && lines[y][x]<='9') matrix[y,x] = lines[y][x] - '0';

				var matrix5 = new int[lines.Length*5,lines.Length*5];
				for (int y=0; y<lines.Length ;y++) for (int x=0; x<lines.Length ;x++) matrix5[y,x] = matrix[y,x];
				for (int i=0; i<4 ;i++) for (int y=0; y<lines.Length ;y++) for (int x=0; x<lines.Length ;x++) matrix5[(i+1)*lines.Length+y,x] = matrix5[i*lines.Length+y,x] + 1  == 10 ? 1 : matrix5[i*lines.Length+y,x] + 1;
				for (int i=0; i<5 ;i++) for (int j=0; j<4 ;j++) for (int y=0; y<lines.Length ;y++) for (int x=0; x<lines.Length ;x++) matrix5[i*lines.Length+y,(j+1)*lines.Length+x] = matrix5[i*lines.Length+y,j*lines.Length+x] + 1 == 10 ? 1 : matrix5[i*lines.Length+y,j*lines.Length+x] + 1;
				//matrix = matrix5;
				/*for (int y=0; y<lines.Length*5 ;y++) {
					for (int x=0; x<lines.Length*5 ;x++) Console.Write(matrix5[y,x]);
					Console.Write('\n');
				}*/
			}

			var sw2 = Stopwatch.StartNew();
			var (risk2, path2) = FindBestPath2(matrix,start);
			sw2.Stop();
			Console.Write('\n');PrintMatrix(matrix,risk2,path2,sw2);

			(int y,int x) srcLoc = (0,0);
			(int y,int x) dstLoc = (matrix.GetLength(0)-1,matrix.GetLength(0)-1);
			var sw1 = Stopwatch.StartNew();
			var (risk, path) = FindBestPath(matrix, srcLoc, dstLoc, 3);

			//if (int.TryParse(args[0], out int ghr) && (risk2 - risk < 3 /*|| path.Count == path2.Count*/)) goto A;

			Console.Write('\n'); PrintMatrix(matrix,risk,path,sw1);

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
		TestLinq.Tester.Go();
		//TestExtension.Tester.Go();
		//TestSpan.Tester.Go();
		//TestStream.Tester.Go();
		//await TestDynamicType.Tester.Go("Alemvik" /*"ElfoCrash"*/);
		//TestCsv();
		//TestXquery.Tester.Go();
		//TestComposition.Tester.Go();
		//TestDeconstruction.Tester.Go();
		//Console.WriteLine(DateTime.Now.Date); // How to have it is OS default format ?
		//var api = new MinimalApi(); // https://localhost:5501/donut
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

	static (int risk, List<(int y,int x)> path) FindBestPath2(int[,] matrix, int start=0)
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

		(int y,int x) Location((int y,int x) location, int width, int x) {
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

		MoveTo(src,-matrix[src.y,src.x],(new List<(int y,int x)>(),(0,0))); // Do not consider the risk of the starting location unless you move there

		return (lowestRiskSoFar,shortestPath);

		void MoveTo((int y, int x) location, int risk, (List<(int y,int x)> path, (int y,int x) max) path) {
			if (location.y<0 || location.y>=height || location.x<0 || location.x>=width || matrix[location.y,location.x]==int.MaxValue || 
				path.path.Contains((location.y,location.x)) || 
				risk + matrix[dst.y,dst.x] + minimalRislLocation * (Math.Abs(location.y-dst.y)+Math.Abs(location.x-dst.x)-1) > lowestRiskSoFar) return;

			iterationCount++;
			risk += matrix[location.y,location.x];
			path.path.Add(location);
			path.max.y = Math.Max(path.max.y,location.y);
			path.max.x = Math.Max(path.max.x,location.x);

			if (location == dst) {
				pathCount++;
				if (risk <= lowestRiskSoFar) {
					if (risk < lowestRiskSoFar || path.path.Count < shortestPath.Count)  {
						shortestPath = path.path;
						Console.Beep(); Console.Write($"\n{DateTime.Now:HH\\hmm} {risk} ({shortestPath.Count})"); for (int i=0; i<=Math.Min(10,path.path.Count) ;i++) Console.Write($"({path.path[i].y},{path.path[i].x}) "); Console.Write('\n');
					}
					lowestRiskSoFar = risk;
				}
				return;
			}

			if (risk + matrix[dst.y,dst.x] > lowestRiskSoFar) return; // no need to go further since we already have found better path than this path segment

			MoveTo((location.y,location.x+1), risk, (new List<(int y,int x)>(path.path),path.max));
			MoveTo((location.y+1,location.x), risk, (new List<(int y,int x)>(path.path),path.max));

			//if (path.path.Count>=maxBackSteps+1) 	
					//for (int i=0; i<path.path.Count ;i++) Console.Write(path.path[i]); Console.Write("\n\n");
	//Console.WriteLine($"{path.Count} ({maxBackSteps}): ({path[^(maxBackSteps+1)].y},{path[^(maxBackSteps+1)].x}) ({path[^(maxBackSteps+0)].y},{path[^(maxBackSteps+0)].x}) ({location.y},{location.x}) {Math.Max(0,path[^(maxBackSteps+1)].y-location.y) + Math.Max(0,path[^(maxBackSteps+1)].x-location.x)}");
			if (Math.Max(0,path.max.y-path.path[^1].y) + Math.Max(0,path.max.x-path.path[^1].x) >= maxBackSteps) return;

			MoveTo((location.y,location.x-1), risk, (new List<(int y,int x)>(path.path),path.max));
			MoveTo((location.y-1,location.x), risk, (new List<(int y,int x)>(path.path),path.max));

			bool print = false;
			Console.Write($"[{lowestRiskSoFar}] ");
			for (int i=0; i<shortestPath.Count ;i++) if (print || shortestPath[i]==location) {
				Console.Write(shortestPath[i]);
				print = true;
			} Console.Write("\n\n");
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
		for (int i=0; i<path.Count ;i++) Console.Write(path[i]);
		Console.WriteLine($"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {matrix.GetLength(0)} rows per {matrix.GetLength(1)} columns is {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}; Risk={risk} ({path.Count-1} moves); srcLoc=({path[0].y},{path[0].x}); dstLoc=({path[path.Count-1].y},{path[path.Count-1].x})\n");
	}
}
