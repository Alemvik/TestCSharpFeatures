using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Alemvik;

// https://github.com/CarterCommunity/Carter
namespace Test {
	public class MinimalApi { // https://www.youtube.com/watch?v=wvpuyQANHog
		public MinimalApi() 
		{
			var msg = "Test Minimal API";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			var webHost = new WebHostBuilder()
				.UseKestrel()
				.ConfigureServices(services => {
					services.AddCarter();
				})
				.Configure(app => {
					app.UseRouting();
					app.UseEndpoints(UriBuilder => UriBuilder.MapCarter());
				})
				.Build();

			webHost.Run();
		}
	}

	public class ApiMethods : CarterModule
	{
		public ApiMethods()
		{
			Get("/hello", async (request, response) => {
				await response.WriteAsync(@"<html>
					<head>
						<title>MinApi</title>
						<style>
							h1 {
								color: red;
							}
						</style>
					</head>
					<body>
						<h1>Hello world !</h1>
					</body>
				</html>");
			});

			Get("/donut", async (request, response) => {
				await response.WriteAsync(System.IO.File.ReadAllText(@"Donut.html"));
			});
		}
	}
}