using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

// https://docs.microsoft.com/en-us/dotnet/csharp/distinguish-delegates-events
// https://docs.microsoft.com/en-us/dotnet/standard/events/

namespace TestEvents;
public static class DelegateExtensions
{
    public static Task InvokeAsync<TArgs>(this Func<object, TArgs, Task> func, object sender, TArgs e)
    {
        return func == null ? Task.CompletedTask
            : Task.WhenAll(func.GetInvocationList().Cast<Func<object, TArgs, Task>>().Select(f => f(sender, e)));
    }
}

public class Person : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	// Simple case
	private string firstName = "Évie";
	public string FirstName {
		get => firstName;
		set {
			if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("First name must not be blank");
			if (value != firstName) {
				firstName = value;
				PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(FirstName)));
			}
		}
	}

	// Les simple case using a common SetField()
	private string lastName = "Dutel";
	public string LastName
	{
		get { return lastName; }
		set { SetField(ref lastName, value, () => LastName); }
	}

	protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
	{
		if (selectorExpression == null) throw new ArgumentNullException(nameof(selectorExpression));
		if (selectorExpression.Body is not MemberExpression body) throw new ArgumentException("The body must be a member expression");
		OnPropertyChanged(body.Member.Name);
	}

	protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(selectorExpression);
		return true;
	}
}

static class Tester {
	static event Func<object, EventArgs, Task> MyAsyncEvent;

	public static async Task Go()
	{
		var msg = "TestEvents";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

		Person p = new() {
			FirstName = "Émie"
		};
		p.PropertyChanged += Person_PropertyChanged;
		p.PropertyChanged += Person_PropertyChanged; // twice the fun
		p.FirstName = "Alain";
		p.PropertyChanged -= Person_PropertyChanged; // no more twice the fun
		p.LastName = "Trépanier";

		// Example 2
		MyAsyncEvent += SomeAsyncMethodA;
		MyAsyncEvent += SomeAsyncMethodB;

		await MyAsyncEvent.InvokeAsync(null, EventArgs.Empty);

		Console.WriteLine("Invoked");
	}

    static void Person_PropertyChanged(Object person, EventArgs e)
    {
		Person p = person as Person;
		PropertyChangedEventArgs ea = e as PropertyChangedEventArgs;
		Console.WriteLine($"FirstName={p.FirstName}, lastName={p.LastName}, The property that just changed is {ea.PropertyName}");
    }

    static async Task SomeAsyncMethodA(object src, EventArgs args)
    {
        Console.WriteLine("Task A...");
        await Task.Delay(2000);
        Console.WriteLine("Task A finished");
    }

    static async Task SomeAsyncMethodB(object src, EventArgs args)
    {
        Console.WriteLine("Task B...");
        await Task.Delay(1000);
        Console.WriteLine("Task B finished");
    }
}