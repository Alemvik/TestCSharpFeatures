using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Alemvik;

// Basicly, spans point to data and they have a length e.g a span may be a pointer to 
// the middle two letters of this string "111AA111". Good practice would be to return a span
// instean of a string, allowing the caller to make a string out of it if needed.
namespace TestSpan; // https://www.youtube.com/watch?v=FM5dpxJMULY

static class Tester {
	public static unsafe void Go()
	{
		var msg = "TestSpan";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

		var dateParts = GetDateComponents(DateTime.Now.ToString("yyyy-MM-dd"));
		Console.WriteLine($"year = {dateParts.year}, month={dateParts.month:00}, day={dateParts.day:00}");
		string sentence = "\taaa\tbbb ccc ddd\t    eee  ";
		Console.WriteLine($"\"{sentence}\"'s middle word is \"{sentence.MiddleWord().ToString()}\"");

		var matrix = new int[6,7];
		//for (int y=0; y<wh ;y++) for (int x=0; x<wh ;x++) matrix[y,x] = -1;
		fixed (int* p = matrix) new Span<int>(p, matrix.Length).Fill(1); // requires to be inside unsafe methods
		for (int y=0; y<matrix.GetLength(0) ;y++) {
			for (int x=0; x<matrix.GetLength(1) ;x++) if (matrix[y,x]==-1) Console.Write("￭"); else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}
	}
	static (int year, int month, int day) GetDateComponents(string sdate_a)
	{
		ReadOnlySpan<char> span = sdate_a;
		return (int.Parse(span.Slice(0, 4)), int.Parse(span.Slice(5, 2)), int.Parse(span.Slice(8)));
	}
	static ReadOnlySpan<char> MiddleWord(this string sentence_a)
	{
		ReadOnlySpan<char> span = sentence_a.Cleanup(); // trim then remove extra spaces

		int spaceCount = 0;
		foreach (char c in span) if (c == ' ') spaceCount++;

		if (spaceCount > 0) {
			spaceCount >>= 1; // divide it by two by shifting it by one
			for (int i = 0; i < span.Length; i++) if (span[i] == ' ' && (--spaceCount <= 0)) {
					if (spaceCount < 0) return span.Slice(0, i);
					for (int j = i + 1; j < span.Length; j++) if (span[j] == ' ') return span.Slice(i + 1, j - i - 1);
				}
		}

		return span;
	}
}
