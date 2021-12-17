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

			string originalPat = @"
			ccc
			bbb{{o{}o}}...
			a{a}a... 
			ccc";
			string transformedPat = TransformPattern(originalPat);

			string str = @"
			ccc
			bbb
			bbboiiio
			a b a
			a   bb  \ta
			aba
			ccc";
			Console.WriteLine($"originalPat=\n{originalPat}\n\ntransformedPat=\n{transformedPat}\n\nstr=\n{str}\n{Fits(transformedPat,str)}");
		}

		public static string TransformPattern(string pattern_a)
		{
			string transformed = Regex.Replace(pattern_a, @"\r\n|\n\r|\r", "\n");
			transformed = Regex.Replace(transformed,@"[ \t]+"," "); // spaces (space and tab combinations) => one space (cannot use /s since it includes newlines)
			var lines = transformed.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			for (int l=0; l<lines.Length ;l++) {
				lines[l] = lines[l].Trim();
				var sb = new StringBuilder();
				bool copying = true;
				for (int c=0; c<lines[l].Length ;c++) { 
					if (copying) { // just check ∙, {
						if (lines[l][c] == '∙') sb.Append('☥'); // ∙ => ☥ since ∙ means at least one non space character
						else {
							sb.Append(lines[l][c]);
							if (lines[l][c] == '{') {
								if (c<lines[l].Length-1 && lines[l][c+1] == '{') c++; // {{ => {
								else if (lines[l].IndexOf('}',c) > 0) copying=false; // When a matching } is found, {content} is replaced by ∙
								else throw new Exception("Uneven sub expression braces (no })");
							} else if (lines[l][c] == '}') {
								if (c<lines[l].Length-1 && lines[l][c+1] == '}') c++; // }} => }
								else throw new Exception("Uneven sub expression braces (no {)");
							}
						}
					} else if (lines[l][c] == '}') {
						sb[sb.Length-1] = '∙'; // replaces { with ∙
						copying=true;
					}
				}

				lines[l] = sb.ToString();

				// Ony one sub pattern i.e. "{{ }}" is allowed
				bool lookingForBegin = true;
				for (int c=0; c<lines[l].Length ;c++) {
					if (lookingForBegin) {
						if (lines[l][c]=='}') throw new Exception("Uneven sub expression braces A");
						if (lines[l][c]=='{') lookingForBegin=false;
					} else {
						if (lines[l][c]=='{') throw new Exception("Uneven sub expression braces B");
						if (lines[l][c]=='}') {
							lookingForBegin = true;
							for (int d=c+1; d<lines[l].Length ;d++) if (lines[l][d]=='{' || lines[l][d]=='}') throw new Exception("Only one sub expression is allowed");
						}
					}
				}
				if (!lookingForBegin) throw new Exception("Uneven sub expression braces C");
			}

			return string.Join('\n', lines);
		}

		// Do the fit line by lines.
		public static bool Fits(string pattern_a, string str_a)
		{
			str_a = Regex.Replace(str_a, @"\r\n|\n\r|\r", "\n"); // pattern_a also has this replacement
			str_a = Regex.Replace(str_a.Replace('∙', '☥').Replace("{", "").Replace("}", ""),@"[ \t]+"," "); // pattern_a also has that first replacement
			pattern_a = Regex.Replace(pattern_a,@"[ \t]+"," "); // replace multiple spaces (including tabs) by a single space. \s inclunes \n
			
			var strLines = str_a.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			var patLines = pattern_a.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			if (strLines.Length < patLines.Length) return false;

			//Console.WriteLine($"\npattern_a=\n{pattern_a}\nstr_a=\n{str_a}\n");
			int l,p;
			for (l=0, p=0; l<strLines.Length ;l++, p++) {
				strLines[l] = strLines[l].Trim();

				if (p == patLines.Length) {
					if (patLines[--p].EndsWith("...")) {
						string eff = patLines[p].Remove(patLines[p].Length-3,3);
						if (!LineFits(eff,strLines[l])) return false;
					} else return false;
				} else {
					patLines[p] = patLines[p].Trim();
					bool prePatIsRepeat = p>0 && patLines[p-1].EndsWith("...");
					if (!LineFits(patLines[p],strLines[l]) && (p==0 || l==0 || !prePatIsRepeat || !LineFits(patLines[--p],strLines[l]))) return false;
				}
			}

			return p == patLines.Length;

			static bool LineFits(string pat, string str) {
				if (pat.EndsWith("...")) pat = pat.Remove(pat.Length-3,3);

				string eff = pat;
				//Console.WriteLine($"\neff={eff}\nstr={str}");
				int a = pat.IndexOf('{');
				if (a >= 0) {
					int b = pat.IndexOf('}', a);
					if (b>0) {
						string newPattern = (pat.Substring(0,a) + pat.Substring(b+1,pat.Length-b-1)).Trim();
						newPattern = Regex.Replace(newPattern,@"[ \t]"," ");
						newPattern = '^' + Regex.Escape(newPattern).Replace("∙", @"\s*\S+.*") + '$';
						if (new Regex(newPattern, RegexOptions.IgnoreCase).IsMatch(str)) return true;
						eff = pat.Replace("{","").Replace("}", "");
					}
				}
				return new Regex('^' + Regex.Escape(eff).Replace("∙", @"\s*\S+.*") + '$', RegexOptions.IgnoreCase).IsMatch(str);
			}
		}
	}
}
