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
	% dotnet nuget list source
	% dotnet new console -n TestAsync ; It will create a TestAsync folder (to delete it: %rm -rf TestAsync)
	% dotnet restore ; Useful to do after adding packages in the Test.csproj file
	% dotnet run -c debug ; or: dotnet run -c release
	% dotnet build
	% dotnet run TestAsync.dll 

Usefull VsCode extensions: .NET Core Test Explorer, Auto Rename Tag, C#, Code Runner, Debugger for Chrome, HTML CSS Support, HTML Preview, .Net Core Tools, NuGet Package Manager, Subtitles Editor, Thunder Client, vscode-icons, XML Tools

To test this, just comment in/out the throw lines from the three task members.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

//using Emvie;
using static Emvie.Convertion; // To get the DataTable ToCsv extension
//using cnv = Emvie.Convertion;

namespace Test {
	public class ProductOwner {
		[Required]
		public string Name { get; set; }

		public DateTimeOffset StartDate { get; set; }

		[Range(5, 15)]
		public int Id { get; set; }
		// Lambda expression (uses =>)
		public override string ToString() => $"Owner's name is {Name}, his id is {Id} and his start date is {StartDate.ToString("yyyy-MM-dd")}\n";
	}

	public class Program {
		public static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		static async Task Main(string[] args)
		{
			var po = config.GetSection("ProductOwner").Get<ProductOwner>();

			if (!Validator.TryValidateObject(po, new ValidationContext(po), new List<ValidationResult>(), true))
				throw new Exception("Unable to find all settings");

			Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), config.GetValue<string>("ConsoleForegroundColor"), true);
			Console.WriteLine($"Config: Console color is {config.GetValue<string>("ConsoleForegroundColor")}; {po}");

			//TestAsync.Tester.Go();
			//TestMisc.Tester.Go();
			//TestJson.Tester.Go(false);
			//TestLinq.Tester.Go();
			//TestExtension.Tester.Go();
			TestSpan.Tester.Go();
			TestStream.Tester.Go();
			await TestDynamicType.Tester.Go("Alemvik" /*"ElfoCrash"*/);
			//TestCsv();
		}

		static void TestCsv()
		{
			Console.WriteLine($"\n--- TestCsv {new String('-', 50)}\n");

			DataTable tbl;
			using (var sr = new StreamReader("Test.csv")) tbl = ConvertCsvToDataTable(sr, new string[] { "first", "last", "birth date" });
			// var ms = new MemoryStream();
			// using (FileStream fs = new FileStream("Test.csv", FileMode.Open, FileAccess.Read)) fs.CopyTo(ms);
			// ms.Seek(0, SeekOrigin.Begin);
			// using (var sr = new StreamReader(ms)) tbl = ConvertCsvToDataTable(sr, new string[] { "first", "last", "birth date" });

			foreach (DataColumn c in tbl.Columns) Console.Write($"{c.ColumnName,-20}"); Console.Write($"\n{new String('-', 20 * tbl.Columns.Count)}\n");
			foreach (DataRow r in tbl.Rows) {
				foreach (DataColumn c in tbl.Columns) Console.Write($"{r[c.Ordinal],-20}");
				Console.Write("\n");
			}
			Console.WriteLine($"tbl.Columns.Count = {tbl.Columns.Count}, tbl.Rows.Count = {tbl.Rows.Count}");
			string csv = tbl.ToCsv();

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write($"\n---\n{csv}\n---\n");
			Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), config.GetValue<string>("ConsoleForegroundColor"), true);
		}
	}
}