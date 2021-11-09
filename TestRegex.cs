using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

using Alemvik;

namespace TestRegex {
	static class Tester {
		public static void Go()
		{
			var msg = "TestRegex";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			string original = "a{a}a";
			string transformed = TransformPattern(original);

			string str = "a bbb  a";
			Console.WriteLine($"{original}\n=>\n{transformed}\n\n{str} : {Fits(str,transformed)}");
		}

		public static string TransformPattern(string pattern_a)
		{
			string transformed = Regex.Replace(pattern_a, @"\r\n|\n\r|\r", "\n");
			var lines = transformed.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int l=0; l<lines.Length ;l++) {
				lines[l] = lines[l].Trim();
				var sb = new StringBuilder();
				bool copying = true;
				for (int c=0; c<lines[l].Length ;c++) { 
					if (copying) { // just check *, {
						if (lines[l][c] == '∙') sb.Append('☥'); // ∙ => ☥ since ∙ means at least one characters
						else {
							sb.Append(lines[l][c]);
							if (c<lines[l].Length-1 && lines[l][c] == '}' && lines[l][c+1] == '}') c++; // }} escapes }
							else if (lines[l][c] == '{') {
								if (c<lines[l].Length-1 && lines[l][c+1] == '{') c++; // {{ escapes {
								else if (lines[l].IndexOf('}',c) > 0) copying=false; // When a matching } is found, {content} is replaced by *
							}
						}
					} else if (lines[l][c] == '}') {
						sb[sb.Length-1] = '∙'; // replaces { with ∙
						copying=true;
					}
				}

				lines[l] = sb.ToString();
			}

			return string.Join('\n', lines);
		}

		public static bool Fits(string str_a, string pattern_a)
		{
			str_a = str_a.Replace('∙', '☥').Replace("{", "").Replace("}", ""); // pattern_a also has that first replacement
			str_a = Regex.Replace(str_a, @"\r\n|\n\r|\r", "\n"); // pattern_a also has this replacement
			var strLines = str_a.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			var patLines = pattern_a.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			if (strLines.Length != patLines.Length) return false;

			for (int l=0; l<strLines.Length ;l++) {
				strLines[l] = strLines[l].Trim();
				int a = patLines[l].IndexOf('{');
				if (a >= 0) {
					int b = patLines[l].IndexOf('}', a);
					if (b>0) {
						string newPattern = patLines[l].Substring(0,a) + patLines[l].Substring(b+1,patLines[l].Length-b-1);
						string pat = '^' + Regex.Escape(newPattern).Replace("∙", @"\s*\S+.*") + '$';
						if (new Regex(pat, RegexOptions.IgnoreCase).IsMatch(strLines[l])) continue;
                  patLines[l] = patLines[l].Replace("{","").Replace("}", "");
					}
				}
				string effPat = '^' + Regex.Escape(patLines[l]).Replace("∙", @"\s*\S+.*") + '$';
				if (!new Regex(effPat, RegexOptions.IgnoreCase).IsMatch(strLines[l])) return false;
			}
			return true;
		}
	}
}
