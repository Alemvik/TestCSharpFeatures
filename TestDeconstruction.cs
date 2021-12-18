using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDeconstruction;

static class Tester {
	public static void Go()
	{
		var msg = "TestDeconstruction";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

		var (name,age) = new Dictionary<string, ushort>{{"Alain Trépanier",54},{"Évie Dutel",13}}.First();

		var (titre, autheur, isbn) = new Book("Barbara Hambly", "Dragonsbane","0-345-31572-3");
		var (t, _, _) = new Book("Conan the Bold", "John Maddox Roberts","0-8125-5210-5");
		var (n, b) = new Dog {Name="Togo", BirthDay = new DateOnly(1913,10,17) };


		Console.WriteLine($"name/age = {name}/{age}\ntitre/autheur/isbn = {titre}/{autheur}/{isbn}\nt = {t}\nn/b = {n}/{b:yyyy-MM-dd}");
	}
}

record Book (string Author, string Title, string Isbn);

class Dog {
	public string Name {get; init;} = default!; // The null-forgiving operator has no effect at run time. It only affects the compiler's static flow analysis by changing the null state of the expression. At run time, expression x! evaluates to the result of the underlying expression x.
	public DateOnly BirthDay {get; init;}

	public void Deconstruct(out string name, out DateOnly birthDay) {
		name = Name;
		birthDay = BirthDay;
	}
}

