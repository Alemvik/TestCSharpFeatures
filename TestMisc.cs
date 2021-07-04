// https://channel9.msdn.com/Blogs/dotnet/Get-started-VSCode-Csharp-NET-Core-Windows
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TestMisc {
	public class SurveyQuestion {
		public string Question { get; init; }
		public string Hint { get; init; }
		public string Answer { get; set; } // about Nullable in Test.csproj:  https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references

		public SurveyQuestion(string question, string hint = "") => (Question, Hint) = (question, hint);

		public override string ToString() {
			if (Hint.Length > 0) return $"{Question} (hint: {Hint})";
			return Question;
		}
	}

	struct Point {
		public double x { get; init; } // When the init keyword is used, it restricts a property to only being set by a Constructor or during Nested Object Creation. After the object is created, the property becomes immutable.
		public double y { get; init; }
		public double z { get; init; }
	}
	class Tester {
		public static void Go() {
			{  // Anonymous type feature
				var person = new { Name = "Alice", Age = 25 };
				Console.WriteLine($"The person is called {person.Name} and is {person.Age} years old.\n");
			}

			{ // Tuple type:  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/value-tuples
				(int, string, string) person = (1967, "Alain", "Trépanier");
				Console.WriteLine($"person = {person}; person.Firstname = {person.Item2}\n");
			}

			var surveyQuestions = new List<SurveyQuestion>();
			surveyQuestions.AddRange(new[] {
				new SurveyQuestion("What's your fave IDE ?"),
				new SurveyQuestion("What is the best programming language ?","see how sharp it is")
			});
			surveyQuestions[1].Answer = "C# of course !";

			foreach (var q in surveyQuestions) Console.WriteLine(q + ": " + (q.Answer ?? "<Not answered yet>"));

			var point = new Point { x = 2, y = 3, z = 4 };
			Console.WriteLine($"\nthe size of a byte is {sizeof(byte)}");
			DisplaySizeof<bool>();
			DisplaySizeof<byte>();
			DisplaySizeof<short>();
			DisplaySizeof<int>();
			DisplaySizeof<long>();
			DisplaySizeof<float>();
			DisplaySizeof<double>();
			DisplaySizeof<decimal>();
			DisplaySizeof<Point>();


			int[][,] jArray = new int[2][,];
			jArray[0] = new int[3, 2] { { 1, 2 }, { 3, 4 }, { 5, 6 } };
			jArray[1] = new int[2, 2] { { 7, 8 }, { 9, 10 } };
			Console.WriteLine($"jArray[0][0,0] = {jArray[0][0, 0]}; jArray[0][1,1] = {jArray[0][1, 1]}; jArray[0][^1,^1] = {jArray[^1][1, 1]}");

			Span<int> numbers = stackalloc int[] { 1, 2, 3 }; // stack has performance gain over the heap. Stack also doesn't require the garbage collector to be freed

			//Console.ReadLine();

			var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			nfi.NumberGroupSeparator = ","; // I also use space

			{
				uint ui = uint.MaxValue;
				ui++;
				Console.WriteLine($"{uint.MaxValue.ToString("N0", nfi)} + 1 = {ui} (no overflow exception was thrown)");
			}

			checked {
				try {
					uint ui = uint.MaxValue;
					ui++; // will throw Unhandled exception. System.OverflowException: Arithmetic operation resulted in an overflow.
					Console.WriteLine($"ui = {ui}");
				} catch (System.OverflowException e) {
					Console.WriteLine($"{uint.MaxValue.ToString("N0", nfi)} + 1 = An overflow exception was thrown");
				}
			}
		}

		static unsafe void DisplaySizeof<T>() where T : unmanaged 
		{ // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/unmanaged-types
			Console.WriteLine($"the size of a {typeof(T)} is {sizeof(T)}");
		}
	}
}
