using System;
using System.Text.RegularExpressions;

namespace TestExtension { // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
	static class Tester {
		public static void Go()
		{
			Console.WriteLine($"\n--- TestExtension {new String('-', 50)}\n");

			Console.WriteLine("abc.def".GlobFits("?bc*")); // starts with any one letter then bc
			Console.WriteLine("abc.def".GlobFits("*ef")); // ends with ef
			Console.WriteLine(GlobFits("abc.def", "*abc*")); // contains abc
			Console.WriteLine(GlobFits("ab...ab..ab", "*ab*ab*ab")); // contains ab twice
			Console.WriteLine(GlobFits(System.IO.Path.GetFileName("c:/abdef/123456"), "*????*")); // at least 4 characters
			Console.WriteLine(System.IO.Path.GetFileName(@"c:/abdef/123456"));
			Console.WriteLine(System.IO.Path.GetFileName(@"c://abdef/123456"));
		}

		// Here globPattern_a is simplified as: ? matches a single character, * matched none or any chartacters. Examples:
		// "?bc*"   : starts with any one letter then "bc"
		// "*ef"    : ends with "ef"
		// "*abc*"  : contains "abc"
		// "*ab*ab*ab": contains "ab" twice and ends with "ab" (three times "ab" in all)
		// "*????*" : has at least 4 characters
		// Typical usage: System.IO.Path.GetFileName("c:/abdef/123456").GlobFits("*????*");
		// Extensions must be defined in non-generic static classes.  In C#, generic means not specific to a particular data type.
		public static bool GlobFits(this string filename_a, string globPattern_a)
		{ // https://en.wikipedia.org/wiki/Glob_(programming)
			var mask = "^" + Regex.Escape(globPattern_a).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
			return new Regex(mask, RegexOptions.IgnoreCase).IsMatch(filename_a);
		}
	}
}