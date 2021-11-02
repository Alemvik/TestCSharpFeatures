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

namespace Test {
	public class MinimalApi { // https://www.youtube.com/watch?v=wvpuyQANHog
		public MinimalApi() 
		{
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
			var msg = "Test Minimal API";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			Get("/hello", async (request, response) => {
				await response.WriteAsync("Hello world !");
			});
		}
	}
}