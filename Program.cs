﻿/*
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

Usefull VsCode extensions (I only use the C# extension): Auto Rename Tag, C#, Code Runner, Debugger for Chrome, HTML CSS Support, HTML Preview, .Net Core Tools, NuGet Package Manager, Subtitles Editor, Thunder Client, vscode-icons, XML Tools

To test this, just comment in/out the throw lines from the three task members.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
			await TestDynamicType.Tester.Go("ElfoCrash");
			await TestDynamicType.Tester.Go("Alemvik");

			var tbl = ConvertCSVtoDataTable("Test.csv");
			foreach (DataColumn c in tbl.Columns) Console.Write($"{c.ColumnName,-20}"); Console.Write($"\n{new String('-', 20 * tbl.Columns.Count)}\n");
			foreach (DataRow r in tbl.Rows) {
				foreach (DataColumn c in tbl.Columns) Console.Write($"{r[c.Ordinal],-20}");
				Console.Write("\n");
			}
		}
		public static DataTable ConvertCSVtoDataTable(string filePath_a, char unusedChar_a = '∙')
		{
			//using (var sr = new StreamReader(filePath_a)) return ConvertCSVtoDataTable(sr, null, unusedChar_a);

			MemoryStream ms = new MemoryStream();
			using (FileStream fs = new FileStream(filePath_a, FileMode.Open, FileAccess.Read)) fs.CopyTo(ms);
			ms.Seek(0, SeekOrigin.Begin);
			var sr = new StreamReader(ms);
			return ConvertCSVtoDataTable(sr, new string[] { "first", "birth date" }, unusedChar_a);
		}

		// Call it using: using (var sr = new StreamReader(filePath_a)) return ConvertCSVtoDataTable(sr, null, unusedChar_a);
		public static DataTable ConvertCSVtoDataTable(StreamReader sr_a, IEnumerable<string> columns_a = null, char unusedChar_a = '∙')
		{
			//StreamReader sr = new StreamReader(filePath_a);
			string[] headers = FixLine(sr_a.ReadLine(), unusedChar_a).Split(unusedChar_a);
			DataTable dt = new DataTable();
			foreach (string header in headers) dt.Columns.Add(header.Trim());

			while (!sr_a.EndOfStream) {
				string[] rows = FixLine(sr_a.ReadLine(), unusedChar_a).Split(unusedChar_a);
				if (rows.Length != dt.Columns.Count) continue;
				DataRow dr = dt.NewRow();
				for (int i = 0; i < headers.Length; i++) {
					if (columns_a is null || columns_a.Contains(dt.Columns[i].ColumnName.ToLower())) dr[i] = rows[i].Trim();
				}
				dt.Rows.Add(dr);
			}

			if (columns_a is not null)
				for (int i = dt.Columns.Count - 1; i >= 0; i--)
					if (!columns_a.Contains(dt.Columns[i].ColumnName.ToLower()))
						dt.Columns.RemoveAt(i);

			return dt;

			static string FixLine(string line_a, char newSeparator_a = '∙')
			{
				var sb = new StringBuilder();
				bool escaping = false;
				for (int i = 0; i < line_a.Length; i++) {
					if (line_a[i] == ',') sb.Append(escaping ? ',' : newSeparator_a);
					else if (line_a[i] == '"') {
						if (escaping) {
							if (i < line_a.Length - 1 && line_a[i + 1] == '"') sb.Append(line_a[i++]);
							else escaping = false;
						} else {
							if (i == 0 || line_a[i - 1] == ',') escaping = true;
							else sb.Append('"');
						}
					} else sb.Append(line_a[i]);
				}
				return sb.ToString();
			}
		}
	}
}