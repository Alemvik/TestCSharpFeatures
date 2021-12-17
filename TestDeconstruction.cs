using System;

namespace TestDeconstruction {
	static class Tester {
		public static void Go()
		{
			var msg = "TestDeconstruction";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			var (titre, autheur, isbn) = new Book("Barbara Hambly", "Dragonsbane","12345");

			Console.WriteLine($"titre/autheur/isbn={titre}/{autheur}/{isbn}");
		}
	}

	public record Book (string Author, string Title, string Isbn);
}
