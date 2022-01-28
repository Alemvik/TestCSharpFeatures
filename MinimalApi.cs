using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

// https://www.youtube.com/watch?v=NtFM-sK6xAo
// https://gist.github.com/davidfowl/ff1addd02d239d2d26f4648a06158727
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0
namespace Test;

public class MinimalApi { public MinimalApi() {
	var app = WebApplication.Create(); // uses Kestrel endpoints in appsettings.json

	var msg = $"Test Minimal API (EnvironmentName={app.Environment.EnvironmentName})"; // For VSCode see .vscode/launch.json. For Visual Studio go to Project > Properties > Debug > Environment Variables:
	Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

	//app.Environment.EnvironmentName = "Development";
	//app.Environment.ApplicationName;
	//app.Environment.EnvironmentName = Environment.Staging;

	//app.Urls.Add("http://localhost:3000");
	//app.Urls.Add("http://localhost:4000");

	app.MapGet("/", () => $"Hello World!");

	app.MapGet("/donut", async (HttpRequest request, HttpResponse response) => {
		await response.WriteAsync(System.IO.File.ReadAllText("Donut.html"));
	});

	//app.UseWelcomePage(

	app.Run();
}}
